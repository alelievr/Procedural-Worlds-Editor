using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using System.Runtime.CompilerServices;

namespace ProceduralWorlds.Noises
{
	public abstract class Noise
	{
		public static readonly float	noiseScale = 0.01f;
	
		public string			name;
		public bool				hasComputeShaders { get; private set; }

		public float			scale;
		public float			persistence;
		public float			lacunarity;
		public int				seed;
		public Vector3			position;
	
		public Noise()
		{
			hasComputeShaders = SystemInfo.graphicsShaderLevel >= 45;
		}

		public abstract float	GetValue(Vector3 position);

		public virtual void	ComputeSampler2D(Sampler2D samp)
		{
			throw new System.NotImplementedException();
		}

		public virtual void	ComputeSampler3D(Sampler3D samp)
		{
			throw new System.NotImplementedException();
		}
	}
}