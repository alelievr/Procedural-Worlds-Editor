using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using PW.Core;

namespace PW.Editor
{
	public abstract class PWDrawer
	{
		protected PWGUIManager		PWGUI = new PWGUIManager();

		protected object			target;

		protected Event				e { get { return Event.current; } }
		
		[System.NonSerialized]
		public bool					isEnabled = false;

		public void OnEnable(object target)
		{
			this.target = target;
			OnEnable();
			isEnabled = true;
		}

		public abstract void OnEnable();

		public void OnGUI(Rect r)
		{
			PWGUI.StartFrame(r);
		}
	}
}