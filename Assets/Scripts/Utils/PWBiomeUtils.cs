using System;
using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using Debug = UnityEngine.Debug;

namespace PW
{
	public static class BiomeUtils {

		static bool			terrainFoldout = false;
		static bool			wetnessFoldout = false;
		static bool			temperatureFoldout = false;
		static bool			waterFoldout = false;
		static bool			update = false;
		static Color[]		blackTexture;
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


//TODO: remove this
/*		public static void ApplyBiomeTerrainModifiers(BlendedBiomeTerrain b)
		{
			if (b.terrain == null)
				return ;
			
			if (b.terrain.type == SamplerType.Sampler2D)
			{
				PWUtils.ResizeSamplerIfNeeded(b.terrain, ref b.biomeTerrain);
				Sampler2D	terrain = b.terrain as Sampler2D;
				Sampler2D	biomeTerrain = b.biomeTerrain as Sampler2D;
				//Fill biomeTerrain instead of terrain to keep an original version of the terrain
				biomeTerrain.Foreach((x, y) => {
					float val = terrain[x, y];
					int	biomeId = b.biomeMap.GetBiomeBlendInfo(x, y).firstBiomeId;
					if (biomeId == -1)
						return val;
					//TODO: biome blending
					return b.biomeTree.GetBiome(biomeId).biomeTerrain.ComputeValue(x, y, val);
				});
			}
			//else: TODO
			//TODO: apply biome terrain modifiers to terrain

			//TODO: apply biome terrain detail (caves / oth)
		}*/

		static IEnumerable< Texture2D > GenerateBiomeBlendMaps(BlendedBiomeTerrain b)
		{
			List< Texture2D >		texs = new List< Texture2D >();

			#if UNITY_EDITOR
				Stopwatch	sw = new Stopwatch();
				sw.Start();
			#endif

			int			chunkSize = b.terrain.size;
			BiomeMap2D	biomeMap = b.biomeMap;
			int			textureCount = 0;
			foreach (var kp in b.biomeTree.GetBiomes())
				if (kp.Value.biomeSurfaces != null)
					foreach (var layer in kp.Value.biomeSurfaces.biomeLayers)
						textureCount += layer.slopeMaps.Count;
			if (blackTexture == null || blackTexture.Length != chunkSize * chunkSize)
				blackTexture = new Color[chunkSize * chunkSize];

			for (int i = 0; i <= textureCount / 4; i++)
			{
				Texture2D	tex = new Texture2D(chunkSize, chunkSize, TextureFormat.RGBA32, true, false);
				tex.SetPixels(blackTexture);
				tex.filterMode = FilterMode.Point;
				tex.Apply();
				texs.Add(tex);
			}

			for (int x = 0; x < chunkSize; x++)
				for (int y = 0; y < chunkSize; y++)
				{
					var bInfo = biomeMap.GetBiomeBlendInfo(x, y);
					if (bInfo.firstBiomeId == -1 || bInfo.secondBiomeId == -1)
						continue ;
					
					//TODO: biome blening
					int		texIndex = bInfo.firstBiomeId / 4;
					int		texChan = bInfo.firstBiomeId % 4;
					Color c = texs[texIndex].GetPixel(x, y);
					c[texChan] = bInfo.firstBiomeBlendPercent;
					texs[texIndex].SetPixel(x, y, c);
				}

			foreach (var tex in texs)
				tex.Apply();

			#if UNITY_EDITOR
				sw.Stop();
				// Debug.Log(sw.ElapsedMilliseconds + "ms taken to generate blend maps");
			#endif
			
			return texs;
		}
	}
}