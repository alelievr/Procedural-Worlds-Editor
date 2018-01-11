using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;

namespace PW.Biomator
{
	[System.SerializableAttribute]
	public class BiomeSwitchData
	{
		public float				min;
		public float				max;
		public string				name;
		public float				absoluteMax; //max value in the map
		public float				absoluteMin; //min value in the map
		public SerializableColor	color;

		public BiomeSwitchData(Sampler samp)
		{
			UpdateSampler(samp);
			name = "swampland";
			min = 70;
			max = 90;
			color = (SerializableColor)new Color(0.196f, 0.804f, 0.196f, 1);
		}

		public void UpdateSampler(Sampler samp)
		{
			if (samp != null)
			{
				absoluteMax = samp.max;
				absoluteMin = samp.min;
			}
		}

		public BiomeSwitchData() : this(null) {}
	}

	public class PWBiomeSwitchList
	{
	}
}