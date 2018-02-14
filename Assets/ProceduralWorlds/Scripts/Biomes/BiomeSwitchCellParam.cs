using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Biomator.SwitchGraph
{
	public class BiomeSwitchCellParam
	{
		public bool			enabled;
		public float		min;
		public float		max;

		public BiomeSwitchCellParam(bool enabled = false, float min = 1e20f, float max = -1e20f)
		{
			this.enabled = enabled;
			this.min = min;
			this.max = max;
		}
	}

	public class BiomeSwitchCellParams
	{
		public BiomeSwitchCellParam[]	switchParams;

		public BiomeSwitchCellParams()
		{
			const int max = BiomeData.maxBiomeSamplers;
			switchParams = new BiomeSwitchCellParam[max];

			for (int i = 0; i < switchParams.Length; i++)
				switchParams[i] = new BiomeSwitchCellParam(false);
		}
	}

	public class BiomeSwitchValues
	{
		public float[]		switchValues;
		public bool[]		enabled;

		public int			length;

		public BiomeSwitchValues()
		{
			const int max = BiomeData.maxBiomeSamplers;

			switchValues = new float[max];
			enabled = new bool[max];
			length = 0;

			for (int i = 0; i < switchValues.Length; i++)
				switchValues[i] = new float();
		}

		public float this[int index]
		{
			get
			{
				return switchValues[index];
			}
			set
			{
				enabled[index] = true;
				switchValues[index] = value;
				length = (index >= length) ? index + 1 : length;
			}
		}

		public override string ToString()
		{
			string s = "";

			for (int i = 0; i < length; i++)
				s += i + " -> " + switchValues[i] + ", ";
			
			return s;
		}

	}
}