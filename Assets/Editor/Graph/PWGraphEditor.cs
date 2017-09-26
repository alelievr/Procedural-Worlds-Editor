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
public partial class PWGraphEditor : EditorWindow {

	//the reference to the graph;
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

	bool 						MacOS;

	public virtual void OnEnable()
	{
		MacOS = SystemInfo.operatingSystem.Contains("Mac");

		Debug.Log("OnEnable graph editor");

		LoadStyles();

		LoadAssets();

		graph.editorEvents.Init();
	}

	//draw the default node graph:
	public virtual void OnGUI()
	{
		e = Event.current;

		//render the graph in the background:
		GUI.depth = -10;

		graph.editorEvents.Reset();

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
			RenderLinks();
		}
		EditorGUILayout.EndHorizontal();

		ContextMenu();

		//fill and process remaining events if there is
		ManageEvents();

		//restore masked events:
		UnMaskEvents();

		//reset to default the depth
		GUI.depth = 0;
	}

	public virtual void OnDisable()
	{
		
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
		if (graph.editorEvents.isSelecting)
		{
			Rect posiviteSelectionRect = PWUtils.CreateRect(e.mousePosition, graph.editorEvents.selectionStartPoint);
			Rect decaledSelectionRect = PWUtils.DecalRect(posiviteSelectionRect, -graph.panPosition);
			selectionStyle.Draw(posiviteSelectionRect, false, false, false, false);

			//iterature throw all nodes of the graph and check if the selection overlaps
			graph.nodes.ForEach(n => n.selected = decaledSelectionRect.Overlaps(n.windowRect));
			graph.editorEvents.selectedNodeCount = graph.nodes.Count(n => n.selected);
		}

		//multiple window drag:
		if (e.type == EventType.MouseDrag && graph.editorEvents.isDraggingSelectedNodes)
		{
				graph.nodes.ForEach(n => {
				if (n.selected)
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
	
			if (graph.inputNode != null)
				RenderNode(nodeId++, graph.outputNode);
		}
		EndWindows();
	}

	void RenderLinks()
	{

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
		if (e.type == EventType.mouseUp && graph.editorEvents.isDraggingLink)
			StopDragLink(false);
			
		//duplicate selected items if cmd+d
		if (e.command && e.keyCode == KeyCode.D && e.type == EventType.KeyDown)
		{
			graph.nodes.ForEach(n => n.Duplicate());

			e.Use();
		}

		//graph panning
		//if the event is a drag then it has't been used before
		if (e.type == EventType.mouseDrag && !graph.editorEvents.isDraggingSomething)
		{
			graph.editorEvents.isPanning = true;
			graph.panPosition += e.delta;
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
				&& !graph.editorEvents.isMouseOverAnchor
				&& !graph.editorEvents.isMouseOverNode
				&& !graph.editorEvents.isMouseOverLink
				&& !graph.editorEvents.isMouseOverOrderingGroup)
			graph.RaiseOnClickNowhere();
	}
}
