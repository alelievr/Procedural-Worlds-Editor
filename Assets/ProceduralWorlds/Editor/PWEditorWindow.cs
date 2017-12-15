using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW.Editor
{
	public class PWEditorWindow : EditorWindow
	{
	
		[System.NonSerialized] bool firstGUILoop = true;
	
		public virtual void OnEnable()
		{
			
		}
	
		public virtual void OnGUIEnable()
		{
	
		}
	
		public virtual void OnGUI()
		{
			if (firstGUILoop)
				OnGUIEnable();
			
			firstGUILoop = false;
		}
	
		public virtual void OnDisable()
		{
			
		}
	
	}
}