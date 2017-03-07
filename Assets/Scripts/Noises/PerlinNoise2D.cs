using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW;

public class PerlinNoise2D : Noise {

	public void	OnStart()
	{
	}

	public void ComputeZone(Bounds bounds)
	{
		if (hasGraphicAcceleration)
		{
			//compute shader here
		}
		else
		{
			//use conventional threaded way
		}
	}

	public float GetValueAt(Vector3 pos)
	{
		return 0;
	}

}
