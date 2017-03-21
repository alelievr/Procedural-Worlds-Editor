using System.Collections.Generic;
using UnityEngine;
using System;

namespace PW
{
	public class Sampler2D
	{
		public float[,]		map;
		public int			size;
		public float		step;

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
		
		public void Foreach(Action< int, int, float > callback)
		{
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					callback(x, y, map[x, y]);
		}
	}

	public class Sampler3D
	{
		public float[,,]	map;
		public int			size;
		public float		step;
		
		public void Foreach(Func< int, int, int, float > callback)
		{
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					for (int z = 0; z < size; z++)
						map[x, y, z] = callback(x, y, z);
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

	public class ChunkStorage<T>
	{
		Dictionary< Vector3i, T > chunks = new Dictionary< Vector3i, T >();

		public bool isLoaded(Vector3i pos)
		{
			return chunks.ContainsKey(pos);
		}
		
		public T	AddChunk(Vector3i pos, T chunk)
		{
			chunks[pos] = chunk;
			return chunk;
		}
	}
}