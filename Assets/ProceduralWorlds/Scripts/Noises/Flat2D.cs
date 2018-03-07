using System.Collections;
using System.Collections.Generic;
using PW.Core;
using UnityEngine;

namespace PW.Noises
{
	public class Flat2D : Noise
	{
		protected float	flatValue;

		public Flat2D(float flatValue = 0)
		{
			UpdateParams(flatValue);
		}

		public void UpdateParams(float flatValue)
		{
			this.flatValue = flatValue;
		}

		public override void ComputeSampler2D(Sampler2D samp)
		{
			if (samp == null)
				Debug.LogError("Null sampler sent to Flat noise");
			
			samp.Foreach((x, y) => {
				return flatValue;
			});
		}

		public override float GetValue(Vector3 position)
		{
			return flatValue;
		}
	}
}
