using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Biomator;

namespace PW.Biomator.SwitchGraph
{
	public class BiomeSwitchCell
	{
		public List< BiomeSwitchCell >	links = new List< BiomeSwitchCell >();
		public float					weight;
		public string					name;
		public Color					color;
		public short					id;

		public BiomeSwitchCellParams	switchParams = new BiomeSwitchCellParams();

		public bool		Overlaps(BiomeSwitchCellParams cellParams)
		{
			int length = cellParams.switchParams.Length;
			for (int i = 0; i < length; i++)
			{
				var c = cellParams.switchParams[i];
				var sp = switchParams.switchParams[i];
				if (c.enabled && sp.enabled
					&& !PWUtils.Overlap(sp.min, sp.max, c.min, c.max))
						return false;
			}
			return true;
		}

		public float	GetWeight(BiomeParamRange paramRanges)
		{
			float	weight = 0;

			int length = switchParams.switchParams.Length;
			for (int i = 0; i < length; i++)
			{
				BiomeSwitchCellParam	param = switchParams.switchParams[i];

				if (param.enabled && paramRanges.ranges[i].magnitude != 0)
					weight += param.max - param.min / paramRanges.ranges[i].magnitude;
				else
					weight += 1;
			}

			return weight;
		}

		public float ComputeBlend(BiomeParamRange param, BiomeSwitchValues values, float blendPercent)
		{
			float	blend = 0;
			float	blendParamCount = 1;

			int length = values.length;
			for (int i = 0; i < length; i++)
			{
				//Compute biome blend using blendPercent
				float v = values[i];
				Vector2 r = param.ranges[i];
				float mag = r.y - r.x;
				float p = mag * blendPercent;
				
				if (mag == 0 || v > r.x + p)
					continue ;

				float b = (.5f - (((v - r.x) / p) / 2));
				blend += b;
				
				// Debug.Log("blend range: " + r + ", mag: " + mag + ", val: " + v + ", blend percent range: " + p + ", blend: " + b);

				blendParamCount++;
			}

			return blend / blendParamCount;
		}

		public bool Matches(BiomeSwitchValues bsv)
		{
			var switchValues = bsv.switchValues;
			
			for (int i = 0; i < switchValues.Length; i++)
			{
				var  p = this.switchParams.switchParams[i];

				if (p.enabled && (switchValues[i] < p.min || switchValues[i] > p.max ))
						return false;
			}

			return true;
		}

		public float	GapWidth(BiomeSwitchCell c2)
		{
			float gap = 0;

			int length = c2.switchParams.switchParams.Length;
			for (int i = 0; i < length; i++)
				if (switchParams.switchParams[i].enabled)
				{
					var s1 = switchParams.switchParams[i];
					var s2 = c2.switchParams.switchParams[i];
					gap += PWUtils.GapWidth(s1.min, s1.max, s2.min, s2.max);
				}
			
			return gap;
		}

		public override string ToString()
		{
			string s = name + " = ";

			for (int i = 0; i < switchParams.switchParams.Length; i++)
				if (switchParams.switchParams[i].enabled)
					s += (i + ": " + switchParams.switchParams[i].min + "->" + switchParams.switchParams[i].max);

			return s;
		}
	}
}