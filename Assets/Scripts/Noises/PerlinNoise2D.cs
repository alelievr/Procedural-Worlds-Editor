using UnityEngine;
using PW;

public class PerlinNoise2D : Noise {

	public void	OnStart()
	{
	}

	public Sampler2D ComputeSampler(Bounds bounds, Sampler2D samp = null)
	{
		if (samp == null)
			samp = new Sampler2D((int)bounds.size.x);
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
