using System.Collections.Generic;
using UnityEngine;
using System;
using PW.Biomator;

namespace PW.Core
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

		public override void Resize(int size)
		{
			this.size = size;
			blendMap = new BiomeBlendPoint[size * size];
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

		public override void Resize(int size)
		{
			this.size = size;
			blendMap = new BiomeBlendPoint[size * size * size];
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
			
			System.Buffer.BlockCopy(blendMap, 0, newSampler.blendMap, 0, blendMap.Length);

			return newSampler;
		}
	}

	public class BiomeData
	{
		public BiomeSwitchTree		biomeTree;
		public PWNode				biomeTreeStartPoint;

		public bool					isWaterless;

		//biome disosition maps (can be 2D or 3D)
		public BiomeMap2D			biomeIds;
		public BiomeMap3D			biomeIds3D;

		public float				waterLevel;
		public Sampler2D			waterHeight;

		public Sampler2D			terrain;
		public Sampler3D			terrain3D;
		public Sampler				terrainRef { get { return (terrain == null) ? terrain3D : terrain as Sampler; } }

		public Sampler2D			wetness;
		public Sampler3D			wetness3D;
		public Sampler				wetnessRef { get { return (wetness == null) ? wetness3D : wetness as Sampler; } }

		public Sampler2D			temperature;
		public Sampler3D			temperature3D;
		public Sampler				temperatureRef { get { return (temperature == null) ? temperature3D : temperature as Sampler; } }
		
		public Vector2Sampler2D		wind;
		public Vector3Sampler2D		wind3D;
		public Sampler				windRef { get { return (wind == null) ? wind3D : wind as Sampler; } }
		public Sampler2D			lighting;
		
		public Sampler2D			air;
		public Sampler3D			air3D;
		public Sampler				airRef { get { return (air == null) ? air3D : air as Sampler; } }
		
		public ComplexEdaphicData	soil;

		public Sampler2D[]			datas;
		public Sampler3D[]			datas3D;
		public string[]				dataNames;

		public BiomeData()
		{
			biomeTree = new BiomeSwitchTree();
			datas = new Sampler2D[9];
			datas3D = new Sampler3D[9];
			isWaterless = true;
		}
	}
	
	public enum SurfaceMapType
	{
		Basic,
		Normal,
		Complex,
	}

	[Serializable]
	public class BiomeSurfaceMaps
	{
		[SerializeField]
		SurfaceMapType		type;

		public string		name;

		public Texture2D	albedo;
		public Texture2D	secondAlbedo;
		public Texture2D	diffuse;
		public Texture2D	normal;
		public Texture2D	secondNormal;
		public Texture2D	height;
		public Texture2D	emissive;
		public Texture2D	specular;
		public Texture2D	opacity;
		public Texture2D	smoothness;
		public Texture2D	ambiantOcculison;
		public Texture2D	detailMask;
		public Texture2D	metallic;
		public Texture2D	roughness;
		public Texture2D	displacement;
		public Texture2D	tesselation;
		
		public Color		temperatureColorModifier;
		public Color		wetnessColorModifier;
	}

	[Serializable]
	public class BiomeSurfaceSlopeMaps
	{
		public float					minSlope;
		public float					maxSlope;

		public float					y;

		public BiomeSurfaceMaps 		surfaceMaps;
	}

	[Serializable]
	public class BiomeSurfaceLayer
	{
		public float							minHeight;
		public float							maxHeight;

		public string							name = "Layer name";
		public bool								foldout = false;

		public List< BiomeSurfaceSlopeMaps	>	slopeMaps = new List< BiomeSurfaceSlopeMaps >();
	}

	[Serializable]
	public class BiomeSurfaces
	{
		public List< BiomeSurfaceLayer >	biomeLayers = new List< BiomeSurfaceLayer >();
	}

	public class Biome
	{
		public BiomeData				biomeDataReference;
		public PWGraphTerrainType		mode;
		public string					name;
		public Color					previewColor;
		public short					id;
		public BiomeTerrain				biomeTerrain;

		public BiomeSurfaces			biomeSurfaces;
	}

	public class BlendedBiomeTerrain
	{
		public BiomeSwitchTree		biomeTree;
		public BiomeMap2D			biomeMap;
		public BiomeMap3D			biomeMap3D;

		public Sampler				terrain;
		public Sampler				biomeTerrain;
		public Sampler				waterHeight;
		public Sampler				wetnessMap;
		public Sampler				temperatureMap;
		public Sampler				windMap;
		public Sampler				lightingMap;
		public Sampler				airMap;

		public Texture2DArray		terrainTextureArray;
	}
	
}