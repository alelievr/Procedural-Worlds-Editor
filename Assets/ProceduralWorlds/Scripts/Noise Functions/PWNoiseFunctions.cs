using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;

namespace PW
{
	public class PWNoiseFunctions
	{

		public static Sampler2D Map(Sampler2D samp, float min, float max, bool alloc = false)
		{
			Sampler2D ret = samp;

			if (alloc)
				ret = new Sampler2D(ret.size, ret.step);
			ret.Foreach((x, y, val) => {
				return Mathf.Lerp(min, max, Mathf.InverseLerp(samp.min, samp.max, samp[x, y]));
			});
			ret.min = min;
			ret.max = max;
			return ret;
		}

	}
}
