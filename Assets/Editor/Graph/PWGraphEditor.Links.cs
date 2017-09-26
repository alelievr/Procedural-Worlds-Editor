using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Node;
using UnityEditor;
using System;

//Links between nodes rendering
public partial class PWGraphEditor {

	const float	tanPower = 50;

	//TODO: mode this in PWNode.Links
	void DrawNodeCurve(PWAnchor anchor, Vector2 endPoint)
	{
		Vector3 startPos = new Vector3(anchor.rect.x + anchor.rect.width, anchor.rect.y + anchor.rect.height / 2, 0);
		DrawSelectedBezier(startPos, endPoint, startPos + Vector3.right * tanPower, Vector3.zero, anchor.colorPalette, 4, PWLinkHighlight.None);
	}
	
    void DrawNodeCurve(PWNodeLink link)
    {
		if (link == null)
		{
			Debug.LogError("[PWGraphEditor] attempt to draw null link !");
			return ;
		}

		Event e = Event.current;

		if (link.controlId == -1)
			link.controlId = GUIUtility.GetControlID(FocusType.Passive);

		//TODO: integrate eventMasks

		Rect start = link.fromAnchor.rect;
		Rect end = link.toAnchor.rect;
        Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
		
		Vector3 startDir = Vector3.right;
		Vector3 endDir = Vector3.left;

        Vector3 startTan = startPos + startDir * tanPower;
        Vector3 endTan = endPos + endDir * tanPower;

		switch (e.GetTypeForControl(link.controlId))
		{
			case EventType.MouseDown:
				if (HandleUtility.nearestControl == link.controlId && (e.button == 0 || e.button == 1))
				{
					GUIUtility.hotControl = link.controlId;
					//unselect all others links:
					graph.RaiseOnLinkSelected(link);
					link.selected = true;
					link.highlight = PWLinkHighlight.Selected;
				}
				break ;
		}
		if (HandleUtility.nearestControl == link.controlId)
		{
			Debug.Log("Bezier curve take the control ! Will it break the window focus ?");
		}

		HandleUtility.AddControl(link.controlId, HandleUtility.DistancePointBezier(e.mousePosition, startPos, endPos, startTan, endTan) / 1.5f);
		if (e.type == EventType.Repaint)
		{
			DrawSelectedBezier(startPos, endPos, startTan, endTan, link.colorPalette, 4, link.highlight);

			if (link != null && link.highlight == PWLinkHighlight.DeleteAndReset)
				link.highlight = PWLinkHighlight.None;
			
			if (link != null && !link.selected && link.highlight == PWLinkHighlight.Selected)
				link.highlight = PWLinkHighlight.None;
		}
    }

	void	DrawSelectedBezier(Vector3 startPos, Vector3 endPos, Vector3 startTan, Vector3 endTan, PWColorPalette colorPalette, int width, PWLinkHighlight highlight)
	{
		switch (highlight)
		{
			case PWLinkHighlight.Selected:
				Handles.DrawBezier(startPos, endPos, startTan, endTan, PWColorScheme.GetColor("selectedNode"), null, width + 3);
;				break ;
			case PWLinkHighlight.Delete:
			case PWLinkHighlight.DeleteAndReset:
				Handles.DrawBezier(startPos, endPos, startTan, endTan, new Color(1f, .0f, .0f, 1), null, width + 2);
				break ;
		}
		Color c = PWColorScheme.GetLinkColor(colorPalette);
		Handles.DrawBezier(startPos, endPos, startTan, endTan, c, null, width);
	}

}
