using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

//popup system used to display ColorField's ColorPicker and settings windows.
namespace PW.Core
{
	public abstract class PWPopup : EditorWindow
	{

		public static int			controlId;

		protected static Vector2	windowSize = new Vector2(220, 300);

		[System.NonSerialized]
		bool firstonGUI = true;

		protected Event e { get { return Event.current; } }

		public EditorWindow	windowToUpdate;

		protected static T OpenPopup< T >(Vector2 windowMinSize, bool allowResize = false) where T : PWPopup
		{
			EditorWindow currentWindow = EditorWindow.focusedWindow;

			T window = EditorWindow.CreateInstance< T >();

			window.windowToUpdate = currentWindow;
			window.ShowAuxWindow();
			window.minSize = windowMinSize;
			if (!allowResize)
				window.maxSize = windowMinSize;

			return window;
		}

		protected static T OpenPopup< T >() where T : PWPopup
		{
			return OpenPopup< T >(windowSize);
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

			Debug.Log("Sending update to " + windowToUpdate);
			if (windowToUpdate != null)
				windowToUpdate.SendEvent(evt);
		}
		
		protected abstract void OnGUIEnable();
		protected abstract void GUIStart();
		protected abstract void GUIUpdate();
		
	}
}
