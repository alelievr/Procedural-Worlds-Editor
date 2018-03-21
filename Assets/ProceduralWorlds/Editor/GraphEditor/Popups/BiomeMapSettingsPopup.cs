using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	public class BiomeMapSettingsPopup : Popup
	{
		bool debug;
		
		public static void OpenPopup(PWGUISettings guiSettings)
		{
			var popup = Popup.OpenPopup< BiomeMapSettingsPopup >(guiSettings.GetHashCode());

			popup.name = "Biome map settings";
			popup.debug = guiSettings.debug;
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
			var popup = FindPopup< BiomeMapSettingsPopup >();

			if (popup == null)
				return ;
			
			settings.debug = popup.debug;
		}
	}
}