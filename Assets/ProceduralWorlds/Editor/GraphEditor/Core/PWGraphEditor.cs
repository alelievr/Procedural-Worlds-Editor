using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using PW;
using PW.Core;
using PW.Node;
using PW.Editor;
using UnityEngine.Profiling;

using Debug = UnityEngine.Debug;

[System.Serializable]
public partial class PWGraphEditor : PWEditorWindow
{

	//the reference to the graph in public for the AssetHandlers class
	public PWGraph				graph;
	public PWBiomeGraph			biomeGraph { get { return graph as PWBiomeGraph; } }
	public PWMainGraph			mainGraph { get { return graph as PWMainGraph; } }


	//event masks, zones where the graph will not process events,
	//useful when you want to add a panel on the top of the graph.
	public Dictionary< int, Rect >	eventMasks = new Dictionary< int, Rect >();
	EventType					savedEventType;
	bool						restoreEvent;


	protected PWGraphEditorEventInfo editorEvents { get { return graph.editorEvents; } }
	protected PWGUIManager		PWGUI = new PWGUIManager();


	//size of the current window, updated each frame
	protected Vector2			windowSize;
	protected DelayedChanges	delayedChanges = new DelayedChanges();


	//Is the editor on MacOS ?
	bool 						MacOS;
	//Is the command (on MacOs) or control (on other OSs) is pressed
	bool						commandOSKey { get { return (MacOS && e.command) || (!MacOS && e.control); } }


	//Panels to load when the graph editor is opened (pannel common to eventy PWGraphEditors)
	protected List< PWGraphPanel >	panels = new List< PWGraphPanel >
	{
		new PWGraphOptionPanel(),
		new PWGraphNodeSelectorPanel(),
		new PWGraphSettingsPanel(),
	};

	//public delegates:
	public delegate void			NodeAction(PWNode node);
	public delegate void			LinkAction(PWNodeLink link);

	
	//fired whe the user resize the window (old window size in parameter)
	public event Action< Vector2 >	OnWindowResize;
	//fired when a graph is loaded/unloaded
	public event Action< PWGraph >	OnGraphChanged;
	//fired when a node is selected
	public event NodeAction			OnNodeSelected;
	//fired when a node is unselected
	public event NodeAction			OnNodeUnselected;
	//fired when fore reload button in the editor is pressed
	public event Action				OnForceReload;
	//fired when fore reload once button in the editor is pressed
	public event Action				OnForceReloadOnce;
	//fired when clicking inside the graph, not on a node nor a link.
	public event Action				OnClickNowhere;
	//fired when a link is not created and dropped in the void
	public event Action				OnLinkCanceled;
	//fired when a link start been dragged by the editor
	public event Action< PWAnchor >	OnLinkStartDragged;
	//fired when a link stop been dragged by the editor
	public event Action				OnLinkStopDragged;
	//fired when a link is selected
	public event LinkAction			OnLinkSelected;
	//fired when a link is unselected
	public event LinkAction			OnLinkUnselected;


	[System.NonSerialized]
	Type							oldGraphType = null;


	public override void OnEnable()
	{
		Profiler.BeginSample("[PW] GraphEditor Enabled");
		base.OnEnable();

		//provide the MacOS bool
		MacOS = SystemInfo.operatingSystem.Contains("Mac");

		EditorApplication.playModeStateChanged += PlayModeChangeCallback;
		Undo.undoRedoPerformed += UndoRedoCallback;

		//clear event masks
		eventMasks.Clear();

		LoadAssets();
		
		//Initialize graph panels
		foreach (var panel in panels)
			panel.Initialize(this);

		Profiler.EndSample();
	}

	public override void OnGUIEnable()
	{
		LoadStyles();
		LoadOrderingGroupStyles();

		if (graph != null)
			LoadGraph(graph);
			
		//save the size of the window
		windowSize = position.size;
	}

	//draw the default node graph:
	public override void OnGUI()
	{
		base.OnGUI();

		if (graph == null)
		{
			RenderGraphNotFound();
			return ;
		}
		
		//update the current GUI settings storage and clear drawed popup list:
		PWGUI.StartFrame(position);
		
		//set the skin for the current window
		GUI.skin = PWGUISkin;

		//protection against node class rename & corrupted nodes
		for (int i = 0; i < graph.nodes.Count; i++)
		{
			var node = graph.nodes[i];
			if (node == null)
				graph.nodes.RemoveAt(i);
			else if (node.GetType() == typeof(PWNode))
				graph.RemoveNode(node);
		}
		
		//disable events if mouse is above an eventMask Rect.
		MaskEvents();

		//profiling
		Profiler.BeginSample("[PW] Graph redering (" + e.type + ")");

		Rect pos = position;
		pos.position = Vector2.zero;
		graph.zoomPanCorrection = GUIScaleUtility.BeginScale(ref pos, pos.size / 2, 1f / graph.scale, false);
		{
			//draw the background:
			RenderBackground();
	
			//manage selection:
			SelectAndDrag();
	
			//graph rendering
			RenderOrderingGroups();
			RenderLinks();
			RenderNodes();
	
			//context menu
			ContextMenu();
	
			//fill and process remaining events if there is
			ManageEvents();

			//reset events for the next frame
			editorEvents.Reset();
	
			if (e.type == EventType.Repaint)
				Repaint();
		}
		GUIScaleUtility.EndScale();

		Profiler.EndSample();
	
		//restore masked events:
		UnMaskEvents();

		//update delayedChanges
		delayedChanges.Update();
	}

	void PlayModeChangeCallback(PlayModeStateChange mode)
	{
		// if (mode == PlayModeStateChange.EnteredEditMode)
			// graph.Process();
	}

	void UndoRedoCallback()
	{
		if (graph == null)
			return ;
		
		graph.UpdateComputeOrder();
		graph.Process();
	}

	public void LoadGraph(string assetPath)
	{
		LoadGraph(AssetDatabase.LoadAssetAtPath< PWGraph >(assetPath));
	}

	public void LoadGraph(PWGraph graph)
	{
		if (this.graph != null)
			UnloadGraph(false);

		this.oldGraphType = graph.GetType();
		this.graph = graph;

		graph.assetFilePath = AssetDatabase.GetAssetPath(graph);
		
		//attach to graph events
		graph.OnNodeAdded += NodeAddedCallback;
		graph.OnNodeRemoved += NodeRemovedCallback;
		graph.OnLinkCreated += LinkCreatedCallback;
		graph.OnLinkRemoved += LinkRemovedCallback;
		graph.OnPostLinkCreated += PostLinkCreatedCallback;
		graph.OnGraphStructureChanged += GraphStructureChangedCallback;

		//set the skin for the node style initialization
		GUI.skin = PWGUISkin;

		if (!styleLoaded)
			LoadStyles();

		if (!graph.initialized)
		{
			graph.Initialize();
			graph.OnEnable();
			SaveGraph();
		}

		if (OnGraphChanged != null)
			OnGraphChanged(graph);
		
		graph.Process();
	}

	public void UnloadGraph(bool unloadAsset = true)
	{
		graph.OnNodeAdded -= NodeAddedCallback;
		graph.OnNodeRemoved -= NodeRemovedCallback;
		graph.OnLinkCreated -= LinkCreatedCallback;
		graph.OnLinkRemoved -= LinkRemovedCallback;
		graph.OnPostLinkCreated -= PostLinkCreatedCallback;
		graph.OnGraphStructureChanged -= GraphStructureChangedCallback;

		SaveGraph();

		if (unloadAsset)
		{
			Resources.UnloadUnusedAssets();
			Resources.UnloadAsset(graph);
		}

		if (OnGraphChanged != null)
			OnGraphChanged(null);
	}

	public override void OnDisable()
	{
		base.OnDisable();

		//destroy the graph so it's not loaded in the void.
		if (graph != null)
			UnloadGraph();
		
		//destroy all instantiated node editors
		foreach (var editorKP in nodeEditors)
			DestroyImmediate(editorKP.Value);
		nodeEditors.Clear();
	}

	void SaveGraph()
	{
		EditorUtility.SetDirty(graph);
		AssetDatabase.SaveAssets();
	}

	void GraphStructureChangedCallback()
	{
		graph.UpdateComputeOrder();
		graph.Process();
	}

	void RenderBackground()
	{
		float	backgroundScale = 2f;
		int		backgroundTileSize = nodeEditorBackgroundTexture.width;
		
		Rect	position = new Rect(
			graph.panPosition.x % backgroundTileSize - backgroundTileSize,
			graph.panPosition.y % backgroundTileSize - backgroundTileSize,
			maxSize.x * 10,
			maxSize.y * 10
		);

		Rect	texCoord = new Rect(
			0,
			0,
			(maxSize.x * 10 / nodeEditorBackgroundTexture.width) * backgroundScale,
			(maxSize.y * 10 / nodeEditorBackgroundTexture.height) * backgroundScale
		);
		
		GUI.DrawTextureWithTexCoords(position, nodeEditorBackgroundTexture, texCoord);
	}

	void RenderGraphNotFound()
	{
		EditorGUILayout.LabelField("Graph not found, ouble click on a graph asset file to a graph to open it");

		if (oldGraphType != null)
		{
			PWGraph newGraph = EditorGUILayout.ObjectField(null, oldGraphType, false) as PWGraph;
	
			if (newGraph != null)
				LoadGraph(newGraph);
		}
	}

	void SelectAndDrag()
	{
		Profiler.BeginSample("[PW] Select and drag");

		//rendering the selection rect
		if (editorEvents.isSelecting)
		{
			Rect posiviteSelectionRect = PWUtils.CreateRect(e.mousePosition, editorEvents.selectionStartPoint);
			Rect decaledSelectionRect = PWUtils.DecalRect(posiviteSelectionRect, -graph.panPosition);

			//draw selection rect
			if (e.type == EventType.Repaint)
				selectionStyle.Draw(posiviteSelectionRect, false, false, false, false);

			//iterate throw all nodes of the graph and check if the selection overlaps
			graph.nodes.ForEach(n => n.isSelected = decaledSelectionRect.Overlaps(n.rect));
			editorEvents.selectedNodeCount = graph.nodes.Count(n => n.isSelected);
		}

		//multiple window drag:
		if (e.type == EventType.MouseDrag && editorEvents.isDraggingSelectedNodes)
		{
				graph.nodes.ForEach(n => {
				if (n.isSelected)
					n.rect.position += e.delta;
				});
		}

		Profiler.EndSample();
	}
}
