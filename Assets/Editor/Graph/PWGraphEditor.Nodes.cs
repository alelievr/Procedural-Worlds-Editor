using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Node;
using PW;

//Nodes rendering
public partial class PWGraphEditor {

	void DisplayDecaledNode(int id, PWNode node, string name)
	{
		var		e = Event.current;
		bool 	Mac = SystemInfo.operatingSystem.Contains("Mac");
		
		//if you are editing the node name, hide the current name.
		if (node.windowNameEdit)
			name = "";

		if (node.isDragged && ((!Mac && e.control) || (Mac && e.command)))
		{
			Vector2 pos = node.windowRect.position;
			float	snapPixels = 25.6f;

			pos.x = Mathf.RoundToInt(Mathf.RoundToInt(pos.x / snapPixels) * snapPixels);
			pos.y = Mathf.RoundToInt(Mathf.RoundToInt(pos.y / snapPixels) * snapPixels);
			node.windowRect.position = pos;
		}
		node.UpdateGraphDecal(currentGraph.graphDecalPosition);
		node.windowRect = PWUtils.DecalRect(node.windowRect, currentGraph.graphDecalPosition);
		Rect decaledRect = GUILayout.Window(id, node.windowRect, node.OnWindowGUI, name, (node.selected) ? node.windowSelectedStyle : node.windowStyle, GUILayout.Height(node.viewHeight));
		if (node.windowRect.Contains(e.mousePosition))
			mouseAboveNode = node;
		else if (e.type == EventType.MouseDown)
			node.OnClickedOutside();
		node.windowRect = PWUtils.DecalRect(decaledRect, -currentGraph.graphDecalPosition);
	}

	void RenderNode(int id, PWNode node, string name, int index, ref bool mouseAboveAnchorLocal, ref bool mouseDraggingWindowLocal)
	{
		Event	e = Event.current;

		DisplayDecaledNode(id, node, name);

		if (node.windowRect.Contains(e.mousePosition - currentGraph.graphDecalPosition))
		{
			mouseAboveNodeIndex = index;
		}

		//highlight, hide, add all linkable anchors:
		if (draggingLink)
			node.HighlightLinkableAnchorsTo(startDragAnchor);
		node.DisplayHiddenMultipleAnchors(draggingLink);

		//process envent, state and position for node anchors:
		var mouseAboveAnchor = node.GetAnchorUnderMouse();
		if (mouseAboveAnchor.mouseAbove)
			mouseAboveAnchorLocal = true;

		if (!mouseDraggingWindowLocal)
			if (node.isDragged)
			{
				if (node.selected)
				{
					int	selectedNodeCount = 0;
	
					currentGraph.ForeachAllNodes(n => { if (n.selected) selectedNodeCount++; }, false, true);
					if (selectedNodeCount != 0)
						draggingSelectedNodes = true;
				}
				mouseDraggingWindowLocal = true;
			}

		//end dragging:
		if ((e.type == EventType.mouseUp && draggingLink == true) //standard drag start
				|| (e.type == EventType.MouseDown && draggingLink == true)) //drag started with context menu
			if (mouseAboveAnchor.mouseAbove && PWNode.AnchorAreAssignable(startDragAnchor, mouseAboveAnchor))
			{
				StopDragLink(true);

				//TODO: manage the AttachLink return values, if one of them is false, delete the link.

				//attach link to the node:
				bool linkNotRevoked = node.AttachLink(mouseAboveAnchor, startDragAnchor);

				if (linkNotRevoked)
				{
					var win = FindNodeById(startDragAnchor.nodeId);
					if (win != null)
					{
						//remove link if it was revoked.
						if (!win.AttachLink(startDragAnchor, mouseAboveAnchor))
							node.DeleteLink(mouseAboveAnchor.anchorId, win, startDragAnchor.anchorId);
						
						graphNeedReload = true;
					}
					else
						Debug.LogWarning("window id not found: " + startDragAnchor.nodeId);
					
					//Recalcul the compute order:
					EvaluateComputeOrder();
				}
			}

		if (mouseAboveAnchor.mouseAbove)
			mouseAboveAnchorInfo = mouseAboveAnchor;
			
		//if you press the mouse above an anchor, start the link drag
		if (mouseAboveAnchor.mouseAbove && e.type == EventType.MouseDown && e.button == 0)
			BeginDragLink();
		
		if (mouseAboveAnchor.mouseAbove
				&& draggingLink
				&& startDragAnchor.anchorId != mouseAboveAnchorInfo.anchorId
				&& PWNode.AnchorAreAssignable(mouseAboveAnchor, startDragAnchor))
			HighlightDeleteAnchor(mouseAboveAnchor);

		//draw links:
		var links = node.GetLinks();
		int		i = 0;
		Handles.BeginGUI();
		foreach (var link in links)
		{
			// Debug.Log("link: " + link.localNodeId + ":" + link.localAnchorId + " to " + link.distantNodeId + ":" + link.distantAnchorId);
			var fromWindow = FindNodeById(link.localNodeId);
			var toWindow = FindNodeById(link.distantNodeId);

			if (toWindow == null) //invalid window ids
			{
				node.DeleteLinkByWindowTarget(link.distantNodeId);
				Debug.LogWarning("window not found: " + link.distantNodeId);
				continue ;
			}
			Rect? fromAnchor = fromWindow.GetAnchorRect(link.localAnchorId);
			Rect? toAnchor = toWindow.GetAnchorRect(link.distantAnchorId);
			if (fromAnchor != null && toAnchor != null)
			{
				DrawNodeCurve(fromAnchor.Value, toAnchor.Value, i++, link);
				if (currentLinks.Count <= linkIndex)
					currentLinks.Add(link);
				else
					currentLinks[linkIndex] = link;
				linkIndex++;
			}
		}
		Handles.EndGUI();

		//display the process time of the window (if not 0)
		if (node.processTime > Mathf.Epsilon)
		{
			GUIStyle gs = new GUIStyle();
			Rect msRect = PWUtils.DecalRect(node.windowRect, currentGraph.graphDecalPosition);
			msRect.position += new Vector2(msRect.size.x / 2 - 10, msRect.size.y + 5);
			gs.normal.textColor = greenRedGradient.Evaluate(node.processTime / 20); //20ms ok, after is red
			GUI.Label(msRect, node.processTime + " ms", gs);
		}

		//check if user have pressed the close button of this window:
		if (node.WindowShouldClose())
			DeleteNode(index);
	}
	
	void DeleteSelectedNodes()
	{
		List< PWNode > nodeToRemove = new List< PWNode >();

		foreach (var node in graph.nodes)
			if (node.selected)
				nodeToRemove.Add(node);

		foreach (var node in nodeToRemove)
			graph.RemoveNode(node);
	}

	void MoveSelectedNodes()
	{
		Debug.Log("moving from context menu");
	}

}
