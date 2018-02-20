using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace PW.Core
{
	[System.Serializable]
	public class PWOrderingGroup
	{
	
		public Rect					orderGroupRect;
		public string				name;
		public SerializableColor	color;

		[System.NonSerialized]
		public bool					resizing = false;

		bool						moving = false;
		int							callbackId;
		int							resizingCallbackId;

		PWGUIManager				PWGUI = new PWGUIManager();

		static Texture2D			movepadTexture;
		static GUIStyle				orderingGroupStyle;
		static GUIStyle				orderingGroupNameStyle;

		[System.NonSerialized]
		List< PWNode >				innerNodes = new List< PWNode >();

		public void Initialize(Vector2 pos)
		{
			orderGroupRect = new Rect();
			orderGroupRect.position = pos;
			orderGroupRect.size = new Vector2(240, 120);
			name = "ordering group";
			color = (SerializableColor)Color.white;
		}

		void CreateAnchorRectCallabck(Rect r, MouseCursor cursor, Action callback)
		{
			EditorGUIUtility.AddCursorRect(r, cursor);

			if (resizing && callbackId == resizingCallbackId && Event.current.type == EventType.MouseDrag)
				callback();
			if (r.Contains(Event.current.mousePosition))
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
				{
					resizing = true;
					resizingCallbackId = callbackId;
					Event.current.Use();
				}
			}
			callbackId++;
		}

		void LoadStyles()
		{
			orderingGroupStyle = GUI.skin.FindStyle("OrderingGroup");
			orderingGroupNameStyle = GUI.skin.FindStyle("OrderingGroupNameStyle");
			movepadTexture = Resources.Load("movepad") as Texture2D;
		}

		public void Render(PWGraph graph, Vector2 screenSize)
		{
			var e = Event.current;
			Rect screen = new Rect(-graph.panPosition, screenSize);

			//check if ordering group is not visible
			if (!orderGroupRect.Overlaps(screen))
				return ;
			
			//Start GUI frame
			PWGUI.StartFrame(screen);
			
			if (orderingGroupStyle == null)
				LoadStyles();

			Rect		orderGroupWorldRect = PWUtils.DecalRect(orderGroupRect, graph.panPosition);

			callbackId = 0;

			int			controlSize = 8;
			int			cornerSize = 14;

			CreateAnchorRectCallabck( //left resize anchor
				new Rect(orderGroupWorldRect.x, orderGroupWorldRect.y + cornerSize, controlSize, orderGroupWorldRect.height - cornerSize * 2),
				MouseCursor.ResizeHorizontal,
				() => orderGroupRect.xMin += e.delta.x
			);
			CreateAnchorRectCallabck( //right resize anchor
				new Rect(orderGroupWorldRect.x + orderGroupWorldRect.width - controlSize, orderGroupWorldRect.y + cornerSize, controlSize, orderGroupWorldRect.height - cornerSize * 2),
				MouseCursor.ResizeHorizontal,
				() => orderGroupRect.xMax += e.delta.x
			);
			CreateAnchorRectCallabck( //top resize anchor
				new Rect(orderGroupWorldRect.x + cornerSize, orderGroupWorldRect.y, orderGroupWorldRect.width - cornerSize * 2, controlSize),
				MouseCursor.ResizeVertical,
				() => orderGroupRect.yMin += e.delta.y
			);
			CreateAnchorRectCallabck( //down resize anchor
				new Rect(orderGroupWorldRect.x + cornerSize, orderGroupWorldRect.y + orderGroupWorldRect.height - controlSize, orderGroupWorldRect.width - cornerSize * 2, controlSize),
				MouseCursor.ResizeVertical,
				() => orderGroupRect.yMax += e.delta.y
			);

			CreateAnchorRectCallabck( //top left anchor
				new Rect(orderGroupWorldRect.x, orderGroupWorldRect.y, cornerSize, cornerSize),
				MouseCursor.ResizeUpLeft,
				() => {orderGroupRect.yMin += e.delta.y; orderGroupRect.xMin += e.delta.x;}
			);
			CreateAnchorRectCallabck( //top right anchor
				new Rect(orderGroupWorldRect.x + orderGroupWorldRect.width - cornerSize, orderGroupWorldRect.y, cornerSize, cornerSize),
				MouseCursor.ResizeUpRight,
				() => {orderGroupRect.yMin += e.delta.y; orderGroupRect.xMax += e.delta.x;}
			);
			CreateAnchorRectCallabck( //down left anchor
				new Rect(orderGroupWorldRect.x, orderGroupWorldRect.y + orderGroupWorldRect.height - cornerSize, cornerSize, cornerSize),
				MouseCursor.ResizeUpRight,
				() => {orderGroupRect.yMax += e.delta.y; orderGroupRect.xMin += e.delta.x;}
			);
			CreateAnchorRectCallabck( //down right anchor
				new Rect(orderGroupWorldRect.x + orderGroupWorldRect.width - cornerSize, orderGroupWorldRect.y + orderGroupWorldRect.height - cornerSize, cornerSize, cornerSize),
				MouseCursor.ResizeUpLeft,
				() => {orderGroupRect.yMax += e.delta.y; orderGroupRect.xMax += e.delta.x;}
			);

			if (e.rawType == EventType.MouseUp)
				resizing = false;

			//draw renamable name field
			orderingGroupNameStyle.normal.textColor = color;
			PWGUI.TextField(orderGroupWorldRect.position + new Vector2(10, -22), ref name, true, orderingGroupNameStyle);

			//draw move pad
			Rect movePadRect = new Rect(orderGroupWorldRect.position + new Vector2(10, 10), new Vector2(50, 30));
			GUI.DrawTextureWithTexCoords(movePadRect, movepadTexture, new Rect(0, 0, 5, 4));
			EditorGUIUtility.AddCursorRect(movePadRect, MouseCursor.MoveArrow);
			if (e.type == EventType.MouseDown && e.button == 0)
				if (movePadRect.Contains(e.mousePosition))
				{
					innerNodes = graph.nodes.Where(n => n.rect.Overlaps(orderGroupRect)).ToList();
					moving = true;
					e.Use();
				}
			if (e.rawType == EventType.MouseUp)
				moving = false;

			if (moving && e.type == EventType.MouseDrag)
			{
				orderGroupRect.position += e.delta;
				innerNodes.ForEach(n => n.rect.position += e.delta);
			}

			//draw ordering group
			GUI.color = color;
			GUI.Label(orderGroupWorldRect, (string)null, orderingGroupStyle);
			GUI.color = Color.white;

			//draw color picker
			Rect colorPickerRect = new Rect(orderGroupWorldRect.x + orderGroupWorldRect.width - 30, orderGroupWorldRect.y + 10, 20, 20);
			PWGUI.ColorPicker(colorPickerRect, ref color, false);

			if (orderGroupWorldRect.Contains(e.mousePosition))
			{
				graph.editorEvents.mouseOverOrderingGroup = this;
				graph.editorEvents.isMouseOverOrderingGroupFrame = true;
			}
		}
	}
}
