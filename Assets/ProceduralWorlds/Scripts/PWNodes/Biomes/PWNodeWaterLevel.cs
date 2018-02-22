using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeWaterLevel : PWNode
	{

		[PWInput("Terrain input")]
		[PWOffset(5)]
		public Sampler		terrainNoise;

		[PWOutput("Biome datas")]
		[PWOffset(5)]
		public BiomeData	outputBiome;

		public float		waterLevel;
		public float		mapMin;
		public float		mapMax;

		public override void OnNodeCreation()
		{
			mapMin = 0;
			mapMax = 100;
			waterLevel = 50;
			name = "Water level";
		}

		public override void OnNodeEnable()
		{
			outputBiome = new BiomeData();

			outputBiome.isWaterless = false;
			outputBiome.biomeSwitchGraphStartPoint = this;
		}
		
		public void UpdateWaterMap()
		{
			outputBiome.UpdateSamplerValue(BiomeSamplerName.terrainHeight, terrainNoise);

			outputBiome.waterLevel = waterLevel;

			if (terrainNoise.type == SamplerType.Sampler2D)
			{
				//terrain mapping
				var mappedTerrain = PWNoiseFunctions.Map(terrainNoise as Sampler2D, mapMin, mapMax, true);
				outputBiome.UpdateSamplerValue(BiomeSamplerName.terrainHeight, mappedTerrain);

				//waterHeight evaluation
				Sampler2D waterHeight = new Sampler2D(terrainNoise.size, terrainNoise.step);
				waterHeight.min = waterLevel - mappedTerrain.max;
				waterHeight.max = waterLevel;
				mappedTerrain.Foreach((x, y, val) => {
					waterHeight[x, y] = waterLevel - val;
				});
				outputBiome.UpdateSamplerValue(BiomeSamplerName.waterHeight, waterHeight);
			}
			else
			{
				//TODO
			}
		}

		public override void OnNodeProcess()
		{
			if (terrainNoise == null)
			{
				Debug.LogError("[PWNodeWaterLevel] null terrain input received !");
				return ;
			}
			
			outputBiome.Reset();

			UpdateWaterMap();
		}
	}
}
