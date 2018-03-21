using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace ProceduralWorlds.Biomator.SwitchGraph
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

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			foreach (var sp in switchParams)
				if (sp.enabled)
					sb.Append(sp.min + " - " + sp.max + " | ");
			
			return sb.ToString();
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
			var sb = new System.Text.StringBuilder();

			for (int i = 0; i < length; i++)
				sb.Append(i + " -> " + switchValues[i] + ", ");
			
			return sb.ToString();
		}

	}
}