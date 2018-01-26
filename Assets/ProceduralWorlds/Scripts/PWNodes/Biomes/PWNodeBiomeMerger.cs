using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using System.Linq;

namespace PW.Node
{
	public class PWNodeBiomeMerger : PWNode
	{

		[PWInput]
		public BlendedBiomeTerrain	inputBlendedTerrain;

		[PWOutput]
		public FinalBiomeTerrain	mergedBiomeTerrain;

		[System.NonSerialized]
		bool						update;

		public override void OnNodeCreation()
		{
			name = "Biome Merger";
		}

		public override void OnNodeEnable()
		{
			//initialize here all unserializable datas used for GUI (like Texture2D, ...)
		}

		public override void OnNodeGUI()
		{
			Sampler finalTerrain = null;
			
			if (inputBlendedTerrain != null)
				finalTerrain = inputBlendedTerrain.biomeData.terrainRef;

			if (finalTerrain != null)
				PWGUI.SamplerPreview("Final merged terrain", finalTerrain);
			else
				EditorGUILayout.LabelField("Null terrain");

			if (update)
			{
				update = false;
				PWGUI.SetUpdateForField(0, true);
			}
		}

		public override void OnNodeProcess()
		{
			if (mergedBiomeTerrain == null)
				mergedBiomeTerrain = new FinalBiomeTerrain();
			
			Sampler finalTerrain = inputBlendedTerrain.biomeData.terrainRef;

			if (finalTerrain.type == SamplerType.Sampler2D)
			{
				BiomeMap2D biomeMap = inputBlendedTerrain.biomeData.biomeIds;

				(finalTerrain as Sampler2D).Foreach((x, y, val) => {
					float ret = 0;

					var biomeInfo = biomeMap.GetBiomeBlendInfo(x, y);
					foreach (var biome in inputBlendedTerrain.biomes)
					{
						var terrain = biome.modifiedTerrain as Sampler2D;
						if (terrain == null)
						{
							Debug.LogError("[PWNodeMerger] can't access to the terrain of the biome " + biome.id + "(" + biome.name + ")");
							continue ;
						}

						if (biome.id == biomeInfo.firstBiomeId)
							ret += terrain[x, y] * biomeInfo.firstBiomeBlendPercent;
						else if (biome.id == biomeInfo.secondBiomeId)
							ret += terrain[x, y] * biomeInfo.secondBiomeBlendPercent;
					}

					return ret;
				});
			}
			else if (finalTerrain.type == SamplerType.Sampler3D)
			{
				Debug.Log("TODO");
			}

			mergedBiomeTerrain.biomeData = inputBlendedTerrain.biomeData;

			mergedBiomeTerrain.biomeSurfacesList.Clear();
			foreach (var biome in inputBlendedTerrain.biomes)
				mergedBiomeTerrain.biomeSurfacesList.Add(biome.id, biome.biomeSurfaces);
			
			update = true;
		}
		
	}
}