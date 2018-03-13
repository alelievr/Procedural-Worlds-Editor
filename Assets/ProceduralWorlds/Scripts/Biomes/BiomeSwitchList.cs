using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using System;
using System.Linq;

namespace ProceduralWorlds.Biomator
{
	[System.Serializable]
	public class BiomeSwitchData
	{
		public float				min;
		public float				max;
		public string				name;
		public float				absoluteMax; //max value in the map
		public float				absoluteMin; //min value in the map
		public string				samplerName;
		public SerializableColor	color;

		public BiomeSwitchData(Sampler samp, string samplerName)
		{
			UpdateSampler(samp);
			name = "swampland";
			min = 70;
			max = 90;
			this.samplerName = samplerName;
			var rand = new System.Random();
			color = (SerializableColor)new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
		}

		public void UpdateSampler(Sampler samp)
		{
			if (samp != null)
			{
				absoluteMax = samp.max;
				absoluteMin = samp.min;
			}
		}

		public BiomeSwitchData(string samplerName) : this(null, samplerName) {}
	}

	[System.Serializable]
	public class BiomeSwitchList : IEnumerable< BiomeSwitchData >
	{
		public List< BiomeSwitchData >		switchDatas = new List< BiomeSwitchData >();

		public Action< BiomeSwitchData >	OnBiomeDataAdded;
		public Action						OnBiomeDataRemoved;
		public Action						OnBiomeDataReordered;
		public Action< BiomeSwitchData >	OnBiomeDataModified;
		
		public string		samplerName;
		public Sampler		sampler;
		
		public float		relativeMin = float.MinValue;
		public float		relativeMax = float.MaxValue;

		public int			Count { get { return switchDatas.Count; } }

		public void UpdateMinMax(float min, float max)
		{
			this.relativeMin = min;
			this.relativeMax = max;
		}

		public void UpdateSampler(string samplerName, Sampler sampler)
		{
			this.samplerName = samplerName;
			this.sampler = sampler;

			foreach (var switchData in switchDatas)
			{
				switchData.samplerName = samplerName;
				switchData.UpdateSampler(sampler);
			}

			//update min and max values in the list:
			switchDatas.First().min = relativeMin;
			switchDatas.Last().max = relativeMax;
		}

		IEnumerator< BiomeSwitchData > IEnumerable< BiomeSwitchData >.GetEnumerator()
		{
			foreach (var sw in switchDatas)
				yield return sw;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			yield return switchDatas;
		}

		public BiomeSwitchData this[int index]
		{
			get { return switchDatas[index]; }
		}
	}
}