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
			inputNode = CreateNewNode< PWNodeBiomeGraphInput >(new Vector2(-100, 0), "Input", true, false);
			outputNode = CreateNewNode< PWNodeBiomeGraphOutput >(new Vector2(100, 0), "Output", true, false);
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
		
		public float ProcessFrom(PWMainGraph graph)
		{
			if (!readyToProcess)
				return -1;
			
			var iNode = (inputNode as PWNodeBiomeGraphInput);
			var savedRealMode = IsRealMode();
			var savedBiomeDataMode = iNode.inputDataMode;
			
			SetRealMode(graph.IsRealMode());
			iNode.inputDataMode = PWNodeBiomeGraphInput.BiomeDataInputMode.MainGraph;

			float ret = Process();

			iNode.inputDataMode = savedBiomeDataMode;
			SetRealMode(savedRealMode);

			return ret;
		}
	}
}