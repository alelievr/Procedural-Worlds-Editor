using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW;

public partial class PWGraphEditor
{

	const float	tanPower = 50;

	//process link creation, drag and select events + draw links
	void RenderLinks()
	{
		//render the dragged link
		if (editorEvents.isDraggingLink || editorEvents.isDraggingNewLink)
			DrawNodeCurve(editorEvents.startedLinkAnchor, e.mousePosition);

		//render node links
		foreach (var node in graph.nodes)
			RenderNodeLinks(node);
		
		if (!editorEvents.isMouseOverLinkFrame)
			editorEvents.mouseOverLink = null;
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

		DrawSelectedBezier(startPos, endPoint, startPos + startDir * tanPower, endPoint, anchor.colorSchemeName, 4, PWLinkHighlight.None);
	}
	
	void DrawNodeCurve(PWNodeLink link)
	{
		if (link == null)
		{
			Debug.LogError("[PWGraphEditor] attempt to draw null link !");
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

		Vector3 startTan = startPos + startDir * tanPower;
		Vector3 endTan = endPos + endDir * tanPower;

		if (e.type == EventType.mouseDown && !editorEvents.isMouseOverAnchor)
		{
			if (HandleUtility.nearestControl == link.controlId && e.button == 0)
			{
				GUIUtility.hotControl = link.controlId;

				//unselect all others links:
				UnselectAllLinks();

				link.selected = true;
				link.highlight = PWLinkHighlight.Selected;
				e.Use();
			}
		}

		//mouse over bezier curve:
		if (HandleUtility.nearestControl == link.controlId)
			editorEvents.isMouseOverLinkFrame = true;

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

	void	OnLinkCreated(PWNodeLink link)
	{
		if (editorEvents.isDraggingLink || editorEvents.isDraggingNewLink)
			StopDragLink(true);
	}

	void UnselectAllLinks()
	{
		foreach (var l in graph.nodeLinkTable.GetLinks())
		{
			l.selected = false;
			l.highlight = PWLinkHighlight.None;
		}
	}
}
