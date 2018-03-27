using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Nodes
{
	public class NodeBiomeDataDecomposer : BaseNode
	{

		[Input("Partial Biome")]
		public PartialBiome		inputPartialBiome;

		[Output("Partial Biome")]
		public PartialBiome		outputBiomeData;

		[Output("Terrain")]
		public Sampler			outputTerrain;

		[Output("Temperature map")]
		public Sampler			outputTemperatureMap;

		[Output("Wetness map")]
		public Sampler			outputWetnessMap;

		public override void OnNodeCreation()
		{
			name = "BiomeData decomposer";
		}

		public override void OnNodeProcess()
		{
			if (inputPartialBiome == null)
			{
				Debug.LogError("[NodeBiomeDataDecomposer]: Null input partial biome data");
				return ;
			}

			var biomeData = inputPartialBiome.biomeDataReference;

			if (biomeData == null)
				return ;

			outputBiomeData = inputPartialBiome;
			outputTerrain = biomeData.GetSampler(BiomeSamplerName.terrainHeight);
			outputTemperatureMap = biomeData.GetSampler(BiomeSamplerName.temperature);
			outputWetnessMap = biomeData.GetSampler(BiomeSamplerName.wetness);
		}
		
	}
}