using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace PW
{
	public static class PWBiomeUtils {
	
		public static void DrawBiomeInfos(BiomeData b)
		{
			EditorGUILayout.LabelField("Biome");

			if (b.terrain != null)
				EditorGUILayout.LabelField("Terrain: 2D");
			else if (b.terrain3D != null)
				EditorGUILayout.LabelField("Terrain: 3D");
		}
		
		public static void DrawBiomeInfos(BiomeData3D b)
		{
			EditorGUILayout.LabelField("Biome 3D");
		}
		
		public static void DrawBiomeInfos(WaterlessBiomeData b)
		{
			EditorGUILayout.LabelField("Waterless biome");
		}
		
		public static void DrawBiomeInfos(WaterlessBiomeData3D b)
		{
			EditorGUILayout.LabelField("Waterless biome 3D");
		}

	}
}