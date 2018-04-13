using System.Collections.Generic;
using UnityEngine;
using System;
using ProceduralWorlds.Biomator;
using ProceduralWorlds.Core;
using System.Linq;

namespace ProceduralWorlds.Biomator
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

	public struct BiomeBlendPoint : IEquatable< BiomeBlendPoint >
	{
		public int		length;

		public short[]	biomeIds;
		public float[]	biomeBlends;

		public short	firstBiomeId { get { return biomeIds[0]; } }
		public float	firstBiomePercent { get { return biomeBlends[0]; } }

		public bool Equals(BiomeBlendPoint other)
		{
			return length == other.length
				&& firstBiomeId == other.firstBiomeId
				&& firstBiomePercent == other.firstBiomePercent
				&& biomeIds.SequenceEqual(other.biomeIds)
				&& biomeBlends.SequenceEqual(other.biomeBlends);
		}

		public void		SetBlendPoint(short id, float blend, int index = -1)
		{
			if (index == -1)
				index = length;
			
			if (index == 0)
			{
				length = 0;
			}
			
			//if there is too many biome to blend, discard
			if (index > biomeIds.Length)
				return ;
			
			biomeIds[index] = id;
			biomeBlends[index] = blend;
			
			length++;
		}
    }

	public class BiomeMap2D : Sampler
	{
		public bool			init = false;
		BiomeBlendPoint[]	blendMap;

		readonly int		maxBiomeBlend = 4;

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
			
			for (int i = 0; i < blendMap.Length; i++)
			{
				blendMap[i].biomeIds = new short[maxBiomeBlend];
				blendMap[i].biomeBlends = new float[maxBiomeBlend];
			}
		}
		
		public void SetPrimaryBiomeId(int x, int y, short id)
		{
			int		i = x + y * size;

			blendMap[i].SetBlendPoint(id, 1, 0);
		}

		public void NormalizeBlendValues(int x, int y)
		{
			//compute first blend percent (the others are already calculated):
			var biomePoint = blendMap[x + y * size];

			if (biomePoint.length == 1)
				return ;

			float a = 0;
			float total = 0;
			for (int i = 1; i < biomePoint.length - 1; i++)
				total += a += biomePoint.biomeBlends[i];
			
			a /= biomePoint.length - 1;

			biomePoint.biomeBlends[0] = 1 - a;

			total += biomePoint.biomeBlends[0];

			//Normalize all values:
			for (int i = 0; i < biomePoint.length; i++)
				biomePoint.biomeBlends[i] /= total;
		}

		public void AddBiome(int x, int y, short id, float blend)
		{
			int i = x + y * size;

			blendMap[i].SetBlendPoint(id, blend);
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
				newSampler.ResizeIfNeeded(size, step);
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
				newSampler.ResizeIfNeeded(size, step);
			}
			else
				newSampler = new BiomeMap3D(size, step);
				
			newSampler.min = min;
			newSampler.max = max;
			
			System.Buffer.BlockCopy(blendMap, 0, newSampler.blendMap, 0, blendMap.Length);

			return newSampler;
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

		public BiomeSurfaceType		type; //same as BiomeGraph.surfaceType
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
		public BiomeGraph				biomeGraph;

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
		public BiomeSwitchGraph		biomeSwitchGraph;
		public BiomeData			biomeData;

		public List< Biome >		biomes = new List< Biome >();
	}
	
}