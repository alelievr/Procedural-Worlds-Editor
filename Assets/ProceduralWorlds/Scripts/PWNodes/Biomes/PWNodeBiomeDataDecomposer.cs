using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeDataDecomposer : PWNode
	{

		[PWInput("Biome Data")]
		public BiomeData		inputBiomeData;

		[PWOutput("Biome Data")]
		public BiomeData		outputBiomeData;

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

		public override void OnNodeGUI()
		{
		}

		public override void OnNodeProcess()
		{
			if (inputBiomeData == null)
			{
				Debug.LogError("[PWNodeBiomeDataDecomposer]: Null input biome data");
				return ;
			}

			outputTerrain = inputBiomeData.terrainRef;
			outputTemperatureMap = inputBiomeData.temperatureRef;
			outputWetnessMap = inputBiomeData.wetnessRef;
		}
		
	}
}