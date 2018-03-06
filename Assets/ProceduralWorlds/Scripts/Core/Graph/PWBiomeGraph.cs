using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Biomator;

namespace PW.Core
{
	public class PWBiomeGraph : PWGraph
	{

		public BiomeSurfaceType	surfaceType;

		public override void InitializeInputAndOutputNodes()
		{
			inputNode = CreateNewNode< PWNodeBiomeGraphInput >(new Vector2(-100, 0), "Input", false, false);
			outputNode = CreateNewNode< PWNodeBiomeGraphOutput >(new Vector2(100, 0), "Output", false, false);
		}

		public void SetInput(PartialBiome biomeData)
		{
			var input = inputNode as PWNodeBiomeGraphInput;

			input.outputPartialBiome = biomeData;
		}

		public Biome GetOutput()
		{
			var output = outputNode as PWNodeBiomeGraphOutput;

			return output.inputBiome;
		}

		public override void OnEnable()
		{
			graphType = PWGraphType.Biome;
			base.OnEnable();
		}

		public override void OnDisable()
		{
			base.OnDisable();
		}
	
	}
}