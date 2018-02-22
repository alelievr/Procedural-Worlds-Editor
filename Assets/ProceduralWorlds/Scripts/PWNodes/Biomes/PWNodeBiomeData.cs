using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeData : PWNode
	{

		[PWInput("Terrain input")]
		[PWOffset(5)]
		[System.NonSerialized]
		public Sampler			terrain;

		[PWOutput("Biome datas")]
		[PWOffset(5)]
		[System.NonSerialized]
		public BiomeData		outputBiome;
		
		public float			mapMin;
		public float			mapMax;

		public override void OnNodeCreation()
		{
			mapMin = 0;
			mapMax = 100;
			name = "Terrain to BiomeData";
		}

		public override void OnNodeEnable()
		{
			CreateNewBiomeData();
		}

		void				CreateNewBiomeData()
		{
			outputBiome = new BiomeData();

			outputBiome.isWaterless = true;
			outputBiome.biomeSwitchGraphStartPoint = this;
		}

		public override void OnNodeProcess()
		{
			outputBiome.Reset();
			
			if (terrain != null && terrain.type == SamplerType.Sampler2D)
			{
				//terrain mapping
				outputBiome.UpdateSamplerValue(BiomeSamplerName.terrainHeight, PWNoiseFunctions.Map(terrain as Sampler2D, mapMin, mapMax, true));
			}
		}
	}
}
