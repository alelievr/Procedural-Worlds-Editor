using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using UnityEditor;
using UnityEngine.Profiling;

//Ordering group rendering for PWGraphEditor
public partial class PWGraphEditor
{
    
	void CreateNewOrderingGroup(object pos)
	{
		PWOrderingGroup	newOrderingGroup = new PWOrderingGroup();

		newOrderingGroup.Initialize((Vector2)pos);
		
	    graph.orderingGroups.Add(newOrderingGroup);
	}

	void DeleteOrderingGroup()
	{
		if (editorEvents.mouseOverOrderingGroup != null)
			graph.orderingGroups.Remove(editorEvents.mouseOverOrderingGroup);
	}

	void RenderOrderingGroups()
	{
		Profiler.BeginSample("[PW] Render ordering groups");

		foreach (var orderingGroup in graph.orderingGroups)
			orderingGroup.Render(graph.panPosition, position.size * (1 / graph.scale), ref graph.editorEvents);

		//if the mouse was not over an ordering group this frame
		if (!editorEvents.isMouseOverOrderingGroupFrame)
			editorEvents.mouseOverOrderingGroup = null;

		Profiler.EndSample();
	}

	bool MaskEvents()
	{
		restoreEvent = false;
		savedEventType = e.type;
		
		//check if we have an event outside of the graph event masks
		if (e.type == EventType.MouseDown || e.type == EventType.ContextClick || e.isKey || e.isScrollWheel)
		{
			foreach (var eventMask in eventMasks)
			{
				if (eventMask.Value.Contains(e.mousePosition))
				{
					//if there is, we say to ignore the event and restore it later
					restoreEvent = true;
					e.type = EventType.Ignore;
					return true;
				}
			}
		}

		return false;
	}

	void UnMaskEvents()
	{
		if (restoreEvent)
			e.type = savedEventType;
	}
	
	void ManageEvents()
	{
		Profiler.BeginSample("[PW] Managing events");

		//do not process events if we are in layout / repaint
		if (e.type == EventType.Repaint || e.type == EventType.Layout)
			return ;

		//we save with the s key
		if (e.type == EventType.KeyDown && e.keyCode == KeyCode.S)
		{
			AssetDatabase.SaveAssets();
			e.Use();
		}

		//begin to darg a link if clicked on anchor and nothing else is started
		if (editorEvents.isMouseClickOnAnchor && !editorEvents.isPanning && !editorEvents.isDraggingSomething)
			StartDragLink();
		
		//click up outside of an anchor, stop dragging
		if (e.type == EventType.MouseUp && editorEvents.isDraggingLink)
			StopDragLink(false);
		
		//duplicate selected items if cmd+d
		if (commandOSKey && e.keyCode == KeyCode.D && e.type == EventType.KeyDown)
		{
			graph.nodes.ForEach(n => n.Duplicate());

			e.Use();
		}

		//graph panning
		//if the event is a drag then it has't been used before
		if (e.type == EventType.MouseDown && !editorEvents.isDraggingSomething)
		{
			editorEvents.isPanning = true;
		}
		
		if (editorEvents.isPanning)
		{
			//mouse middle button or left click + cmd on mac and left click + control on other OS
			if (e.button == 2 || (e.button == 0 && commandOSKey))
				graph.panPosition += e.delta;
		}
		
		//Graph selection start event 
		if (e.type == EventType.MouseDown) //if event is mouse down
		{
			if (!editorEvents.isMouseOverSomething //if mouse is not above something
				&& e.button == 0
				&& e.modifiers == EventModifiers.None)
			{
				editorEvents.selectionStartPoint = e.mousePosition;
				editorEvents.isSelecting = true;
			}
		}

		if (editorEvents.isPanning)
			Undo.RecordObject(graph, "graph pan");
		if (editorEvents.isDraggingOrderingGroup)
			Undo.RecordObject(graph, "ordering graph drag");
		if (GUI.changed)
			Undo.RecordObject(graph, "something changed");
			
		//on mouse button up
		if (e.rawType == EventType.MouseUp)
		{
			editorEvents.isDraggingOrderingGroup = false;
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
			editorEvents.isZooming = true;
			graph.scale *= 1 - (e.delta.y / 40f);
			graph.scale = Mathf.Clamp(graph.scale, .15f, 3);
		}

		//undo and redo
		if (commandOSKey && e.type == EventType.KeyDown)
		{
			if (e.keyCode == KeyCode.Z)
			{
				Undo.PerformUndo();
				e.Use();
			}
			if ((e.keyCode == KeyCode.Z && e.shift) || e.keyCode == KeyCode.Y)
			{
				Undo.PerformRedo();
				e.Use();
			}
		}
		
		//must be placed at the end of the function
		//unselect all selected links and raise an event for nodes if click beside.
		if (e.type == EventType.MouseDown
				&& !editorEvents.isMouseOverAnchor
				&& !editorEvents.isMouseOverNode
				&& !editorEvents.isMouseOverLink
				&& !editorEvents.isMouseOverOrderingGroup)
		{
			graph.RaiseOnClickNowhere();

			UnselectAllLinks();
		}

		Profiler.EndSample();
	}
}
