using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using PW.Core;

namespace PW
{
	public static class BiomeUtils {

		static bool terrainFoldout = false;
		static bool wetnessFoldout = false;
		static bool temperatureFoldout = false;
		static bool waterFoldout = false;
		static bool update = false;
		static PWGUIManager	PWGUI = new PWGUIManager();
	
		public static void DrawBiomeInfos(BiomeData b)
		{
			PWGUI.currentWindowRect = new Rect(0, 0, 200, 150);
			PWGUI.StartFrame();
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

		public static void ApplyBiomeTerrainModifiers(BlendedBiomeTerrain b)
		{
			if (b.terrain.type == SamplerType.Sampler2D)
			{
				(b.terrain as Sampler2D).Foreach((x, y, val) => {
					int	biomeId = b.biomeMap.GetBiomeBlendInfo(x, y).firstBiomeId;
					//TODO: biome blending
					return b.biomeTree.GetBiome(biomeId).biomeTerrain.ComputeValue(x, y, val);
				});
			}
			//else: TODO
			//TODO: apply biome terrain modifiers to terrain

			//TODO: apply biome terrain detail (caves / oth)
		}

		public static void BakeBiomeSurfaceMaps(BlendedBiomeTerrain b)
		{
			//TODO: create surface blend maps
		}
	}
}