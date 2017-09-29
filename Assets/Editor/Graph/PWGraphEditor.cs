using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using PW;
using PW.Core;
using PW.Node;

using Debug = UnityEngine.Debug;

[System.Serializable]
public partial class PWGraphEditor : PWEditorWindow {

	//the reference to the graph in public for the AssetHandlers class
	public PWGraph				graph;

	//event masks, zones where the graph will not process events,
	//useful when you want to add a panel on the top of the graph.
	public List< Rect >			eventMasks = new List< Rect >();
	EventType					savedEventType;
	bool						restoreEvent;
	
	protected PWGraphEditorEventInfo editorEvents { get { return graph.editorEvents; } }

	//custom editor events:
	public event Action< Vector2 >	OnWindowResize;
	
	//current Event:
	Event						e;
	
	protected Vector2			windowSize;

	//Is the editor on MacOS ?
	bool 						MacOS;

	public override void OnEnable()
	{
		base.OnEnable();

		MacOS = SystemInfo.operatingSystem.Contains("Mac");

		LoadAssets();
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
			//TODO: rework this
			Debug.Log("NULL graph !");
			return ;
		}

		e = Event.current;
		
		//set the skin for the current window
		GUI.skin = PWGUISkin;

		editorEvents.Reset();

		EditorGUIExtension.BeginZoomArea(graph.scale, position);
		{
			//disable events if mouse is above an eventMask Rect.
			//TODO: test this
			if (MaskEvents())
				return ;
	
			//draw the background:
			RenderBackground();
	
			//manage selection:
			SelectAndDrag();
	
			//graph rendering
			EditorGUILayout.BeginHorizontal(); //is it useful ?
			{
				RenderOrderingGroups();
				RenderNodes();
			}
			EditorGUILayout.EndHorizontal();
	
			ContextMenu();
	
			//fill and process remaining events if there is
			ManageEvents();
	
			//restore masked events:
			UnMaskEvents();
	
			//reset to default the depth
			GUI.depth = 0;
	
			//TODO: fix ?
			if (e.type == EventType.Repaint)
				Repaint();
		}
		EditorGUIExtension.EndZoomArea();
		
		//save the size of the window
		windowSize = position.size;
	}

	//TODO: move elsewhere
	public void LoadGraph(string file)
	{
		LoadGraph(AssetDatabase.LoadAssetAtPath< PWGraph >(file));
	}

	public void LoadGraph(PWGraph graph)
	{
		Debug.Log("graph initi: " + graph.initialized);
		Debug.Log("Loading graph: " + graph.inputNode + "/" + graph.outputNode);
		this.graph = graph;
		
		graph.OnNodeAdded += OnNodeAddedCallback;
		graph.OnNodeRemoved += OnNodeRemovedCallback;

		//set the skin for the node style initialization
		GUI.skin = PWGUISkin;

		if (!graph.initialized)
		{
			graph.Initialize();
			graph.OnEnable();
			EditorUtility.SetDirty(graph);
			AssetDatabase.SaveAssets();
		}
		Debug.Log("graph initi: " + graph.initialized);
	}

	public void UnloadGraph()
	{
		graph.OnNodeAdded -= OnNodeAddedCallback;
		graph.OnNodeRemoved -= OnNodeRemovedCallback;

		Resources.UnloadAsset(graph);
	}

	public override void OnDisable()
	{
		base.OnDisable();

		//destroy the graph so it's not loaded in the void.
		if (graph != null)
			UnloadGraph();
	}

	bool MaskEvents()
	{
		restoreEvent = false;
		savedEventType = e.type;
		
		//check if we have an event outside of the graph event masks
		if (e.isMouse || e.isKey || e.isScrollWheel)
		{
			foreach (var eventMask in eventMasks)
				if (eventMask.Contains(e.mousePosition))
				{
					//if there is, we say to ignore the event and restore it later
					restoreEvent = true;
					e.type = EventType.Ignore;
					return true;
				}
		}
		return false;
	}

	void RenderBackground()
	{
		float	scale = 2f;
		
		GUI.DrawTextureWithTexCoords(
			new Rect(graph.panPosition.x % 128 - 128, graph.panPosition.y % 128 - 128, maxSize.x, maxSize.y),
			nodeEditorBackgroundTexture, new Rect(0, 0, (maxSize.x / nodeEditorBackgroundTexture.width) * scale,
			(maxSize.y / nodeEditorBackgroundTexture.height) * scale)
		);
	}

	void SelectAndDrag()
	{
		//rendering the selection rect
		if (editorEvents.isSelecting)
		{
			Rect posiviteSelectionRect = PWUtils.CreateRect(e.mousePosition, editorEvents.selectionStartPoint);
			Rect decaledSelectionRect = PWUtils.DecalRect(posiviteSelectionRect, -graph.panPosition);

			//draw selection rect
			if (e.type == EventType.Repaint)
				selectionStyle.Draw(posiviteSelectionRect, false, false, false, false);

			//iterature throw all nodes of the graph and check if the selection overlaps
			graph.nodes.ForEach(n => n.isSelected = decaledSelectionRect.Overlaps(n.windowRect));
			editorEvents.selectedNodeCount = graph.nodes.Count(n => n.isSelected);
		}

		//multiple window drag:
		if (e.type == EventType.MouseDrag && editorEvents.isDraggingSelectedNodes)
		{
				graph.nodes.ForEach(n => {
				if (n.isSelected)
					n.windowRect.position += e.delta;
				});
		}
	}

	void RenderOrderingGroups()
	{
		foreach (var orderingGroup in graph.orderingGroups)
			orderingGroup.Render(graph.panPosition, position.size, ref graph.editorEvents);
	}

	void RenderNodes()
	{
		int		nodeId = 0;
		
		BeginWindows();
		{
			foreach (var node in graph.nodes)
			{
				RenderNode(nodeId++, node);
			}
	
			//display the graph input and output:
			RenderNode(nodeId++, graph.outputNode);
			RenderNode(nodeId++, graph.inputNode);
		}
		EndWindows();
	}

	void ManageEvents()
	{
		//we save with the s key
		if (e.type == EventType.KeyDown && e.keyCode == KeyCode.S)
		{
			e.Use();
			AssetDatabase.SaveAssets();
		}
		
		//click up outside of an anchor, stop dragging
		if (e.type == EventType.mouseUp && editorEvents.isDraggingLink)
			StopDragLink(false);
			
		//duplicate selected items if cmd+d
		if (e.command && e.keyCode == KeyCode.D && e.type == EventType.KeyDown)
		{
			graph.nodes.ForEach(n => n.Duplicate());

			e.Use();
		}

		//graph panning
		//if the event is a drag then it has't been used before
		if (e.type == EventType.mouseDrag && !editorEvents.isDraggingSomething)
		{
			//mouse middle button or left click + cmd on mac and left click + control on other OS
			if (e.button == 2 || (e.button == 0 && ((MacOS && e.command) || (!MacOS && e.control))))
			{
				editorEvents.isPanning = true;
				graph.panPosition += e.delta;
			}
		}
		
		//Graph selection start event 
		if (e.type == EventType.MouseDown) //if event is mouse down
		{
			if (!editorEvents.isMouseOverSomething //if mouse is not above something
				&& e.button == 0
				&& !e.command
				&& !e.control)
			{
				editorEvents.selectionStartPoint = e.mousePosition;
				editorEvents.isSelecting = true;
			}
		}

		//on mouse button up
		if (e.type == EventType.MouseUp)
		{
			editorEvents.isSelecting = false;
			editorEvents.isPanning = false;
			editorEvents.isDraggingSelectedNodes = false;
		}
		
		//esc key event:
		if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
		{
			if (editorEvents.isDraggingLink)
				StopDragLink(false);
			editorEvents.isSelecting = false;
			editorEvents.isDraggingLink = false;
			editorEvents.isDraggingNewLink = false;
		}
		
		//fire the resize event
		if (windowSize != Vector2.zero && windowSize != position.size)
			if (OnWindowResize != null)
				OnWindowResize(position.size);
		
		//zoom
		if (e.type == EventType.ScrollWheel)
		{
			graph.scale *= 1 - (e.delta.y / 100f);
			graph.scale = Mathf.Clamp(graph.scale, .2f, 2);
		}
	}

	void UnMaskEvents()
	{
		if (restoreEvent)
			e.type = savedEventType;
	}

	void DrawNodeGraphCore()
	{
		Event		e = Event.current;

		Rect snappedToAnchorMouseRect = new Rect((int)e.mousePosition.x, (int)e.mousePosition.y, 0, 0);
	
		//unselect all selected links if click beside.
		if (e.type == EventType.MouseDown
				&& !editorEvents.isMouseOverAnchor
				&& !editorEvents.isMouseOverNode
				&& !editorEvents.isMouseOverLink
				&& !editorEvents.isMouseOverOrderingGroup)
			graph.RaiseOnClickNowhere();
	}
}
