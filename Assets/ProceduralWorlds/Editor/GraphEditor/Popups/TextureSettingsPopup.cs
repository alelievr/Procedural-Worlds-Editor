using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	public class TextureSettingsPopup : Popup
	{

		ScaleMode	scaleMode;
		float		scaleAspect;
		Material		material;
		FilterMode	filterMode;
		bool			debug;

		public static void OpenPopup(PWGUISettings guiSettings)
		{
			var popup = Popup.OpenPopup< TextureSettingsPopup >(guiSettings.GetHashCode());

			popup.name = "Texture settings";
			popup.filterMode = guiSettings.filterMode;
			popup.scaleMode = guiSettings.scaleMode;
			popup.material = guiSettings.material;
			popup.scaleAspect = guiSettings.scaleAspect;
			popup.debug = guiSettings.debug;
		}

		protected override void GUIUpdate()
		{
			EditorGUI.BeginChangeCheck();
			{
				EditorGUIUtility.labelWidth = 80;
				filterMode = (FilterMode)EditorGUILayout.EnumPopup("filter mode", filterMode);
				scaleMode = (ScaleMode)EditorGUILayout.EnumPopup("scale mode", scaleMode);
				scaleAspect = EditorGUILayout.FloatField("scale aspect", scaleAspect);
				material = (Material)EditorGUILayout.ObjectField("material", material, typeof(Material), false);
				debug = EditorGUILayout.Toggle("debug", debug);
				EditorGUIUtility.labelWidth = 0;
			}
			if (EditorGUI.EndChangeCheck())
				SendUpdate("TextureSettingsUpdate");
		}

		public static void UpdateDatas(PWGUISettings settings)
		{
			var popup = FindPopup< TextureSettingsPopup >();

			if (popup == null)
				return ;
			
			settings.scaleAspect = popup.scaleAspect;
			settings.scaleMode = popup.scaleMode;
			settings.material = popup.material;
			settings.filterMode = popup.filterMode;
			settings.debug = popup.debug;
		}
	}
}