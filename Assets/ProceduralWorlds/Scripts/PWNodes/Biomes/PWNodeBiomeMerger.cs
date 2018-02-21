using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using System.Linq;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeMerger : PWNode
	{

		[PWInput]
		public BlendedBiomeTerrain	inputBlendedTerrain;

		[PWOutput]
		public FinalTerrain			mergedBiomeTerrain;

		public bool					biomeTerrainsFoldout;

		Sampler						finalTerrain = null;

		public override void OnNodeCreation()
		{
			name = "Biome Merger";
		}

		public override void OnNodeEnable()
		{
			//initialize here all unserializable datas used for GUI (like Texture2D, ...)
		}

		public override void OnNodeProcess()
		{
			if (mergedBiomeTerrain == null)
				mergedBiomeTerrain = new FinalTerrain();
			
			if (inputBlendedTerrain.biomeData == null)
			{
				Debug.LogError("[PWBiomeMerger] Can't find BiomeData, did you forgot to specify the BiomeGraph in a Biome node");
				return ;
			}
			
			var inputTerrain = inputBlendedTerrain.biomeData.GetSampler(BiomeSamplerName.terrainHeight);
			finalTerrain = inputTerrain.Clone(finalTerrain);

			if (finalTerrain.type == SamplerType.Sampler2D)
			{
				BiomeMap2D biomeMap = inputBlendedTerrain.biomeData.biomeMap;

				(finalTerrain as Sampler2D).Foreach((x, y, val) => {
					float ret = 0;

					var biomeInfo = biomeMap.GetBiomeBlendInfo(x, y);
					foreach (var biome in inputBlendedTerrain.biomes)
					{
						var terrain = biome.modifiedTerrain as Sampler2D;

						if (terrain == null)
						{
							PWUtils.LogErrorMax("[PWNodeMerger] can't access to the terrain of the biome " + biome.id + "(" + biome.name + ")", 100);
							continue ;
						}

						//TODO: test this blending !
						for (int i = 0; i < biomeInfo.length; i++)
							if (biomeInfo.biomeIds[i] == biome.id)
								ret += terrain[x, y] * biomeInfo.biomeBlends[i] / biomeInfo.totalBlend;
					}

					return ret;
				});
			}
			else if (finalTerrain.type == SamplerType.Sampler3D)
			{
				Debug.Log("TODO: 3D Terrains");
			}

			mergedBiomeTerrain.biomeData = inputBlendedTerrain.biomeData;
			mergedBiomeTerrain.mergedTerrain = finalTerrain;
			mergedBiomeTerrain.materializerType = mainGraphRef.materializerType;

			mergedBiomeTerrain.biomeSurfacesList.Clear();
			foreach (var biome in inputBlendedTerrain.biomes)
			{
				if (mergedBiomeTerrain.biomeSurfacesList.ContainsKey(biome.id))
					Debug.LogError("[PWBiomeMerger] Duplicate biome in the biome graph: " + biome.name + ", id: " + biome.id);
				mergedBiomeTerrain.biomeSurfacesList[biome.id] = biome.biomeSurfaceGraph;
			}
		}
		
	}
}