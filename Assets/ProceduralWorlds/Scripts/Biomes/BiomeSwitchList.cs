using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using System;
using System.Linq;

namespace ProceduralWorlds.Biomator
{
	[System.Serializable]
	public class BiomeSwitchData : IEquatable< BiomeSwitchData >
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

		public override bool Equals(object obj)
		{
			if (obj is BiomeSwitchData)
				return Equals(obj as BiomeSwitchData);
			return false;
		}

		public static bool operator ==(BiomeSwitchData bsd1, BiomeSwitchData bsd2)
		{
			return bsd1.Equals(bsd2);
		}
		
		public static bool operator !=(BiomeSwitchData bsd1, BiomeSwitchData bsd2)
		{
			return !bsd1.Equals(bsd2);
		}

		public bool Equals(BiomeSwitchData other)
		{
			return min == other.min
				&& max == other.max
				&& absoluteMin == other.absoluteMin
				&& absoluteMax == other.absoluteMax
				&& samplerName == other.samplerName;
		}

		public override int GetHashCode()
		{
			int hash = 59;

			hash = hash * 31 + min.GetHashCode();
			hash = hash * 31 + max.GetHashCode();
			hash = hash * 31 + absoluteMin.GetHashCode();
			hash = hash * 31 + absoluteMax.GetHashCode();
			hash = hash * 31 + samplerName.GetHashCode();

			return hash;
		}

		public BiomeSwitchData(string samplerName) : this(null, samplerName) {}
	}

	[System.Serializable]
	public class BiomeSwitchList : IEnumerable< BiomeSwitchData >, IEquatable< BiomeSwitchList >
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

		public bool Equals(BiomeSwitchList other)
		{
			if (switchDatas.Count != other.switchDatas.Count)
				return false;
			
			for (int i = 0; i < switchDatas.Count; i++)
				if (switchDatas[i] != other.switchDatas[i])
					return false;
				
			return true;
		}

		public static bool operator ==(BiomeSwitchList bsl1, BiomeSwitchList bsl2)
		{
			return bsl1.Equals(bsl2);
		}

		public static bool operator !=(BiomeSwitchList bsl1, BiomeSwitchList bsl2)
		{
			return !bsl1.Equals(bsl2);
		}

		public override int GetHashCode()
		{
			int hash = 149;

			foreach (var switchData in switchDatas)
				hash = hash * 31 + switchData.GetHashCode();
			
			return hash;
		}

		public override bool Equals(object obj)
		{
			if (obj is BiomeSwitchList)
				return Equals(obj as BiomeSwitchList);
			return false;
		}

		public BiomeSwitchData this[int index]
		{
			get { return switchDatas[index]; }
		}
	}
}