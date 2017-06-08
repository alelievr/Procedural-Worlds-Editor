using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;

namespace PW.Biomator
{
	public static class BiomeBlending {

		//TODO: move this to C++

		//TODO: possibility to have a different step for biome maps
		
		public static void BlendTopDownd2D(List< Biome > inputBiomeMap, BiomeData biomeData)
		{
			var		tree = biomeData.biomeTree;
			bool	is3D = false;

			//check the maps dimentions (if one map is in 3D, the output biome map will be in 3D)
			if (biomeData.air3D != null || biomeData.datas3D != null || biomeData.wind3D != null || biomeData.wetness3D != null || biomeData.temperature3D != null)
				is3D = true;

			if (is3D)
			{
				biomeData.biomes3D = new Sampler3D(biomeData.terrain.size, biomeData.terrain.step);
			}
			else
			{
				biomeData.biomes = new Sampler2D(biomeData.terrain.size, biomeData.terrain.step);

				biomeData.biomes.Foreach((x, y) => {
					float	val = 0;

					//TODO: 3D terrain management

					float temp = (biomeData.temperature != null) ? biomeData.temperature[x, y] : 0;
					float wet = (biomeData.wetness != null) ? biomeData.wetness[x, y] : 0;
					bool wat = (biomeData.isWaterless) ? false : biomeData.terrain[x, y] - biomeData.waterHeight[x, y] < 0;
					
					//ignore other settings for the moment
					
					tree.GetBiome(wat, biomeData.terrain[x, y], temp, wet, 0, 0, 0);

					return val;
				});
			}
			//TODO: find the biomes by temp / wet / ...

			//Lerp the terrain materials
		}

		public static void BlendPlanar3D(List< Biome > inputBiomeMap, BiomeData biomeData)
		{
			var tree = biomeData.biomeTree;
			//blend
		}

	}
}