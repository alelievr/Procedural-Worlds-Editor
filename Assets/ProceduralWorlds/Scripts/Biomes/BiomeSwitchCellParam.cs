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

		public int						Length { get { return switchParams.Length; } }

		public BiomeSwitchCellParams()
		{
			switchParams = new BiomeSwitchCellParam[(int)BiomeSwitchMode.Count];

			for (int i = 0; i < switchParams.Length; i++)
				switchParams[i] = new BiomeSwitchCellParam(false);
		}

		public void ImportValues(BiomeSwitchValues values, float blendPercent, BiomeParamRange ranges)
		{
			for (int i = 0; i < values.switchValues.Length; i++)
			{
				var mode = (BiomeSwitchMode)i;
				if (values.enabled[i] && i != (int)BiomeSwitchMode.Water && ranges.ContainsKey(mode))
				{
					float range = ranges[mode].magnitude;
					float min = values[i] - (range * blendPercent);
					float max = values[i] + (range * blendPercent);
					switchParams[i] = new BiomeSwitchCellParam(true, min, max);
				}
			}
		}

		public BiomeSwitchCellParam this[int index]
		{
			get { return switchParams[index]; }
			set { switchParams[index] = value; }
		}

		public BiomeSwitchCellParam this[BiomeSwitchMode mode]
		{
			get { return this[(int)mode]; }
			set { this[(int)mode] = value; }
		}
	}

	public class BiomeSwitchValues
	{
		public float[]		switchValues;
		public bool[]		enabled;

		public BiomeSwitchValues()
		{
			switchValues = new float[(int)BiomeSwitchMode.Count];
			enabled = new bool[(int)BiomeSwitchMode.Count];

			for (int i = 0; i < switchValues.Length; i++)
				switchValues[i] = new float();
		}

		public float this[int index]
		{
			get { return switchValues[index]; }
			set { enabled[index] = true; switchValues[index] = value; }
		}

		public float this[BiomeSwitchMode mode]
		{
			get { return this[(int)mode]; }
			set { this[(int)mode] = value; }
		}

		public override string ToString()
		{
			string	s = "";

			for (int i = 0; i < (int)BiomeSwitchMode.Count; i++)
				s += (BiomeSwitchMode)i + ": " + switchValues[i] + ", ";
			
			return s;
		}
	}
}