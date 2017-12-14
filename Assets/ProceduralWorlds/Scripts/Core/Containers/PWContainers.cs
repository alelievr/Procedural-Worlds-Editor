using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using PW.Biomator;
using PW.Core;

namespace PW.Core
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
		public Sampler				waterHeight;
		public Sampler				wetnessMap;
		public Sampler				temperatureMap;
		public Sampler				windMap;
		public Sampler				lightingMap;
		public Sampler				airMap;

		//TODO: save vertex datas to the disk
		public BiomeMap2D			biomeMap;
		public BiomeMap3D			biomeMap3D;
		public Texture2DArray		albedoMaps;

		public List< PWBiomeGraph >	biomes;

		public override string ToString()
		{
			return GetType().Name + "(" + size + ")";
		}
	}

	[Serializable]
	public class SideView2DData : ChunkData
	{
	}
	
	[Serializable]
	public class TopDown2DData : ChunkData
	{
		//TODO for chunk saving to file: encode image to png and store path.
		[System.NonSerializedAttribute]
		public Texture2D	texture;
	}

	//TODO: other storage classes
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

	public enum	TerrainDetailType : Int16
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
		//TODO
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

	/*
	**	Utils
	*/

	[Serializable]
	public struct Vector3i
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
	}

	[Serializable]
	public class Pair< T, U >
	{
		[SerializeField]
		public T	first;
		[SerializeField]
		public U	second;

		public Pair(T f, U s)
		{
			first = f;
			second = s;
		}
	}

	/*
	**	PWNode render settings
	*/

	public enum PWGUIFieldType
	{
		Color,
		Text,
		Slider,
		IntSlider,
		TexturePreview,
		Sampler2DPreview,
		BiomeMapPreview,
		Texture2DArrayPreview,
	}

	[Serializable]
	public class PWGUISettings
	{
		public bool				active {get;  private set;}
		public Vector2			windowPosition;
		public PWGUIFieldType	fieldType;

		[System.NonSerializedAttribute]
		public object	oldState = null;

		//we put all possible datas for each inputs because unity serialization does not support inheritence :(
		
		//colorPicker:
		public SerializableColor	c;
		public Vector2				thumbPosition;

		//Sampler2D:
		public FilterMode			filterMode;
		public SerializableGradient	serializableGradient;
		[System.NonSerialized]
		public bool					update;
		public bool					debug;
		[System.NonSerialized]
		public Rect					savedRect;

		[System.NonSerialized]
		public Gradient				gradient;
		[System.NonSerialized]
		public Texture2D			texture;

		//Texture:
		// public FilterMode		filterMode; //duplicated
		public ScaleMode			scaleMode;
		public float				scaleAspect;
		//TODO: light-weight serializableMaterial
		[System.NonSerialized]
		public Material				material;

		//Texture2DArray:
		[System.NonSerialized]
		public Texture2D[]			textures;

		//Editor utils:
		[System.NonSerialized]
		public int					popupHeight;

		//Sampler value to update textures:
		[System.NonSerialized]
		public Sampler2D			sampler2D;
		[System.NonSerialized]
		public BiomeData			biomeData;
		
		public PWGUISettings()
		{
			active = false;
			update = false;
		}

		public object Active(object o)
		{
			active = true;
			oldState = o;
			return o;
		}

		public object InActive()
		{
			active = false;
			return oldState;
		}

		public object Invert(object o)
		{
			if (active)
				return InActive();
			else
				return Active(o);
		}
	}
}
