using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Node;

//Links between nodes rendering
public partial class PWGraphEditor {
	
    void DrawNodeCurve(Rect start, Rect end, int index, PWLink link)
    {
		Event e = Event.current;

		int		id;
		if (link == null)
			id = -1;
		else
			id = GUIUtility.GetControlID((link.localName + link.distantName + index).GetHashCode(), FocusType.Passive);

        Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
		
		Vector3 startDir = Vector3.right;;
		Vector3 endDir = Vector3.left;;

		float	tanPower = 50;

        Vector3 startTan = startPos + startDir * tanPower;
        Vector3 endTan = endPos + endDir * tanPower;

		if (link != null && !draggingNode && String.IsNullOrEmpty(GUI.GetNameOfFocusedControl()))
		{
			switch (e.GetTypeForControl(id))
			{
				case EventType.MouseDown:
					if (link.linkHighlight == PWLinkHighlight.Delete)
						break ;
					if (!draggingLink && HandleUtility.nearestControl == id && (e.button == 0 || e.button == 1))
					{
						GUIUtility.hotControl = id;
						//unselect all others links:
						foreach (var l in currentLinks)
							l.selected = false;
						link.selected = true;
						link.linkHighlight = PWLinkHighlight.Selected;
					}
					break ;
			}
			if (HandleUtility.nearestControl == id)
			{
				Debug.Log("bezier curve take the control !");
				GUIUtility.hotControl = id;
				link.hover = true;
			}
		}

		HandleUtility.AddControl(id, HandleUtility.DistancePointBezier(e.mousePosition, startPos, endPos, startTan, endTan) / 1.5f);
		if (e.type == EventType.Repaint)
		{
			PWLinkHighlight s = (link != null) ? (link.linkHighlight) : PWLinkHighlight.None;
			PWNodeProcessMode m = (link != null) ? link.mode : PWNodeProcessMode.AutoProcess;
			switch ((link != null) ? link.linkType : PWLinkType.BasicData)
			{
				case PWLinkType.Sampler3D:
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(.1f, .1f, .1f), 8, s, m);
					break ;
				case PWLinkType.ThreeChannel:
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(0f, 0f, 1f), 12, s, m);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(0f, 1f, 0f), 8, s, m);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(1f, 0f, 0f), 4, s, m);
					break ;
				case PWLinkType.FourChannel:
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(.1f, .1f, .1f), 16, s, m);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(0f, 0f, 1f), 12, s, m);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(0f, 1f, 0f), 8, s, m);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(1f, 0f, 0f), 4, s, m);
					break ;
				default:
					DrawSelectedBezier(startPos, endPos, startTan, endTan, (link == null) ? startDragAnchor.anchorColor : link.color, 4, s, m);
					break ;
			}
			if (link != null && link.linkHighlight == PWLinkHighlight.DeleteAndReset)
				link.linkHighlight = PWLinkHighlight.None;
			if (link != null && !link.selected && link.linkHighlight == PWLinkHighlight.Selected)
				link.linkHighlight = PWLinkHighlight.None;
		}
    }

	void	DrawSelectedBezier(Vector3 startPos, Vector3 endPos, Vector3 startTan, Vector3 endTan, Color c, int width, PWLinkHighlight linkHighlight, PWNodeProcessMode linkMode)
	{
		switch (linkHighlight)
		{
			case PWLinkHighlight.Selected:
				Handles.DrawBezier(startPos, endPos, startTan, endTan, PWColorPalette.GetColor("selectedNode"), null, width + 3);
				break;
			case PWLinkHighlight.Delete:
			case PWLinkHighlight.DeleteAndReset:
				Handles.DrawBezier(startPos, endPos, startTan, endTan, new Color(1f, .0f, .0f, 1), null, width + 2);
				break ;
		}
		Handles.DrawBezier(startPos, endPos, startTan, endTan, c, null, width);

		if (linkMode == PWNodeProcessMode.RequestForProcess)
		{
			Vector3[] points = Handles.MakeBezierPoints(startPos, endPos, startTan, endTan, 4);
			Vector2 pauseSize = new Vector2(20, 20);
			Matrix4x4 savedGUIMatrix = GUI.matrix;
			Rect pauseRect = new Rect((Vector2)points[2] - pauseSize / 2, pauseSize);
			float angle = Vector2.Angle((startPos.y > endPos.y) ? startPos - endPos : endPos - startPos, Vector2.right);
			GUIUtility.RotateAroundPivot(angle, points[2]);
			GUI.color = c;
            GUI.DrawTexture(pauseRect, pauseIconTexture);
			GUI.color = Color.white;
			GUI.matrix = savedGUIMatrix;
        }
	}

}
