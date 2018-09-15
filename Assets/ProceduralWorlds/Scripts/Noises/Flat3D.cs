using System.Collections;
using System.Collections.Generic;
using ProceduralWorlds.Core;
using UnityEngine;

namespace ProceduralWorlds.Noises
{
	public class Flat3D : Noise
	{
		float flatValue;

		public Flat3D(float flatValue = 0)
		{
			UpdateParams(flatValue);
		}

		public void UpdateParams(float flatValue)
		{
			this.flatValue = flatValue;
		}

		public override void ComputeSampler3D(Sampler3D samp, Vector3 position)
		{
			if (samp == null)
			{
				Debug.LogError("Null sampler sent to Flat noise");
				return ;
			}

			samp.Foreach((x, y, z) => {
				return flatValue;
			});
		}

		public override float GetValue(Vector3 position)
		{
			throw new System.NotImplementedException();
		}
	}
}