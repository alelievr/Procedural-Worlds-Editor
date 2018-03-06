using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Editor
{
	public class PWTextureSettingsPopup : PWPopup
	{

		static ScaleMode	scaleMode;
		static float		scaleAspect;
		static Material		material;
		static FilterMode	filterMode;
		static bool			debug;

		public static void OpenPopup(FilterMode filterMode, ScaleMode scaleMode, float scaleAspect, Material material, bool debug = false)
		{
			var popup = PWPopup.OpenPopup< PWTextureSettingsPopup >();

			popup.name = "Texture settings";
			PWTextureSettingsPopup.filterMode = filterMode;
			PWTextureSettingsPopup.scaleMode = scaleMode;
			PWTextureSettingsPopup.material = material;
			PWTextureSettingsPopup.scaleAspect = scaleAspect;
			PWTextureSettingsPopup.debug = debug;
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
			settings.scaleAspect = scaleAspect;
			settings.scaleMode = scaleMode;
			settings.material = material;
			settings.filterMode = filterMode;
			settings.debug = debug;
		}
	}
}