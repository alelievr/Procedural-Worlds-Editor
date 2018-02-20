using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW.Core
{
	public class PWBiomeMapSettingsPopup : PWPopup
	{
		public static bool debug = false;
		
		public static void OpenPopup(PWGUISettings guiSettings)
		{
			PWPopup.OpenPopup< PWBiomeMapSettingsPopup >();

			debug = guiSettings.debug;
			controlId = guiSettings.GetHashCode();
		}
		
		protected override void OnGUIEnable()
		{

		}
	
		protected override void GUIStart()
		{
		}
		
		protected override void GUIUpdate()
		{
			EditorGUI.BeginChangeCheck();
			{
				debug = EditorGUILayout.Toggle("Debug", debug);
			}
			if (EditorGUI.EndChangeCheck())
				SendUpdate("BiomeMapSettingsUpdate");
		}
	}
}