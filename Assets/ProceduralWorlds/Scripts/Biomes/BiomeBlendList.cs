using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System;
using PW.Core;

namespace PW.Biomator
{
	[System.Serializable]
	public class BiomeBlendList
	{
		public bool[]	blendEnabled;

		public bool		listFoldout = false;

		public void UpdateIfNeeded(BiomeData biomeData)
		{
			if (blendEnabled != null && biomeData.length == blendEnabled.Length)
				return ;
			
			blendEnabled = new bool[biomeData.length];
			
			//set all blendEnabled value to true
			for (int i = 0; i < biomeData.length; i++)
				blendEnabled[i] = true;

			int waterIndex = biomeData.GetBiomeIndex(BiomeSamplerName.waterHeight);

			if (waterIndex != -1)
				blendEnabled[waterIndex] = false;
		}
	}
}