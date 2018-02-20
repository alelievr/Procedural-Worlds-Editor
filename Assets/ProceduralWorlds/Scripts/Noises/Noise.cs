using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using System.Runtime.CompilerServices;

namespace PW.Noises
{
	public abstract class Noise
	{
	
		public string	name;
		public bool		hasComputeShaders;
	
		public Noise()
		{
			hasComputeShaders = SystemInfo.graphicsShaderLevel >= 45;
		}

		public virtual float Get2D(float x, float y, int seed)
		{
			return 0;
		}

		public virtual float Get3D(float x, float y, float z, int seed)
		{
			return 0;
		}

		public abstract void ComputeSampler(Sampler samp, int seed);
	}
}