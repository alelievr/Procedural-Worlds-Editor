using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using ProceduralWorlds.Biomator;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Core
{
	/*
	**	Parent class to store everything needed to render a chunk
	*/
	[Serializable]
	public abstract class ChunkData
	{
		public int					size;
		public MaterializerType		materializerType;

		public Sampler				terrain;

		public BiomeMap2D			biomeMap;
		public BiomeMap3D			biomeMap3D;

		public override string ToString()
		{
			return GetType().Name + "(" + size + ")";
		}
	}

	[Serializable]
	public class TopDownChunkData : ChunkData
	{
		
	}

	public enum CaveType
	{
		PerlinWorms,
		CellularAutomata,
	}

	public enum TerrainDetailBlendMode
	{
		Additive,
		Subtractive,
		Max,
		Min,
	}

	public enum	TerrainDetailType
	{
		River,
		Lake,
		Ravine,
/*		FractalRiverBassin,
		Volcano,
		UndergroundRiver,
		UnderGroundLake,
		Caves,
		Holes,
*/
	}

	/*
	**	This structure contains all information about landforms to apply to terrain during the building
	*/
	[Serializable]
	public class TerrainDetail
	{
		public int			biomeDetailMask = (int)TerrainDetailType.River | (int)TerrainDetailType.Lake;

		//Datas for rivers:
		public int			sourcePointMinHeight = 70;
		public int			sourcePointMaxHeight = 100;
		public float		noiseIntensity;
		public float		sourceRarity = 0.5f;

		//Datas for Lakes:
		public int			lakeMinHeight = 70;
		public int			lakeMaxHeight = 100;
		public float		lakeSize = 0.5f;

		//Datas for Ravines:

		//...
	}

	//Datas stored for river / lakes / oth precomputing
	public class GeologicBakedDatas
	{
		public Sampler2D		waterBodies;

		//TODO: other geologic datas (for geologic update :)
	}

	public enum MaterializerType
	{
		//2D materializers:
		SquareTileMap,
		// HexTileMap,
		// MarchingSquare,

		//3D:
		// MarchingCube,
		// ExtendedMarchingCube,
		// DualCountering,
		// SurfaceNets,
		// GreedyMeshing,
		// MonotoneMeshing,
	}
	
	public class FinalTerrain
	{
		public BiomeData				biomeData;
		public Sampler					mergedTerrain;
		public MaterializerType			materializerType;

		public Dictionary< short, BiomeSurfaceGraph >	biomeSurfacesList = new Dictionary< short, BiomeSurfaceGraph >();
	}

	/*
	**	Utils
	*/

	[Serializable]
	public struct Vector3i : IEquatable< Vector3i >
	{
		public int	x;
		public int	y;
		public int	z;

		public Vector3i(float x, float y, float z)
		{
			this.x = (int)x;
			this.y = (int)y;
			this.z = (int)z;
		}

		public Vector3i(float a)
		{
			this.x = (int)a;
			this.y = (int)a;
			this.z = (int)a;
		}

		public static explicit operator Vector3(Vector3i v)
		{
			return new Vector3(v.x, v.y, v.z);
		}

		public static implicit operator Vector3i(Vector3 v)
		{
			return new Vector3i(v.x, v.y, v.z);
		}

		public bool Equals(Vector3i other)
		{
			return other.x == x && other.y == y && other.z == z;
		}
	}
}
