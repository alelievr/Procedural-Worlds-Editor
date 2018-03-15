using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using ProceduralWorlds.Core;

//popup system used to display ColorField's ColorPicker and settings windows.
namespace ProceduralWorlds.Editor
{
	public abstract class Popup : EditorWindow
	{

		public static int			controlId;

		protected static Vector2	windowSize = new Vector2(220, 300);

		[System.NonSerialized]
		bool firstonGUI = true;

		protected Event e { get { return Event.current; } }

		public EditorWindow	windowToUpdate;

		protected static T OpenPopup< T >(int id, Vector2 windowMinSize, bool allowResize = false) where T : Popup
		{
			EditorWindow currentWindow = EditorWindow.focusedWindow;

			T window = EditorWindow.CreateInstance< T >();

			controlId = id;
			window.windowToUpdate = currentWindow;
			window.ShowAuxWindow();
			window.minSize = windowMinSize;
			if (!allowResize)
				window.maxSize = windowMinSize;

			return window;
		}

		protected static T OpenPopup< T >(int id) where T : Popup
		{
			return OpenPopup< T >(id, windowSize);
		}

		void OnEnable()
		{
			GUIStart();
		}

		void OnGUI()
		{
			if (firstonGUI)
				OnGUIEnable();
			firstonGUI = false;

			GUIUpdate();

			if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
				Close();
		}

		protected void SendUpdate(string key)
		{
			var evt = EditorGUIUtility.CommandEvent(key);

			if (windowToUpdate != null)
				windowToUpdate.SendEvent(evt);
		}
		
		protected static T FindPopup< T >() where T : Popup
		{
			var popups = Resources.FindObjectsOfTypeAll< T >();

			if (popups == null || popups.Length == 0)
				return null;

			return popups[0] as T;
		}
		
		protected virtual void OnGUIEnable() {}
		protected virtual void GUIStart() {}
		protected abstract void GUIUpdate();
		
	}
}
