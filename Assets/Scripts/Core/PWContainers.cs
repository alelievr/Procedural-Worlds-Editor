using UnityEngine;
using System;

namespace PW
{
	[System.SerializableAttribute]
	public class Sampler2D
	{
		public float[,]		map;
		public int			size;
		public float		step;
		
		public float this[int x, int y]
		{
			get {return map[x, y];}
			set {map[x, y] = value;}
		}

		public Sampler2D(int size, float step = 1)
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
	}

	[System.SerializableAttribute]
	public class Sampler3D
	{
		public float[,,]	map;
		public int			size;
		public float		step;

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
	}

	public class SideView2DData
	{
		public Vector2		size;
	}
	
	public class TopDown2DData
	{
		public Vector2		size;
		public Texture2D	texture;
	}

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

	[System.SerializableAttribute]
	public class ChunkStorage< T, U > where T : class where U : class
	{
		[System.SerializableAttribute]
		public class ChunkDictionary : SerializableDictionary< Vector3i, Pair< T, U > > {}
		[SerializeField]
		ChunkDictionary chunks = new ChunkDictionary();

		public bool isLoaded(Vector3i pos)
		{
			return chunks.ContainsKey(pos);
		}
		
		public T	AddChunk(Vector3i pos, T chunk, U userChunkDatas)
		{
			chunks[pos] = new Pair< T, U >(chunk, userChunkDatas);
			return chunk;
		}

		public T	GetChunkDatas(Vector3i pos)
		{
			if (chunks.ContainsKey(pos))
				return chunks[pos].first;
			return null;
		}

		public U	GetChunkUserDatas(Vector3i pos)
		{
			if (chunks.ContainsKey(pos))
				return chunks[pos].second;
			return null;
		}

		public Pair< T, U > this[Vector3i pos]
		{
			get {
				return chunks[pos];
			}
		}
	}
}