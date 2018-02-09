using System.Collections.Generic;
using UnityEngine;
using System;
using PW.Biomator;
using PW.Core;

namespace PW.Biomator
{
	/*
	**	Biomes
	*/

	[Serializable]
	public enum BiomeDataType
	{
		BiomeData,
		BiomeData3D,
		WaterlessBiomeData,
		WaterlessBiomeData3D,
	}

	[Serializable]
	public class BasicEdaphicData
	{
		public Sampler2D		PH;
		public Sampler2D		drainage;
		public Sampler2D		nutrient;
		public Sampler2D		mineral;
	}

	[System.SerializableAttribute]
	public class ComplexEdaphicData : BasicEdaphicData
	{
		public Sampler2D		clay;
		public Sampler2D		silt;
		public Sampler2D		sand;
		public Sampler2D		gravel;
	}
	
	[System.SerializableAttribute]
	public class BasicEdaphicData3D
	{
		public Sampler3D		PH;
		public Sampler3D		drainage;
		public Sampler3D		nutrient;
		public Sampler3D		mineral;
	}

	[Serializable]
	public class ComplexEdaphicData3D : BasicEdaphicData
	{
		public Sampler3D		clay;
		public Sampler3D		silt;
		public Sampler3D		sand;
		public Sampler3D		gravel;
	}

	public struct BiomeBlendPoint
	{
		public short	firstBiomeId;
		public short	secondBiomeId;
        public float	firstBiomeBlendPercent;
        public float	secondBiomeBlendPercent;
    }

	public class BiomeMap2D : Sampler
	{
		public bool			init = false;
		BiomeBlendPoint[]	blendMap;

		public BiomeMap2D(int size, float step)
		{
			this.step = step;
			Resize(size);
		}

		public override void Resize(int size, float step = -1)
		{
			this.size = size;
			blendMap = new BiomeBlendPoint[size * size];
			this.step = (step == -1) ? this.step : step;
		}
		
		public void SetFirstBiomeId(int x, int y, short id)
		{
			int		i = x + y * size;
			blendMap[i].firstBiomeId = id;
			blendMap[i].firstBiomeBlendPercent = 1;
			//TODO: blending here
		}

		public BiomeBlendPoint	GetBiomeBlendInfo(int x, int y)
		{
			return blendMap[x + y * size];
		}
		
		public override Sampler Clone(Sampler reuseObject)
		{
			BiomeMap2D	newSampler;

			if (reuseObject != null)
			{
				newSampler = reuseObject as BiomeMap2D;
				if (newSampler.size != size)
					newSampler.Resize(size);
			}
			else
				newSampler = new BiomeMap2D(size, step);
				
			newSampler.min = min;
			newSampler.max = max;
			
			System.Buffer.BlockCopy(blendMap, 0, newSampler.blendMap, 0, blendMap.Length);

			return newSampler;
		}
	}

	public class BiomeMap3D : Sampler
	{
		BiomeBlendPoint[]	blendMap;
		
		public BiomeMap3D(int size, float step)
		{
			this.step = step;
			Resize(size);
		}

		public override void Resize(int size, float step = -1)
		{
			this.size = size;
			blendMap = new BiomeBlendPoint[size * size * size];
			this.step = (step == -1) ? this.step : step;
		}

		public BiomeBlendPoint this[int x, int y, int z]
		{
			get { return blendMap[x + y * size + z * size * size]; }
		}
		
		public override Sampler Clone(Sampler reuseObject)
		{
			BiomeMap3D	newSampler;

			if (reuseObject != null)
			{
				newSampler = reuseObject as BiomeMap3D;
				if (newSampler.size != size)
					newSampler.Resize(size);
			}
			else
				newSampler = new BiomeMap3D(size, step);
				
			newSampler.min = min;
			newSampler.max = max;
			
			System.Buffer.BlockCopy(blendMap, 0, newSampler.blendMap, 0, blendMap.Length);

			return newSampler;
		}
	}

	public class BiomeData
	{
		// public BiomeSwitchTree		biomeTree;
		public BiomeSwitchGraph		biomeSwitchGraph;
		// public PWNode				biomeTreeStartPoint;
		public PWNode				biomeSwitchGraphStartPoint;

		public bool					isWaterless;

		//biome disosition maps (can be 2D or 3D)
		public BiomeMap2D			biomeIds;
		public BiomeMap3D			biomeIds3D;

		public float				waterLevel;
		public Sampler2D			waterHeight;

		public Sampler2D			terrain;
		public Sampler3D			terrain3D;
		public Sampler				terrainRef
		{
			get { return (terrain == null) ? terrain3D : terrain as Sampler; }
			set { terrain = value as Sampler2D; terrain3D = value as Sampler3D; }
		}

		public Sampler2D			wetness;
		public Sampler3D			wetness3D;
		public Sampler				wetnessRef
		{
			get { return (wetness == null) ? wetness3D : wetness as Sampler; }
			set { wetness = value as Sampler2D; wetness3D = value as Sampler3D; }
		}

		public Sampler2D			temperature;
		public Sampler3D			temperature3D;
		public Sampler				temperatureRef
		{
			get { return (temperature == null) ? temperature3D : temperature as Sampler; }
			set { temperature = value as Sampler2D; temperature3D = value as Sampler3D; }
		}
		
		public Vector2Sampler2D		wind;
		public Vector3Sampler2D		wind3D;
		public Sampler				windRef
		{
			get { return (wind == null) ? wind3D : wind as Sampler; }
			set { wind = value as Vector2Sampler2D; wind3D = value as Vector3Sampler2D; }
		}
		public Sampler2D			lighting;
		
		public Sampler2D			air;
		public Sampler3D			air3D;
		public Sampler				airRef
		{
			get { return (air == null) ? air3D : air as Sampler; }
			set { air = value as Sampler2D; air3D = value as Sampler3D; }
		}
		
		public ComplexEdaphicData	soil;

		public Sampler2D[]			datas;
		public Sampler3D[]			datas3D;
		public string[]				dataNames;

		public List< short >		ids = new List< short >();

		public BiomeData()
		{
			// biomeTree = new BiomeSwitchTree();
			biomeSwitchGraph = new BiomeSwitchGraph();
			datas = new Sampler2D[9];
			datas3D = new Sampler3D[9];
			isWaterless = true;
		}
	}
	
	public enum BiomeSurfaceType
	{
		SurfaceMaps,
		Color,
		Material,
	}
	
	public enum SurfaceMapsType
	{
		Basic,
		Normal,
		Complex,
	}

	[Serializable]
	public class BiomeSurfaceMaps
	{
		[SerializeField]
		public SurfaceMapsType	type;

		public string			name;

		public Texture2D		albedo;
		public Texture2D		secondAlbedo;
		public Texture2D		normal;
		public Texture2D		secondNormal;
		public Texture2D		height;
		public Texture2D		emissive;
		public Texture2D		specular;
		public Texture2D		opacity;
		public Texture2D		smoothness;
		public Texture2D		ambiantOcculison;
		public Texture2D		detailMask;
		public Texture2D		metallic;
		public Texture2D		roughness;
		public Texture2D		displacement;
		public Texture2D		tesselation;
		
		public Color			temperatureColorModifier;
		public Color			wetnessColorModifier;
	}

	[Serializable]
	public class BiomeSurfaceColor
	{
		public Color		baseColor;

		public bool			colorOverParamEnabled;
		public float		minRange;
		public float		maxRange;
		public Color		colorOverParam;
	}

	[Serializable]
	public class BiomeSurfaceMaterial
	{
		public Material		material;

		public List< string >	propertiesOverParams = new List< string >();
	}

	[Serializable]
	public class BiomeSurface
	{
		//test variable:
		public string				name;

		public BiomeSurfaceType		type; //same as PWBiomeGraph.surfaceType
		public BiomeSurfaceMaps		maps;
		public BiomeSurfaceColor	color;
		public BiomeSurfaceMaterial	material;
	}

	public class BiomeDetail
	{
		//list of model, placing algo ...
	}

	public class BiomeDetails : List< BiomeDetail >
	{
		public void Add()
		{
			BiomeDetail	bd = new BiomeDetail();

			this.Add(bd);
		}
	}

	public class PartialBiome
	{
		public BiomeData				biomeDataReference;
		public string					name;
		public Color					previewColor;
		public short					id;
		public PWBiomeGraph				biomeGraph;

		public override string ToString()
		{
			return "Partial biome '" + name +"', id: " + id + ", data: " + biomeDataReference;
		}
	}

	public class Biome
	{
		public BiomeData				biomeDataReference;
		public string					name;
		public Color					previewColor;
		public short					id;

		//datas added by the biome graph
		public Sampler					modifiedTerrain;
		public BiomeSurfaceGraph		biomeSurfaceGraph;
	}

	public class BlendedBiomeTerrain
	{
		public BiomeSwitchTree		biomeTree;
		public BiomeData			biomeData;

		public List< Biome >		biomes = new List< Biome >();
	}
	
}