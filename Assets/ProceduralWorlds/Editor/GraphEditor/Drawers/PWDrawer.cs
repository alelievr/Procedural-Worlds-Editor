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

		public void OnEnable(object target)
		{
			this.target = target;
			OnEnable();
		}

		public abstract void OnEnable();
	}
}