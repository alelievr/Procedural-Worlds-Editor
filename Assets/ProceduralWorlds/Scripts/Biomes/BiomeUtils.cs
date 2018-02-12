using System;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using Debug = UnityEngine.Debug;

namespace PW.Biomator
{
	public static class BiomeUtils
	{

		static bool			terrainFoldout = false;
		static bool			wetnessFoldout = false;
		static bool			temperatureFoldout = false;
		static bool			waterFoldout = false;
		static bool			update = false;
		static Color[]		blackTexture;
		static PWGUIManager	PWGUI = new PWGUIManager();
	
		public static void DrawBiomeInfos(Rect view, BiomeData b)
		{
			if (b == null)
			{
				EditorGUILayout.LabelField("Null biome data");
				return ;
			}

			PWGUI.StartFrame(view);
			EditorGUILayout.LabelField("Biome datas:");

			update = GUILayout.Button("Update maps");

			//2D maps:
			if (b.terrain != null)
				if ((terrainFoldout = EditorGUILayout.Foldout(terrainFoldout, "Terrain 2D")))
					PWGUI.Sampler2DPreview(b.terrain, update);

			if (b.waterHeight != null)
				if ((waterFoldout = EditorGUILayout.Foldout(waterFoldout, "Water map")))
					PWGUI.Sampler2DPreview(b.waterHeight, update);

			if (b.wetness != null)
				if ((wetnessFoldout = EditorGUILayout.Foldout(wetnessFoldout, "Wetness map")))
					PWGUI.Sampler2DPreview(b.wetness, update);
			
			if (b.temperature != null)
				if ((temperatureFoldout = EditorGUILayout.Foldout(temperatureFoldout, "Temperature map")))
					PWGUI.Sampler2DPreview(b.temperature, update);
			
			//3D maps:
			if (b.terrain3D != null)
				EditorGUILayout.LabelField("Terrain: 3D");
		}

	}
}