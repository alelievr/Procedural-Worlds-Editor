using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using System.Linq;
using ProceduralWorlds.Biomator;
using System;

namespace ProceduralWorlds.Nodes
{
	public class NodeBiomeMerger : BaseNode
	{

		[Input("Blended terrain")]
		public BlendedBiomeTerrain	inputBlendedTerrain;

		[Output("Merged terrain")]
		public WorldChunk			mergedBiomeTerrain;
		
		public MaterializerType		materializerType;

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
				mergedBiomeTerrain = new WorldChunk();
			
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
					float	ret = 0;
					var		biomeInfo = biomeMap.GetBiomeBlendInfo(x, y);
					
					for (int i = 0; i < biomeInfo.length; i++)
					{
						if (!inputBlendedTerrain.biomePerIds.ContainsKey(biomeInfo.biomeIds[i]))
						{
							Debug.Log("Ids: ");
							foreach (var kp in inputBlendedTerrain.biomePerIds)
								Debug.Log(kp.Key + " - " + kp.Value.name);
							Debug.Log("Point: ");
							foreach (var id in biomeInfo.biomeIds)
								Debug.Log(id);
						}
						var biome = inputBlendedTerrain.biomePerIds[biomeInfo.biomeIds[i]];
						
						if (biome == null)
							throw new NullReferenceException("[NodeMerger] Can't access to biome(null) from biome blender inputs");

						Sampler2D modifiedTerrain = biome.modifiedTerrain as Sampler2D;
						
						if (modifiedTerrain == null)
							throw new InvalidOperationException("[NodeMerger] can't access to the terrain of the biome " + biome.id + "(" + biome.name + ")");

						if (biomeInfo.biomeIds[i] == biome.id)
							ret += modifiedTerrain[x, y] * biomeInfo.biomeBlends[i];
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
			mergedBiomeTerrain.materializerType = materializerType;

			mergedBiomeTerrain.biomeSurfacesList.Clear();
			foreach (var biomeKP in inputBlendedTerrain.biomePerIds)
			{
				if (biomeKP.Value == null)
					continue ;
				
				var biome = biomeKP.Value;
				if (mergedBiomeTerrain.biomeSurfacesList.ContainsKey(biome.id))
					Debug.LogError("[PWBiomeMerger] Duplicate biome in the biome graph: " + biome.name + ", id: " + biome.id);
				mergedBiomeTerrain.biomeSurfacesList[biome.id] = biome.biomeSurfaceGraph;
			}
		}
		
	}
}