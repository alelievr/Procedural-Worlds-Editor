using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using PW.Biomator;

namespace PW.Core
{
	/*
	**	Graph computing storage classes (must not be serialized)
	*/

	public enum SamplerType
	{
		Sampler2D,
		Sampler3D,
		Vector2Sampler2D,
		Vector3Sampler2D,
		Vector2Sampler3D,
		Vector3Sampler3D,
	}

	/*
	**	Parent of all Sampler (array) storage classes
	*/
	public abstract class Sampler
	{
		public int			size;
		public float		step;
		public SamplerType	type;
		public float		min = 0;
		public float		max = 1;
	}
	
	/*
	**	Store a 2D array of float (usefull for 2D noise or heightmaps)
	*/
	public class Sampler2D : Sampler
	{
		[NonSerializedAttribute]
		public float[,]		map;
		
		public float this[int x, int y]
		{
			get {return map[x, y];}
			set {map[x, y] = value;}
		}
		
		public float At(int x, int y, bool normalized)
		{
			if (normalized)
				return (map[x, y] - min) / (max - min);
			else
				return map[x, y];
		}

		public Sampler2D(int size, float step)
		{
			this.map = new float[size, size];
			this.step = step;
			this.size = size;
			this.type = SamplerType.Sampler2D;
		}

		public void Resize(int size)
		{
			this.map = new float[size, size];
			this.size = size;
		}

		public void Foreach(Func< int, int, float > callback)
		{
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					map[x, y] = callback(x, y);
		}
		
		public void Foreach(Func< int, int, float, float > callback, bool normalized = false)
		{
			if (normalized)
				for (int x = 0; x < size; x++)
					for (int y = 0; y < size; y++)
						map[x, y] = callback(x, y, At(x, y, true));
			else
				for (int x = 0; x < size; x++)
					for (int y = 0; y < size; y++)
						map[x, y] = callback(x, y, map[x, y]);
		}
		
		public void Foreach(Action< int, int, float > callback, bool normalized = false)
		{
			if (normalized)
				for (int x = 0; x < size; x++)
					for (int y = 0; y < size; y++)
						callback(x, y, At(x, y, true));
			else
				for (int x = 0; x < size; x++)
					for (int y = 0; y < size; y++)
						callback(x, y, map[x, y]);
		}
		
		public override string ToString()
		{
			return "Samp2D(" + size + ")";
		}
	}

	/*
	**	Store 3D array of float (usefull for 3D noise)
	*/
	[Serializable]
	public class Sampler3D : Sampler
	{
		[NonSerializedAttribute]
		public float[,,]	map;

		public Sampler3D(int size, float step)
		{
			this.map = new float[size, size, size];
			this.size = size;
			this.step = step;
			this.type = SamplerType.Sampler3D;
		}

		public float this[int x, int y, int z]
		{
			get {return map[x, y, z];}
			set {map[x, y, z] = value;}
		}

		public float At(int x, int y, int z, bool normalized)
		{
			if (normalized)
				return (map[x, y, z] + min) / (max - min);
			else
				return map[x, y, z];
		}
		
		public void Foreach(Func< int, int, int, float > callback)
		{
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					for (int z = 0; z < size; z++)
						map[x, y, z] = callback(x, y, z);
		}
		
		public void Foreach(Func< int, int, int, float, float > callback, bool normalized = false)
		{
			if (normalized)
				for (int x = 0; x < size; x++)
					for (int y = 0; y < size; y++)
						for (int z = 0; z < size; z++)
							map[x, y, z] = callback(x, y, z, At(x, y, z, true));
			else
				for (int x = 0; x < size; x++)
					for (int y = 0; y < size; y++)
						for (int z = 0; z < size; z++)
							map[x, y, z] = callback(x, y, z, map[x, y, z]);
		}
		
		public void Foreach(Action< int, int, int, float > callback, bool normalized = false)
		{
			if (normalized)
				for (int x = 0; x < size; x++)
					for (int y = 0; y < size; y++)
						for (int z = 0; z < size; z++)
							callback(x, y, z, At(x, y, z, true));
			else
				for (int x = 0; x < size; x++)
					for (int y = 0; y < size; y++)
						for (int z = 0; z < size; z++)
							callback(x, y, z, map[x, y, z]);
		}

		public override string ToString()
		{
			return "Samp3D(" + size + ")";
		}
	}

	/*
	**	Vector samplers, same as Samplers but with vectors instead of floats
	*/

	[Serializable]
	public class Vector2Sampler2D : Sampler
	{
		public Vector2[,]	map;
	}
	
	[Serializable]
	public class Vector3Sampler2D : Sampler
	{
		public Vector3[,]	map;
	}
	
	[Serializable]
	public class Vector2Sampler3D : Sampler
	{
		public Vector2[,,]	map;
	}
	
	[Serializable]
	public class Vector3Sampler3D : Sampler
	{
		public Vector3[,,]	map;
	}

	/*
	**	Terrain storage classes:
	*/

	/*
	**	Parent class to store everything needed to render a chunk
	*/
	[Serializable]
	public abstract class ChunkData
	{
		public int			size;

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
		BiomeBlendPoint[]	blendMap;

		public BiomeMap2D(int size, float step)
		{
			this.size = size;
			this.step = step;
			blendMap = new BiomeBlendPoint[size * size];
		}
		
		public void SetFirstBiomeId(int x, int y, short id)
		{
			int		i = x + y * size;
			blendMap[i].firstBiomeId = id;
		}

		public BiomeBlendPoint	GetBiomeBlendInfo(int x, int y)
		{
			return blendMap[x + y * size];
		}
	}

	public class BiomeMap3D : Sampler
	{
		BiomeBlendPoint[]	blendMap;
		
		public BiomeMap3D(int size, float step)
		{
			this.size = size;
			this.step = step;
			blendMap = new BiomeBlendPoint[size * size * size];
		}

		public BiomeBlendPoint this[int x, int y, int z]
		{
			get { return blendMap[x + y * size + z * size * size]; }
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

		public Sampler2D			biomeTerrain;
		public Sampler3D			biomeTerrain3D;
		public Sampler				biomeTerrainRef { get { return (biomeTerrain == null) ? biomeTerrain3D : biomeTerrain as Sampler; } }
		
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

	public enum SurfaceMapType
	{
		Basic,
		Normal,
		Complex,
	}

	public class SurfaceMaps
	{
		SurfaceMapType		type;

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

		public Texture2D	blend;
	}

	public enum BiomeTerrainModifierType
	{
		Curve,
		Max,
	}

	/*
	**	Store the terrain modification performed by biome
	*/
	[Serializable]
	public class BiomeTerrainModifer
	{
		public BiomeTerrainModifierType		type;

		//Curve modifier:
		public SerializableAnimationCurve	curve = new SerializableAnimationCurve();

		//Max modifier:
		public Sampler						inputMaxTerrain;
		
		//TODO: other modifiers
	}

	[Serializable]
	public class BiomeTerrain
	{
		[SerializeField]
		public List< BiomeTerrainModifer >	terrainModifiers = new List< BiomeTerrainModifer >();
	}

	public class Biome
	{
		public BiomeData			biomeDataReference;
		public PWTerrainOutputMode	mode;
		public string				name;
		public Color				previewColor;
		public short				id;

		//datas for TopDown2D terrain
		public SurfaceMaps			surfaceMaps;
		public Color				surfaceColorModifier;

		//TODO: datas for others output modes
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

	[Serializable]
	public class PWGUISettings
	{
		public bool		active {get;  private set;}
		public Vector2	windowPosition;

		[System.NonSerializedAttribute]
		public object	oldState = null;

		//we put all possible datas for each inputs because unity serialization does not support inheritence :(
		
		//colorPicker:
		public SerializableColor	c;
		public Vector2				thumbPosition;

		//Sampler2D:
		public FilterMode			filterMode;
		public SerializableGradient	serializableGradient;
		[System.NonSerializedAttribute]
		public bool					update;
		public bool					debug;
		[System.NonSerializedAttribute]
		public Rect					savedRect;

		[System.NonSerializedAttribute]
		public Gradient				gradient;
		[System.NonSerializedAttribute]
		public Texture2D			texture;

		//Texture:
		// public FilterMode		filterMode; //duplicated
		public ScaleMode			scaleMode;
		public float				scaleAspect;
		//TODO: light-weight serializableMaterial
		[System.NonSerializedAttribute]
		public Material				material;

		//Editor utils:
		[System.NonSerializedAttribute]
		public int					popupHeight;
		

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
