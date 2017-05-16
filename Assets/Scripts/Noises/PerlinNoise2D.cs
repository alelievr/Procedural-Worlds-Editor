using UnityEngine;
using PW;

namespace PW
{
	public class PerlinNoise2D : Noise {
	
		public void	OnStart()
		{
		}
	
		public Sampler2D ComputeSampler(Bounds bounds, Sampler2D samp)
		{
			if (samp == null)
				Debug.LogError("null sampler send to ComputeSampler !");
			if (hasGraphicAcceleration)
			{
				//compute shader here
			}
			else
			{
				//use conventional threaded way
			}
			return samp;
		}
	
		public float GetValueAt(Vector3 pos)
		{
			return 0;
		}
	
	}
}