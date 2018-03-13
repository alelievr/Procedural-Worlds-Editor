using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	public abstract class Drawer
	{
		protected ProceduralWorldsGUI		PWGUI = new ProceduralWorldsGUI();

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