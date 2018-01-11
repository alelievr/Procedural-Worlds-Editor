using UnityEngine;
using System;

namespace PW.Core
{
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
	public abstract class Sampler : IPWCloneable< Sampler >
	{
		public int			size;
		public float		step;
		public SamplerType	type;
		public float		min = 0;
		public float		max = 1;

		public abstract Sampler Clone(Sampler reuseObject);
		public abstract void Resize(int size, float step = -1);

		public bool NeedResize(int size, float step)
		{
			return (this.size != size || this.step != step);
		}
	}

	/*
	**	Store a 2D array of float (usefull for 2D noise or heightmaps)
	*/
	public class Sampler2D : Sampler
	{
		public float[,]		map;
		
		public float this[int x, int y]
		{
			get {return map[x, y];}
			set {map[x, y] = value;}
		}
		
		public float At(int x, int y, bool normalized)
		{
			if (normalized)
				return Mathf.InverseLerp(min, max, map[x, y]);
			else
				return map[x, y];
		}

		public Sampler2D(int size, float step)
		{
			this.step = step;
			this.type = SamplerType.Sampler2D;
			Resize(size);
		}

		public override void Resize(int size, float step = -1)
		{
			this.size = size;
			this.map = new float[size, size];
			this.step = (step == -1) ? this.step : step;
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
		
		public override Sampler Clone(Sampler reuseObject)
		{
			Sampler2D	newSampler;

			if (reuseObject != null)
			{
				newSampler = reuseObject as Sampler2D;
				if (newSampler.size != size)
					newSampler.Resize(size);
			}
			else
				newSampler = new Sampler2D(size, step);
			
			System.Buffer.BlockCopy(map, 0, newSampler.map, 0, map.Length * sizeof(float));

			return newSampler;
		}
		
		public override string ToString()
		{
			return "Samp2D(" + size + ")";
		}
	}

	/*
	**	Store 3D array of float (usefull for 3D noise)
	*/
	public class Sampler3D : Sampler
	{
		public float[,,]	map;

		public Sampler3D(int size, float step)
		{
			this.step = step;
			this.type = SamplerType.Sampler3D;
			Resize(size);
		}

		public override void Resize(int size, float step = -1)
		{
			this.size = size;
			this.map = new float[size, size, size];
			this.step = (step == -1) ? this.step : step;
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

		public override Sampler Clone(Sampler reuseObject)
		{
			Sampler3D	newSampler;

			if (reuseObject != null)
			{
				newSampler = reuseObject as Sampler3D;
				if (newSampler.size != size)
					newSampler.Resize(size);
			}
			else
				newSampler = new Sampler3D(size, step);
			
			System.Buffer.BlockCopy(map, 0, newSampler.map, 0, map.Length);

			return newSampler;
		}

		public override string ToString()
		{
			return "Samp3D(" + size + ")";
		}
	}

}