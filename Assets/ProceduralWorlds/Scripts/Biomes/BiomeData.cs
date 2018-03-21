using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Biomator
{
	public static class BiomeSamplerName
	{
		public const string	terrainHeight = "Terrain height";
		public const string	waterHeight = "Water height";
		public const string	temperature = "Temperature";
		public const string	wetness = "Wetness";

		public static IEnumerable< string >	GetNames()
		{
			yield return terrainHeight;
			yield return waterHeight;
			yield return temperature;
			yield return wetness;
		}
	}

	public class BiomeData
	{
		public const int		maxBiomeSamplers = 10;

		//number of samplers in the current biomeData
		public int	length;

		public class BiomeDataSampler
		{
			public Sampler		dataRef
			{
				get { return data2D != null ? data2D as Sampler : data3D as Sampler; }
				set { data2D = value as Sampler2D; data3D = value as Sampler3D; }
			}

			public Sampler2D	data2D;
			public Sampler3D	data3D;
			public bool			is3D = false;
			public string		key = null;
			public int			index = 0;
			public bool			enabled = false;
		}

		public BiomeDataSampler[]	biomeSamplers = new BiomeDataSampler[maxBiomeSamplers];
		public Dictionary< string, BiomeDataSampler > biomeSamplerNameMap = new Dictionary< string, BiomeDataSampler >();

		//list of biome ids
		public HashSet< short >		ids = new HashSet< short >();

		public bool					isWaterless = true;
		public float				waterLevel;

		//data for the switch graph (to choose biomes using biomeSamplers)
		public BiomeSwitchGraph		biomeSwitchGraph;
		public BaseNode				biomeSwitchGraphStartPoint;

		//biome maps
		public BiomeMap2D			biomeMap;
		public BiomeMap3D			biomeMap3D;

		public ComplexEdaphicData	soil;

		public BiomeData()
		{
			biomeSwitchGraph = new BiomeSwitchGraph();
		}

		public void Reset()
		{
			ids.Clear();
			biomeSamplerNameMap.Clear();

			for (int i = 0; i < length; i++)
				biomeSamplers[i] = null;

			length = 0;
		}

		public void UpdateSamplerValue(string key, Sampler value)
		{
			if (!biomeSamplerNameMap.ContainsKey(key))
			{
				if (length >= maxBiomeSamplers)
				{
					Debug.LogError("Can't add biome sampler " + key + ", sampler limit reached");
					return ;
				}

				BiomeDataSampler dataSampler = new BiomeDataSampler();
				dataSampler.enabled = true;
				dataSampler.index = length;
				dataSampler.key = key;
				biomeSamplers[length] = dataSampler;
				biomeSamplerNameMap[key] = dataSampler;

				length++;
			}

			biomeSamplerNameMap[key].dataRef = value;
			biomeSamplerNameMap[key].is3D = value is Sampler3D;
		}
		
		public BiomeDataSampler GetDataSampler(int index)
		{
			return biomeSamplers[index];
		}

		public BiomeDataSampler GetDataSampler(string key)
		{
			BiomeDataSampler ret;

			biomeSamplerNameMap.TryGetValue(key, out ret);

			return ret;
		}

		public Sampler	GetSampler(int index)
		{
			if (biomeSamplers[index] == null)
				return null;
			
			return biomeSamplers[index].dataRef;
		}

		public Sampler	GetSampler(string key)
		{
			var data = GetDataSampler(key);

			if (data == null)
				return null;
			
			return data.dataRef;
		}
		
		public Sampler2D GetSampler2D(int key)
		{
			return GetSampler(key) as Sampler2D;
		}

		public Sampler3D GetSampler3D(int key)
		{
			return GetSampler(key) as Sampler3D;
		}

		public Sampler2D GetSampler2D(string key)
		{
			return GetSampler(key) as Sampler2D;
		}

		public Sampler3D GetSampler3D(string key)
		{
			return GetSampler(key) as Sampler3D;
		}

		public string GetBiomeKey(int index)
		{
			return biomeSamplers[index].key;
		}

		public int GetBiomeIndex(string key)
		{
			BiomeDataSampler dataSampler;
			biomeSamplerNameMap.TryGetValue(key, out dataSampler);
			
			if (dataSampler == null)
				return -1;
			
			return dataSampler.index;
		}
	}
}