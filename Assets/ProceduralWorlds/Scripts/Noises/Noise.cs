using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;

namespace PW.Noises
{
	public abstract class Noise {
	
		public string	name;
		public bool		hasGraphicAcceleration;
	
		public Noise()
		{
			hasGraphicAcceleration = SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null;
		}

		public virtual float Get2D(float x, float y, int seed)
		{
			return 0;
		}

		public virtual float Get3D(float x, float y, float z, int seed)
		{
			return 0;
		}

		public virtual void ComputeSampler(Sampler samp, int seed)
		{

		}
	}
}