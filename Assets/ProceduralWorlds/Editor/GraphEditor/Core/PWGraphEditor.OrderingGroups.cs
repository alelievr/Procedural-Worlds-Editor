using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using System.Linq;
using UnityEditor;
using UnityEngine.Profiling;
using System;
using PW;

//Ordering group rendering for PWGraphEditor
public partial class PWGraphEditor
{
	int				callbackId;
	
	Texture2D		movepadTexture;
	GUIStyle		orderingGroupStyle;
	GUIStyle		orderingGroupNameStyle;
	
	void LoadOrderingGroupStyles()
	{
		orderingGroupStyle = GUI.skin.FindStyle("OrderingGroup");
		orderingGroupNameStyle = GUI.skin.FindStyle("OrderingGroupNameStyle");
		movepadTexture = Resources.Load("GUI/movepad") as Texture2D;
	}

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

	void CreateAnchorRectCallabck(PWOrderingGroup orderingGroup, Rect r, MouseCursor cursor, Action callback)
	{
		EditorGUIUtility.AddCursorRect(r, cursor);

		if (orderingGroup.resizing && callbackId == orderingGroup.resizingCallbackId && Event.current.type == EventType.MouseDrag)
			callback();
		if (r.Contains(Event.current.mousePosition))
		{
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				orderingGroup.resizing = true;
				orderingGroup.resizingCallbackId = callbackId;
				Event.current.Use();
			}
		}
		callbackId++;
	}

	void RenderOrderingGroups()
	{
		Profiler.BeginSample("[PW] Render ordering groups");

		foreach (var orderingGroup in graph.orderingGroups)
			RenderOrderingGroup(orderingGroup, graph, position.size * (1 / graph.scale));

		//if the mouse was not over an ordering group this frame
		if (!editorEvents.isMouseOverOrderingGroupFrame)
			editorEvents.mouseOverOrderingGroup = null;

		Profiler.EndSample();
	}
	
	public void RenderOrderingGroup(PWOrderingGroup orderingGroup, PWGraph graph, Vector2 screenSize)
	{
		var e = Event.current;
		Rect screen = new Rect(-graph.panPosition, screenSize);

		//check if ordering group is not visible
		if (!orderingGroup.orderGroupRect.Overlaps(screen))
			return ;
		
		//Start GUI frame
		PWGUI.StartFrame(screen);
		
		if (orderingGroupStyle == null)
			LoadStyles();

		Rect		orderGroupWorldRect = PWUtils.DecalRect(orderingGroup.orderGroupRect, graph.panPosition);

		callbackId = 0;

		int			controlSize = 8;
		int			cornerSize = 14;

		//AH this is ugly
		CreateAnchorRectCallabck(orderingGroup, //left resize anchor
			new Rect(orderGroupWorldRect.x, orderGroupWorldRect.y + cornerSize, controlSize, orderGroupWorldRect.height - cornerSize * 2),
			MouseCursor.ResizeHorizontal,
			() => orderingGroup.orderGroupRect.xMin += e.delta.x
		);
		CreateAnchorRectCallabck(orderingGroup, //right resize anchor
			new Rect(orderGroupWorldRect.x + orderGroupWorldRect.width - controlSize, orderGroupWorldRect.y + cornerSize, controlSize, orderGroupWorldRect.height - cornerSize * 2),
			MouseCursor.ResizeHorizontal,
			() => orderingGroup.orderGroupRect.xMax += e.delta.x
		);
		CreateAnchorRectCallabck(orderingGroup, //top resize anchor
			new Rect(orderGroupWorldRect.x + cornerSize, orderGroupWorldRect.y, orderGroupWorldRect.width - cornerSize * 2, controlSize),
			MouseCursor.ResizeVertical,
			() => orderingGroup.orderGroupRect.yMin += e.delta.y
		);
		CreateAnchorRectCallabck(orderingGroup, //down resize anchor
			new Rect(orderGroupWorldRect.x + cornerSize, orderGroupWorldRect.y + orderGroupWorldRect.height - controlSize, orderGroupWorldRect.width - cornerSize * 2, controlSize),
			MouseCursor.ResizeVertical,
			() => orderingGroup.orderGroupRect.yMax += e.delta.y
		);

		CreateAnchorRectCallabck(orderingGroup, //top left anchor
			new Rect(orderGroupWorldRect.x, orderGroupWorldRect.y, cornerSize, cornerSize),
			MouseCursor.ResizeUpLeft,
			() => { orderingGroup.orderGroupRect.min += e.delta; }
		);
		CreateAnchorRectCallabck(orderingGroup, //top right anchor
			new Rect(orderGroupWorldRect.x + orderGroupWorldRect.width - cornerSize, orderGroupWorldRect.y, cornerSize, cornerSize),
			MouseCursor.ResizeUpRight,
			() => {
				orderingGroup.orderGroupRect.yMin += e.delta.y;
				orderingGroup.orderGroupRect.xMax += e.delta.x;
			}
		);
		CreateAnchorRectCallabck(orderingGroup, //down left anchor
			new Rect(orderGroupWorldRect.x, orderGroupWorldRect.y + orderGroupWorldRect.height - cornerSize, cornerSize, cornerSize),
			MouseCursor.ResizeUpRight,
			() => {
				orderingGroup.orderGroupRect.xMin += e.delta.x;
				orderingGroup.orderGroupRect.yMax += e.delta.y;
			}
		);
		CreateAnchorRectCallabck(orderingGroup, //down right anchor
			new Rect(orderGroupWorldRect.x + orderGroupWorldRect.width - cornerSize, orderGroupWorldRect.y + orderGroupWorldRect.height - cornerSize, cornerSize, cornerSize),
			MouseCursor.ResizeUpLeft,
			() => {
				orderingGroup.orderGroupRect.yMax += e.delta.y;
				orderingGroup.orderGroupRect.xMax += e.delta.x;
			}
		);

		if (e.rawType == EventType.MouseUp)
			orderingGroup.resizing = false;

		//draw renamable name field
		orderingGroupNameStyle.normal.textColor = orderingGroup.color;
		PWGUI.TextField(orderGroupWorldRect.position + new Vector2(10, -22), ref orderingGroup.name, true, orderingGroupNameStyle);

		//draw move pad
		Rect movePadRect = new Rect(orderGroupWorldRect.position + new Vector2(10, 10), new Vector2(50, 30));
		GUI.DrawTextureWithTexCoords(movePadRect, movepadTexture, new Rect(0, 0, 5, 4));
		EditorGUIUtility.AddCursorRect(movePadRect, MouseCursor.MoveArrow);
		if (e.type == EventType.MouseDown && e.button == 0)
			if (movePadRect.Contains(e.mousePosition))
			{
				orderingGroup.innerNodes = graph.allNodes.Where(n => n.rect.Overlaps(orderingGroup.orderGroupRect)).ToList();
				orderingGroup.moving = true;
				e.Use();
			}
		if (e.rawType == EventType.MouseUp)
			orderingGroup.moving = false;

		if (orderingGroup.moving && e.type == EventType.MouseDrag)
		{
			orderingGroup.orderGroupRect.position += e.delta;
			orderingGroup.innerNodes.ForEach(n => n.rect.position += e.delta);
		}

		//draw ordering group
		GUI.color = orderingGroup.color;
		GUI.Label(orderGroupWorldRect, (string)null, orderingGroupStyle);
		GUI.color = Color.white;

		//draw color picker
		Rect colorPickerRect = new Rect(orderGroupWorldRect.x + orderGroupWorldRect.width - 30, orderGroupWorldRect.y + 10, 20, 20);
		PWGUI.ColorPicker(colorPickerRect, ref orderingGroup.color, false);

		if (orderGroupWorldRect.Contains(e.mousePosition))
		{
			graph.editorEvents.mouseOverOrderingGroup = orderingGroup;
			graph.editorEvents.isMouseOverOrderingGroupFrame = true;
		}
	}
}
