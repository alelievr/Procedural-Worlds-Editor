using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;
using PW.Core;
using PW;

public partial class PWGraphEditor
{
	//process link creation, drag and select events + draw links
	void RenderLinks()
	{
		Profiler.BeginSample("[PW] render links");

		//render the dragged link
		if (editorEvents.isDraggingLink || editorEvents.isDraggingNewLink)
			DrawNodeCurve(editorEvents.startedLinkAnchor, e.mousePosition);

		//render node links
		foreach (var node in graph.nodes)
			RenderNodeLinks(node);
		
		if (!editorEvents.isMouseOverLinkFrame)
			editorEvents.mouseOverLink = null;

		Profiler.EndSample();
	}

	void RenderNodeLinks(PWNode node)
	{
		Handles.BeginGUI();
		foreach (var anchorField in node.outputAnchorFields)
			foreach (var anchor in anchorField.anchors)
				foreach (var link in anchor.links)
					DrawNodeCurve(link);
		Handles.EndGUI();
	}
	
	void DrawNodeCurve(PWAnchor anchor, Vector2 endPoint, bool anchorSnapping = true)
	{
		Rect anchorRect = anchor.rectInGraph;
		Vector3 startPos = new Vector3(anchorRect.x + anchorRect.width, anchorRect.y + anchorRect.height / 2, 0);
		Vector3 startDir = Vector3.right;

		if (anchorSnapping && editorEvents.isMouseOverAnchor)
		{
			var toAnchor = editorEvents.mouseOverAnchor;
			var fromAnchor = editorEvents.startedLinkAnchor;

			if (PWAnchorUtils.AnchorAreAssignable(fromAnchor, toAnchor))
				endPoint = toAnchor.rectInGraph.center;
		}

		float tanPower = (startPos - (Vector3)endPoint).magnitude / 2;
		tanPower = Mathf.Clamp(tanPower, 0, 100);

		DrawSelectedBezier(startPos, endPoint, startPos + startDir * tanPower, (Vector3)endPoint + -startDir * tanPower, anchor.colorSchemeName, 4, PWLinkHighlight.None);
	}
	
	void DrawNodeCurve(PWNodeLink link)
	{
		if (link == null)
		{
			Debug.LogError("[PWGraphEditor] attempt to draw null link !");
			return ;
		}
		if (link.fromAnchor == null || link.toAnchor == null)
		{
			Debug.LogError("[PWGraphEditor] null anchors in a the link: " + link);
			return ;
		}

		Event e = Event.current;

		link.controlId = GUIUtility.GetControlID(FocusType.Passive);

		Rect start = link.fromAnchor.rectInGraph;
		Rect end = link.toAnchor.rectInGraph;

		Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
		Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);

		Vector3 startDir = Vector3.right;
		Vector3 endDir = Vector3.left;
		
		float tanPower = (startPos - endPos).magnitude / 2;
		tanPower = Mathf.Clamp(tanPower, 0, 100);

		Vector3 startTan = startPos + startDir * tanPower;
		Vector3 endTan = endPos + endDir * tanPower;

		if (e.type == EventType.MouseDown && !editorEvents.isMouseOverAnchor)
		{
			if (HandleUtility.nearestControl == link.controlId && e.button == 0)
			{
				GUIUtility.hotControl = link.controlId;

				PWNodeLink oldSelectedLink = graph.nodeLinkTable.GetLinks().FirstOrDefault(l => l.selected);

				if (oldSelectedLink != null && OnLinkUnselected != null)
					OnLinkUnselected(oldSelectedLink);

				//unselect all others links:
				UnselectAllLinks();
				UnselectAllNodes();

				link.selected = true;
				link.highlight = PWLinkHighlight.Selected;

				if (OnLinkSelected != null)
					OnLinkSelected(link);
				
				e.Use();
			}
		}

		//mouse over bezier curve:
		if (HandleUtility.nearestControl == link.controlId)
		{
			editorEvents.mouseOverLink = link;
			editorEvents.isMouseOverLinkFrame = true;
		}

		if (e.type == EventType.Repaint)
		{
			DrawSelectedBezier(startPos, endPos, startTan, endTan, link.colorSchemeName, 4, link.highlight);

			if (link != null && link.highlight == PWLinkHighlight.DeleteAndReset)
				link.highlight = PWLinkHighlight.None;
			
			if (link != null && !link.selected && link.highlight == PWLinkHighlight.Selected)
				link.highlight = PWLinkHighlight.None;
		}
		else if (e.type == EventType.Layout)
		{
			float bezierDistance = HandleUtility.DistancePointBezier(e.mousePosition, startPos, endPos, startTan, endTan);
			HandleUtility.AddControl(link.controlId, bezierDistance);
		}
	}

	void	DrawSelectedBezier(Vector3 startPos, Vector3 endPos, Vector3 startTan, Vector3 endTan, PWColorSchemeName colorSchemeName, int width, PWLinkHighlight highlight)
	{
		switch (highlight)
		{
			case PWLinkHighlight.Selected:
				Handles.DrawBezier(startPos, endPos, startTan, endTan, PWColorTheme.selectedColor, null, width + 3);
					break ;
			case PWLinkHighlight.Delete:
			case PWLinkHighlight.DeleteAndReset:
				Handles.DrawBezier(startPos, endPos, startTan, endTan, PWColorTheme.deletedColor, null, width + 2);
				break ;
		}
		Color c = PWColorTheme.GetLinkColor(colorSchemeName);
		Handles.DrawBezier(startPos, endPos, startTan, endTan, c, null, width);
	}
	
	// string undoName = "Link created";

	void	LinkCreatedCallback(PWNodeLink link)
	{
		if (editorEvents.isDraggingLink || editorEvents.isDraggingNewLink)
			StopDragLink(true);
			
		//Currently causes weird bugs
		
		// if (link.toNode == null || link.fromNode == null)
		// 	return ;

		// Debug.Log("register !");
		
		// Undo.RecordObject(graph, undoName);
	}

	void	PostLinkCreatedCallback(PWNodeLink link)
	{
		// if (link.toNode == null || link.fromNode == null)
		// 	return ;

		// Debug.Log("register !");
		
		// Undo.RecordObject(link.toNode, undoName);
		// Undo.RecordObject(link.fromNode, undoName);
		// Undo.RecordObject(graph, undoName);
	}

	void	LinkRemovedCallback(PWNodeLink link)
	{
		if (link.toNode == null || link.fromNode == null)
			return ;

		//Currently causes weird bugs

		// string undoName = "Link removed";
		
		// Undo.RecordObject(link.toNode, undoName);
		// Undo.RecordObject(link.fromNode, undoName);
		// Undo.RecordObject(graph, undoName);
	}

	void	UnselectAllLinks()
	{
		foreach (var l in graph.nodeLinkTable.GetLinks())
		{
			l.selected = false;
			l.highlight = PWLinkHighlight.None;
		}
	}
	
	void StartDragLink()
	{
		Undo.RecordObject(graph, "Link started");
		graph.editorEvents.startedLinkAnchor = editorEvents.mouseOverAnchor;
		graph.editorEvents.isDraggingLink = true;
		
		if (OnLinkStartDragged != null)
			OnLinkStartDragged(editorEvents.startedLinkAnchor);
	}

	void StopDragLink(bool linked)
	{
		Debug.Log("linked: " + linked);
		graph.editorEvents.isDraggingLink = false;

		if (!linked && OnLinkCanceled != null)
			OnLinkCanceled();
		
		if (OnLinkStopDragged != null)
			OnLinkStopDragged();
	}

	void DeleteAllAnchorLinks()
	{
		editorEvents.mouseOverAnchor.RemoveAllLinks();
	}
}
