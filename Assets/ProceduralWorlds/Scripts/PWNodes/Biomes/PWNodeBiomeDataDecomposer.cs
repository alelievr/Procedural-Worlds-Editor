using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeDataDecomposer : PWNode
	{

		[PWInput("Partial Biome")]
		public PartialBiome		inputPartialBiome;

		[PWOutput("Partial Biome")]
		public PartialBiome		outputBiomeData;

		[PWOutput("Terrain")]
		public Sampler			outputTerrain;

		[PWOutput("Temperature map")]
		public Sampler			outputTemperatureMap;

		[PWOutput("Wetness map")]
		public Sampler			outputWetnessMap;

		public override void OnNodeCreation()
		{
			name = "BiomeData decomposer";
		}

		public override void OnNodeEnable()
		{
		}

		public override void OnNodeProcess()
		{
			if (inputPartialBiome == null)
			{
				Debug.LogError("[PWNodeBiomeDataDecomposer]: Null input partial biome data");
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