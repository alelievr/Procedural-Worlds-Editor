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
			int length = cellParams.Length;
			for (int i = 0; i < length; i++)
			{
				var c = cellParams[i];
				var sp = switchParams[i];
				if (c.enabled && sp.enabled
					&& !PWUtils.Overlap(sp.min, sp.max, c.min, c.max))
						return false;
			}
			return true;
		}

		public float	GetWeight(BiomeParamRange paramRanges)
		{
			float	weight = 0;

			for (int i = 0; i < switchParams.Length; i++)
			{
				BiomeSwitchCellParam	param = switchParams[i];

				if (param.enabled && paramRanges[i].magnitude != 0)
					weight += param.max - param.min / paramRanges[i].magnitude;
				else
					weight += 1;
			}

			return weight;
		}

		public float ComputeBlend(BiomeParamRange ranges, BiomeSwitchValues values, float blendPercent)
		{
			float	blend = 0;
			float	blendParamCount = 1;

			foreach (var rangeKP in ranges)
			{
				int i = (int)rangeKP.Key;

				//Compute biome blend using blendPercent
				float v = values[i];
				Vector2 r = rangeKP.Value;
				float mag = r.y - r.x;
				float p = mag * blendPercent;
				
				if (mag == 0)
					continue ;

				blend += (1 - ((v - r.x) / p)) / 2;

				blendParamCount++;
			}

			return blend / blendParamCount;
		}

		public bool Matches(BiomeSwitchValues bsv)
		{
			for (int i = 0; i < bsv.switchValues.Length; i++)
			{
				var  p = this.switchParams[i];

				if (p.enabled && (bsv[i] < p.min || bsv[i] > p.max ))
						return false;
			}

			return true;
		}

		public float	GapWidth(BiomeSwitchCell c2)
		{
			float gap = 0;

			int length = c2.switchParams.Length;
			for (int i = 0; i < length; i++)
				if (switchParams[i].enabled)
				{
					var s1 = switchParams[i];
					var s2 = c2.switchParams[i];
					gap += PWUtils.GapWidth(s1.min, s1.max, s2.min, s2.max);
				}
			
			return gap;
		}

		public override string ToString()
		{
			string s = name + " = ";

			for (int i = 0; i < switchParams.Length; i++)
				if (switchParams[i].enabled)
					s += (i + ": " + switchParams[i].min + "->" + switchParams[i].max);

			return s;
		}
	}
}