using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	public class BiomeMapSettingsPopup : Popup
	{
		static bool debug;
		
		public static void OpenPopup(PWGUISettings guiSettings)
		{
			var popup = Popup.OpenPopup< BiomeMapSettingsPopup >();

			popup.name = "Biome map settings";
			debug = guiSettings.debug;
			controlId = guiSettings.GetHashCode();
		}
		
		protected override void OnGUIEnable()
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

		public static void UpdateDatas(PWGUISettings settings)
		{
			settings.debug = BiomeMapSettingsPopup.debug;
		}
	}
}