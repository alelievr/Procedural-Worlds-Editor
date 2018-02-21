using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeTemperature : PWNode
	{

		[PWInput("biome datas")]
		public BiomeData		inputBiomeData;
		[PWInput("temperature map"), PWNotRequired]
		public Sampler			temperatureMap;

		public Sampler			localTemperatureMap;
		
		[PWOutput]
		public BiomeData		outputBiome;

		public float			minTemperature = -30;
		public float			maxTemperature = 40;
		public float			terrainHeightMultiplier = 0;
		public float			waterMultiplier = .2f;
		public float			averageTemperature = 17;

		public bool				updateTemperatureMap = false;
		public bool				internalTemperatureMap = true;

		public float			minTemperatureMapInput;
		public float			maxTemperatureMapInput;

		public override void OnNodeCreation()
		{
			name = "Temperature node";
		}

		public override void OnNodeEnable()
		{
			UpdateTemperatureMap();
		}

		public override void OnNodeAnchorLink(string prop, int index)
		{
			if (prop == "temperatureMap")
			{
				internalTemperatureMap = false;
				UpdateTemperatureMap();
			}
		}
		
		public override void OnNodeAnchorUnlink(string prop, int index)
		{
			if (prop == "temperatureMap")
			{
				internalTemperatureMap = true;
				temperatureMap = null;
				UpdateTemperatureMap();
			}
		}

		public void UpdateTemperatureMap()
		{
			if (localTemperatureMap == null || inputBiomeData == null)
				return ;
			
			var inputTemperatureMap = temperatureMap as Sampler2D;
				
			var terrain = inputBiomeData.GetSampler2D(BiomeSamplerName.terrainHeight);
			var waterHeight = inputBiomeData.GetSampler2D(BiomeSamplerName.waterHeight);

			(localTemperatureMap as Sampler2D).Foreach((x, y, val) => {
				float	terrainMod = 0;
				float	waterMod = 0;
				float	temperatureRange = (maxTemperature - minTemperature);
				float	mapValue = averageTemperature;

				if (!internalTemperatureMap)
					mapValue = Mathf.Lerp(Mathf.Max(minTemperature, minTemperatureMapInput), Mathf.Min(maxTemperature, maxTemperatureMapInput), inputTemperatureMap[x, y]);

				if (terrainHeightMultiplier != 0 && terrain != null)
					terrainMod = terrain.At(x, y, true) * terrainHeightMultiplier * temperatureRange;
				if (waterMultiplier != 0 && waterHeight != null)
					waterMod = waterHeight.At(x, y, true) * waterMultiplier * temperatureRange;
				return Mathf.Clamp(mapValue + terrainMod + waterMod, minTemperature, maxTemperature);
			});

			localTemperatureMap.min = minTemperature;
			localTemperatureMap.max = maxTemperature;

			if (inputBiomeData != null)
				inputBiomeData.UpdateSamplerValue(BiomeSamplerName.temperature, localTemperatureMap);
				
			updateTemperatureMap = true;
		}

		void CreateTemperatureMapIfNotExists()
		{
			if (temperatureMap == null)
			{
				localTemperatureMap = new Sampler2D(chunkSize, step);
				return ;
			}

			if (localTemperatureMap == null || localTemperatureMap.NeedResize(chunkSize, step))
			{
				localTemperatureMap = temperatureMap.Clone(localTemperatureMap);
				return ;
			}
		}

		public override void OnNodeProcess()
		{
			CreateTemperatureMapIfNotExists();

			UpdateTemperatureMap();

			outputBiome = inputBiomeData;
		}

		//to prebuild the biome tree:
		public override void OnNodeProcessOnce()
		{
			outputBiome = inputBiomeData;
		}

	}
}
