using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

//popup system used to display ColorField's ColorPicker and settings windows.
namespace PW
{
	public static class PWPopup {

		public static bool		shouldClose;
		public static bool		mouseAbove {get; private set;}
		public static EventType	eventType;

		private class PWPopupData
		{
			public PWGUISettings			guiSettings;
			public Action< PWGUISettings>	windowGUI;

			public PWPopupData(PWGUISettings guiSettings, Action< PWGUISettings > windowGUI)
			{
				this.guiSettings = guiSettings;
				this.windowGUI = windowGUI;
			}
		}

		private static List< PWPopupData > toRender = new List< PWPopupData >();
		
		private static bool					dragging = false;
		private static Vector2				popupSize = new Vector2(150, 250);
		private static GUIStyle				popupStyle;
		private static GUIStyle				closeStyle;

		static PWPopup()
		{
			popupStyle = GUI.skin.FindStyle("Popup");
			closeStyle = GUI.skin.FindStyle("WinBtnCloseMac");
			mouseAbove = false;
		}

		public static void AddToRender(PWGUISettings s, Action< PWGUISettings > windowGUI)
		{
			if (Event.current.type == EventType.Layout)
				toRender.Add(new PWPopupData(s, windowGUI));
		}

		public static void ClearAll()
		{
			toRender.Clear();
		}

		public static void RenderAll()
		{
			mouseAbove = false;

			foreach (var d in toRender)
				RenderPopup(d);
		}

		private static void RenderPopup(PWPopupData data)
		{
			var e = Event.current;

			if (e.type == EventType.Ignore)
				e.type = eventType;
			
			Vector2 position = data.guiSettings.windowPosition;
			int		closeIconSize = 18;
			Rect	popupRect = new Rect(position, popupSize);
			Rect	dragRect = new Rect(position, new Vector2(popupSize.x, 18));
			Rect	closeIconRect = new Rect(position + new Vector2(popupSize.x, 0), new Vector2(closeIconSize, closeIconSize));

			if (dragRect.Contains(e.mousePosition))
			{
				if (e.type == EventType.MouseDown)
					dragging = true;
			}
			// GUI.Label(closeIconRect, (string)null, closeStyle);
			if (closeIconRect.Contains(e.mousePosition))
				if (e.type == EventType.MouseUp)
					data.guiSettings.InActive();
			else if (e.type == EventType.MouseUp)
				dragging = false;
			
			if (dragging && e.type == EventType.mouseDrag)
				data.guiSettings.windowPosition += e.delta;
			
			GUILayout.BeginArea(popupRect, popupStyle);
			data.guiSettings.update = false;
			data.windowGUI(data.guiSettings);
			GUILayout.EndArea();

			if (popupRect.Contains(e.mousePosition))
				mouseAbove = true;
		}
	}
}