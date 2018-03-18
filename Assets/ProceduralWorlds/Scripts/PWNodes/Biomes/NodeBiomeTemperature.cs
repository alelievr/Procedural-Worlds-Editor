using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Node
{
	public class NodeBiomeTemperature : BaseNode
	{

		[Input("biome datas")]
		public BiomeData		inputBiomeData;
		[Input("temperature map"), NotRequired]
		public Sampler			temperatureMap;

		public Sampler			localTemperatureMap;
		
		[Output]
		public BiomeData		outputBiome;

		public float			minTemperature = -30;
		public float			maxTemperature = 40;
		public float			terrainHeightMultiplier = 0;
		public float			waterMultiplier = .2f;
		public float			averageTemperature = 17;

		public bool				internalTemperatureMap = true;

		public float			minTemperatureMapInput = 0.2f;
		public float			maxTemperatureMapInput = 0.6f;

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
			}
		}
		
		public override void OnNodeAnchorUnlink(string prop, int index)
		{
			if (prop == "temperatureMap")
			{
				internalTemperatureMap = true;
				temperatureMap = null;
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
				{
					float wh = waterHeight.At(x, y, false);
					wh /= (waterHeight.max - waterHeight.min) / 2;
					waterMod = wh * waterMultiplier * temperatureRange;
				}
				return Mathf.Clamp(mapValue + terrainMod + waterMod, minTemperature, maxTemperature);
			});

			localTemperatureMap.min = minTemperature;
			localTemperatureMap.max = maxTemperature;

			if (inputBiomeData != null)
				inputBiomeData.UpdateSamplerValue(BiomeSamplerName.temperature, localTemperatureMap);
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
