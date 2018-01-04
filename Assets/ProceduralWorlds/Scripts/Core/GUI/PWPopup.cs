using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

//popup system used to display ColorField's ColorPicker and settings windows.
namespace PW.Core
{
	public abstract class PWPopup : EditorWindow
	{

		[System.NonSerialized]
		bool firstonGUI = true;

		protected Event e { get { return Event.current; } }

		protected static T OpenPopup< T >() where T : PWPopup
		{
			T window = EditorWindow.GetWindow< T >();

			window.ShowAuxWindow();

			return window;
		}

		void OnEnable()
		{
			Start();
		}

		void OnGUI()
		{
			if (firstonGUI)
				OnGUIEnable();
			firstonGUI = false;

			Update();
		}
		
		protected abstract void OnGUIEnable();
		protected abstract void Start();
		protected abstract void Update();
		
	}
}
