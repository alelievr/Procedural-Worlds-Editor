using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using ProceduralWorlds.Noises;
using ProceduralWorlds.Node;

namespace ProceduralWorlds.Biomator
{
	[System.Serializable]
	public class BiomeDataInputGenerator
	{
		public int			seed;
		public int			octaves = 3;
		public float		persistance = .85f;
		public float		lacunarity = 1.5f;
		public int			size = 32;
		public float		step = 1;
		public int			maxTerrainHeight = 100;
		public bool			isWaterless;
		public float		waterLevel = 62;

		BiomeMap2D Generate2DBiomeMap(short biomeId)
		{
			BiomeMap2D biomeMap2D = new BiomeMap2D(size, step);

			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					biomeMap2D.AddBiome(x, y, biomeId, 1);
				
			return biomeMap2D;
		}

		Sampler2D GenerateWaterHeight(Sampler2D terrainHeight)
		{
			Sampler2D waterHeight = new Sampler2D(terrainHeight.size, terrainHeight.step);

			terrainHeight.Foreach((x, y, val) => {
				waterHeight[x, y] = waterLevel - val;
			});

			return waterHeight;
		}

		BiomeData Generate2DBiomeData()
		{
			BiomeData biomeData = new BiomeData();
			BiomeSwitchGraph switchGraph = new BiomeSwitchGraph();
			Sampler2D terrainHeight = new Sampler2D(size, step);
			PerlinNoise2D perlin = new PerlinNoise2D(seed);

			perlin.UpdateParams(seed, step, octaves, persistance, lacunarity);

			perlin.ComputeSampler2D(terrainHeight);

			terrainHeight = NoiseFunctions.Map(terrainHeight, 0, maxTerrainHeight);

			biomeData.waterLevel = waterLevel;
			biomeData.isWaterless = isWaterless;
			biomeData.UpdateSamplerValue(BiomeSamplerName.terrainHeight, terrainHeight);
			if (!isWaterless)
				biomeData.UpdateSamplerValue(BiomeSamplerName.waterHeight, GenerateWaterHeight(terrainHeight));
			
			switchGraph.BuildTestGraph(0);

			biomeData.biomeMap = Generate2DBiomeMap(0);
			biomeData.biomeSwitchGraph = switchGraph;
			return biomeData;
		}

		public PartialBiome		GeneratePartialBiome2D(BiomeGraph biomeGraph)
		{
			PartialBiome partialBiome = new PartialBiome();

			partialBiome.biomeDataReference = Generate2DBiomeData();

			return partialBiome;
		}
	}
}