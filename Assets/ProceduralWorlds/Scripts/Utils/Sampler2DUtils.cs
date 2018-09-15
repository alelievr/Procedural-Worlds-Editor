using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ProceduralWorlds.Core
{
	public static class Sampler2DUtils
	{
		public static Texture2D ToTexture2D(Sampler2D sampler, Gradient grad, Texture2D reuse = null)
		{
			return ToTexture2D(sampler, (val) => grad.Evaluate(Mathf.InverseLerp(sampler.min, sampler.max, val)), reuse);
		}

		public static Texture2D ToTexture2D(Sampler2D sampler, Color min, Color max, Texture2D reuse = null)
		{
			return ToTexture2D(sampler, (val) => Color.Lerp(min, max, Mathf.InverseLerp(sampler.min, sampler.max, val)), reuse);
		}

		static Texture2D ToTexture2D(Sampler2D sampler, Func< float, Color > colorMapFunction, Texture2D reuse = null)
		{
			if (reuse == null)
			{
				reuse = new Texture2D(sampler.size, sampler.size);
				reuse.filterMode = FilterMode.Point;
			}

			sampler.Foreach((x, y, val) => {
				reuse.SetPixel(x, y, colorMapFunction(val));
			});

			reuse.Apply();

			return reuse;
		}
	}
}