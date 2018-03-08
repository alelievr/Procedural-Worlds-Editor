using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;
using System.Linq;
using PW;

//Event management for PWGraphs editor
public partial class PWGraphEditor
{

	bool MaskEvents()
	{
		restoreEvent = false;
		savedEventType = e.type;
		
		//check if we have an event outside of the graph event masks
		if (e.type == EventType.MouseDown || e.type == EventType.ContextClick || e.isKey || e.isScrollWheel)
		{
			foreach (var eventMask in eventMasks)
			{
				if (eventMask.Contains(e.mousePosition))
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

		if (editorEvents.isDraggingOrderingGroup)
			Undo.RecordObject(graph, "ordering group drag");
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
		{
			if (OnWindowResize != null)
				OnWindowResize(windowSize);
			Repaint();
			windowSize = position.size;
		}
		
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
		
		//reset current layout:
		if (e.type == EventType.KeyDown && e.keyCode == KeyCode.R && e.shift)
		{
			ResetLayout();
			e.Use();
		}

		CommandEvents();
		
		//must be placed at the end of the function
		//unselect all selected links and raise an event for nodes if click beside.
		if (e.type == EventType.MouseDown
				&& !editorEvents.isMouseOverAnchor
				&& !editorEvents.isMouseOverNode
				&& !editorEvents.isMouseOverLink
				&& !editorEvents.isMouseOverOrderingGroup)
		{
			if (OnClickNowhere != null)
				OnClickNowhere();

			UnselectAllLinks();
		}

		Profiler.EndSample();
	}

	void CommandEvents()
	{
		if (e.type != EventType.ValidateCommand)
			return ;
			
		var selectedNodes = graph.allNodes.Where(n => n.isSelected).ToList();
		
		switch (e.commandName)
		{
			case "Duplicate":
				foreach (var node in selectedNodes)
					node.Duplicate();
				break ;
			case "Delete":
				var selectedLinks = graph.nodeLinkTable.GetLinks().Where(l => l.selected);

				foreach (var node in selectedNodes)
					node.RemoveSelf();
				
				foreach (var link in selectedLinks)
					graph.RemoveLink(link);
				break ;
			case "Cut":
				break ;
			case "Copy":
				break ;
			case "Paste":
				break ;
			case "FrameSelected":
				var selectedNode = graph.allNodes.FirstOrDefault(n => n.isSelected);

				if (selectedNode != null)
					graph.panPosition = -selectedNode.rect.position + windowSize / 2 - selectedNode.rect.size / 2;
				break ;
			case "Find":
				break ;
			case "SelectAll":
				break ;
		}
	}

	public void RaiseNodeSelected(PWNode node)
	{
		if (OnNodeSelected != null)
			OnNodeSelected(node);
	}

	public void RaiseNodeUnselected(PWNode node)
	{
		if (OnNodeUnselected != null)
			OnNodeUnselected(node);
	}

	public void Reload()
	{
		if (OnForceReload != null)
			OnForceReload();
		
		graph.Process();
	}

	public void GraphPostProcessCallback()
	{
		//send preProcess event
		foreach (var nodeEditorKP in nodeEditors)
			nodeEditorKP.Value.OnNodePreProcess();
	}

	public void GraphPreProcessCallback()
	{
		//send postProcess event
		foreach (var nodeEditorKP in nodeEditors)
			nodeEditorKP.Value.OnNodePostProcess();
	}

	public void ReloadOnce()
	{
		if (OnForceReloadOnce != null)
			OnForceReloadOnce();
			
		graph.ProcessOnce();
	}
}
