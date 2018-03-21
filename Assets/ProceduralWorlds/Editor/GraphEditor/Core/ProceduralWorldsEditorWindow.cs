using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	public class ProceduralWorldsEditorWindow : EditorWindow
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
			{
				firstGUILoop = false;
				OnGUIEnable();
			}
		}
	
		public virtual void OnDisable()
		{
			
		}
	
	}
}