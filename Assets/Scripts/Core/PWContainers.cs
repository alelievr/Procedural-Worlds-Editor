using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace PW
{
	/*
	**	Graph calculus storage classes (must not be serialized)
	*/

	public class Sampler2D
	{
		[NonSerializedAttribute]
		public float[,]		map;
		[NonSerializedAttribute]
		public int			size;
		[NonSerializedAttribute]
		public float		step;
		
		public float this[int x, int y]
		{
			get {return map[x, y];}
			set {map[x, y] = value;}
		}

		public Sampler2D(int size, float step)
		{
			this.map = new float[size, size];
			this.step = step;
			this.size = size;
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
		
		public void Foreach(Func< int, int, float, float > callback)
		{
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					map[x, y] = callback(x, y, map[x, y]);
		}
		
		public void Foreach(Action< int, int, float > callback)
		{
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					callback(x, y, map[x, y]);
		}
		
		public override string ToString()
		{
			return "Samp2D(" + size + ")";
		}
	}

	[System.SerializableAttribute]
	public class Sampler3D
	{
		[NonSerializedAttribute]
		public float[,,]	map;
		[NonSerializedAttribute]
		public int			size;
		[NonSerializedAttribute]
		public float		step;

		public Sampler3D(int size, float step)
		{
			this.map = new float[size, size, size];
			this.size = size;
			this.step = step;
		}

		public float this[int x, int y, int z]
		{
			get {return map[x, y, z];}
			set {map[x, y, z] = value;}
		}
		
		public void Foreach(Func< int, int, int, float > callback)
		{
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					for (int z = 0; z < size; z++)
						map[x, y, z] = callback(x, y, z);
		}
		
		public void Foreach(Func< int, int, int, float, float > callback)
		{
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					for (int z = 0; z < size; z++)
						map[x, y, z] = callback(x, y, z, map[x, y, z]);
		}
		
		public void Foreach(Action< int, int, int, float > callback)
		{
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
	**	Terrain storage classes:
	*/

	[System.SerializableAttribute]
	public abstract class ChunkData
	{
		public int			size;

		public override string ToString()
		{
			return GetType().Name + "(" + size + ")";
		}
	}

	[System.SerializableAttribute]
	public class SideView2DData : ChunkData
	{
	}
	
	[System.SerializableAttribute]
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

	//TODO: this is not float its sampler2D x)

	[System.SerializableAttribute]
	public class BasicEdaphicData
	{
		public float		PH;
		public float		drainage;
		public float		nutrient;
		public float		mineral;
	}

	[System.SerializableAttribute]
	public class ComplexEdaphicData : BasicEdaphicData
	{
		public float		clay;
		public float		silt;
		public float		sand;
		public float		coarseSand;
	}

	[System.SerializableAttribute]
	public abstract class BiomeData {}

	[System.SerializableAttribute]
	public class BiomeLessData : BiomeData {}
	
	/* Height biomes */

	[System.SerializableAttribute]
	public class WaterlessBiomeHeightData : BiomeData
	{
		public float		height;
	}

	[System.SerializableAttribute]
	public class BiomeHeightData : WaterlessBiomeHeightData
	{
		public float		waterLevel;
	}

	/* Wetness biomes */

	[System.SerializableAttribute]
	public class WaterlessWetnessBiomeData : BiomeData
	{
		public float		wetness;
	}
	
	[System.SerializableAttribute]
	public class WaterlessWetnessBiomeData3D : WaterlessWetnessBiomeData
	{
		public float		height;
	}
	
	[System.SerializableAttribute]
	public class WetnessBiomeData : WaterlessWetnessBiomeData
	{
		public float		waterLevel;
	}
	
	[System.SerializableAttribute]
	public class WetnessBiomeData3D : WetnessBiomeData
	{
		public float		height;
	}

	/* Climate biomes */

	[System.SerializableAttribute]
	public class WaterlessClimateBiomeData : BiomeData
	{
		public float		wetness;
		public float		temperature;
	}

	[System.SerializableAttribute]
	public class WaterlessClimateBiomeData3D : WaterlessClimateBiomeData
	{
		public float		height;
	}

	[System.SerializableAttribute]
	public class ClimateBiomeData : WaterlessClimateBiomeData
	{
		public float		waterLevel;
	}

	[System.SerializableAttribute]
	public class ClimateBiomeData3D : ClimateBiomeData
	{
		public float		height;
	}

	/* Enhanced climate biomes */
	
	[System.SerializableAttribute]
	public class WaterlessEnhancedClimateBiomeData : BiomeData
	{
		public float			wetness;
		public float			temperature;
		public Vector2			wind;
		public float			lighting;
		public BasicEdaphicData	soil;
	}

	[System.SerializableAttribute]
	public class WaterlessEnhancedClimateBiomeData3D : WaterlessEnhancedClimateBiomeData
	{
		public float		height;
	}

	[System.SerializableAttribute]
	public class EnhancedClimateBiomeData : WaterlessEnhancedClimateBiomeData
	{
		public float		waterLevel;
	}
	
	[System.SerializableAttribute]
	public class EnhancedClimateBiomeData3D : EnhancedClimateBiomeData
	{
		public float		height;
	}

	/* Complex climate + edaphic biomes */

	[System.SerializableAttribute]
	public class WaterlessComplexClimateBiomeData : BiomeData
	{
		public float				wetness;
		public float				temperature;
		public Vector2				wind;
		public float				lighting;
		public float				air;
		public ComplexEdaphicData	soil;
	}

	[System.SerializableAttribute]
	public class WaterlessComplexClimateBiomeData3D : WaterlessComplexClimateBiomeData
	{
		public float			height;
	}

	[System.SerializableAttribute]
	public class ComplexClimateBiomeData : WaterlessComplexClimateBiomeData
	{
		public float			waterLevel;
	}

	[System.SerializableAttribute]
	public class ComplexClimateBiomeData3D : ComplexClimateBiomeData
	{
		public float			height;
	}

	/* Custom biome */

	[System.SerializableAttribute]
	public class CustomBiomeData : BiomeData
	{
		public float[]			datas;
	}

	/*
	**	Utils
	*/

	[System.SerializableAttribute]
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

	[System.SerializableAttribute]
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

	[System.SerializableAttribute]
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