using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

//popup system used to display ColorField's ColorPicker and settings windows.
namespace PW.Core
{
	public static class PWPopup {

		public static bool		mouseAbove {get; private set;}
		public static EventType	eventType;

		private class PWPopupData
		{
			public PWGUISettings	guiSettings;
			public Action			windowGUI;
			public string			name;
			public int				popupWidth;

			public PWPopupData(PWGUISettings guiSettings, Action windowGUI, string name, int popupWidth)
			{
				this.guiSettings = guiSettings;
				this.windowGUI = windowGUI;
				this.name = name;
				this.popupWidth = popupWidth;
			}
		}

		private static List< PWPopupData > toRender = new List< PWPopupData >();
		
		private static bool					dragging = false;
		private static GUIStyle				popupStyle;
		private static GUIStyle				headerStyle;
		private static Texture2D			closeTexture;

		static PWPopup()
		{
			popupStyle = GUI.skin.FindStyle("Popup");
			closeTexture = Resources.Load< Texture2D >("ic_error");
			headerStyle = GUI.skin.FindStyle("PopupHeader");
			
			mouseAbove = false;
		}

		public static void AddToRender(PWGUISettings s, string name, Action windowGUI, int popupWidth = 150)
		{
			if (Event.current.type == EventType.Layout)
				toRender.Add(new PWPopupData(s, windowGUI, name, popupWidth));
		}

		public static void ClearAll()
		{
			toRender.Clear();
		}

		public static void RenderAll(ref bool needExternalRepaint)
		{
			mouseAbove = false;

			foreach (var d in toRender)
				RenderPopup(d, ref needExternalRepaint);
		}

		private static void RenderPopup(PWPopupData data, ref bool needExternalRepaint)
		{
			var e = Event.current;

			if (e.type == EventType.Ignore)
				e.type = eventType;
			
			Vector2 position = data.guiSettings.windowPosition;
			int		closeIconSize = 17;
			int		headerPadding = 12;
			Rect	popupRect = new Rect(position, new Vector2(data.popupWidth, data.guiSettings.popupHeight + 20 + 5));
			Rect	dragRect = new Rect(position, new Vector2(data.popupWidth, 18));
			Rect	headerRect = new Rect(dragRect.position + new Vector2(headerPadding, -1), dragRect.size - new Vector2(headerPadding * 2 + closeIconSize, 0));
			Rect	closeIconRect = new Rect(position + new Vector2(data.popupWidth - closeIconSize - 12, 1), new Vector2(closeIconSize, closeIconSize));

			if (dragRect.Contains(e.mousePosition))
			{
				if (e.type == EventType.MouseDown)
					dragging = true;
			}
			if (closeIconRect.Contains(e.mousePosition))
				if (e.type == EventType.MouseUp)
					data.guiSettings.InActive();
			if (e.type == EventType.MouseUp)
				dragging = false;
			
			if (dragging && e.type == EventType.mouseDrag)
				data.guiSettings.windowPosition += e.delta;
			
			GUILayout.BeginArea(popupRect, popupStyle);
			EditorGUILayout.BeginVertical();
			data.windowGUI();
			EditorGUILayout.EndVertical();
			if (e.type == EventType.Repaint)
				data.guiSettings.popupHeight = (int)GUILayoutUtility.GetLastRect().height;
			GUILayout.EndArea();
			
			GUI.color = Color.red;
			GUI.DrawTexture(closeIconRect, closeTexture);
			GUI.Label(headerRect, data.name, headerStyle);
			
			GUI.color = Color.white;

			if (dragging)
				needExternalRepaint = true;

			if (popupRect.Contains(e.mousePosition))
				mouseAbove = true;
		}
	}
}
