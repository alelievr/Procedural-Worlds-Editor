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
	
	//size of the current window, updated each frame
	protected Vector2			windowSize;

	//Is the editor on MacOS ?
	bool 						MacOS;
	//Is the command (on MacOs) or control (on other OSs) is pressed
	bool						commandOSKey { get { return (MacOS && e.command) || (!MacOS && e.control); } }

	//Layout additional windows
	protected PWGraphOptionBar			optionBar;
	protected PWGraphNodeSelectorBar	nodeSelectorBar;
	protected PWGraphSettingsBar		settingsBar;


	//custom editor events:
	//fired whe the user resize the window (old window size in parameter)
	public event Action< Vector2 >	OnWindowResize;
	//fired when a graph is loaded/unloaded
	public event Action< PWGraph >	OnGraphChanged;

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

		Profiler.EndSample();
	}

	public override void OnGUIEnable()
	{
		LoadStyles();

		if (graph != null)
			LoadGraph(graph);
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
		graph.PWGUI.StartFrame(position);
		
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
		
		//save the size of the window
		windowSize = position.size;
	}

	void PlayModeChangeCallback(PlayModeStateChange mode)
	{
		// if (mode == PlayModeStateChange.EnteredEditMode)
			// graph.Process();
	}

	void UndoRedoCallback()
	{
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

		//update graph in views:
		optionBar = new PWGraphOptionBar(graph);
		nodeSelectorBar = new PWGraphNodeSelectorBar(graph);
		settingsBar = new PWGraphSettingsBar(graph);
		optionBar.LoadStyles();
		nodeSelectorBar.LoadStyles();
		settingsBar.LoadStyles();

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
