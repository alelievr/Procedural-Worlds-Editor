using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using PW.Core;

namespace PW
{
	public static class PWBiomeUtils {
	
		public static void DrawBiomeInfos(BiomeData b)
		{
			EditorGUILayout.LabelField("Biome datas:");

			if (b.terrain != null)
				EditorGUILayout.LabelField("Terrain: 2D");
			else if (b.terrain3D != null)
				EditorGUILayout.LabelField("Terrain: 3D");
		}
	}
}