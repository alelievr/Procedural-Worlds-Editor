using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW.Core
{
	public class PWTextureSettingsPopup : PWPopup
	{

		public static ScaleMode		scaleMode;
		public static float			scaleAspect;
		public static Material		material;
		public static FilterMode	filterMode;
		public static bool			debug;

		public static void OpenPopup(FilterMode filterMode, ScaleMode scaleMode, float scaleAspect, Material material, bool debug = false)
		{
			PWPopup.OpenPopup< PWTextureSettingsPopup >();

			PWTextureSettingsPopup.filterMode = filterMode;
			PWTextureSettingsPopup.scaleMode = scaleMode;
			PWTextureSettingsPopup.material = material;
			PWTextureSettingsPopup.scaleAspect = scaleAspect;
			PWTextureSettingsPopup.debug = debug;
		}

		protected override void GUIStart()
		{
		}
	
		protected override void OnGUIEnable()
		{
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
	}
}