using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AnimatedValues;
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
	public class ChunkData
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

		[System.NonSerialized]
		public Dictionary< short, BiomeSurfaces > biomeTexturing;

		public override string ToString()
		{
			return GetType().Name + "(" + size + ")";
		}
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
		FadeBlock,
	}

	[Serializable]
	public class PWGUISettings
	{
		public Vector2			windowPosition;
		public PWGUIFieldType	fieldType;

		[System.NonSerialized]
		public object	oldState = null;
		[System.NonSerialized]
		public bool		firstRender = true;

		//we put all possible datas for each inputs because unity serialization does not support inheritence :(

		//text field:
		public bool					editing;
		
		//colorPicker:
		public SerializableColor	c;
		public Vector2				thumbPosition;

		//Sampler2D:
		public FilterMode			filterMode;
		public SerializableGradient	serializableGradient;
		[System.NonSerialized]
		public bool					update;
		[System.NonSerialized]
		public bool					samplerTextureUpdated = false;
		public bool					debug;

		//verson of the debug bool only updated durin Layout passes (use this for conditional debug display)
		[SerializeField]
		bool						_frameSafeDebug;
		public bool					frameSafeDebug
		{
			get
			{
				if (Event.current.type == EventType.Layout)
					_frameSafeDebug = debug;
				return _frameSafeDebug;
			}
		}
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

		//Fade block:
		[System.NonSerialized]
		public AnimBool				fadeStatus;
		public bool					faded;
		
		public PWGUISettings()
		{
			update = false;
		}
	}
}
