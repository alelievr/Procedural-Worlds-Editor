using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Core
{
	public class BiomeGraph : BaseGraph
	{

		[TextSerializeField]
		public BiomeSurfaceType	surfaceType;

		public override void InitializeInputAndOutputNodes()
		{
			inputNode = CreateNewNode< NodeBiomeGraphInput >(new Vector2(-100, 0), "Input", true, false);
			outputNode = CreateNewNode< NodeBiomeGraphOutput >(new Vector2(100, 0), "Output", true, false);
		}

		public void SetInput(PartialBiome biomeData)
		{
			var input = inputNode as NodeBiomeGraphInput;

			input.outputPartialBiome = biomeData;
		}

		public Biome GetOutput()
		{
			var output = outputNode as NodeBiomeGraphOutput;

			return output.inputBiome;
		}

		public override void OnEnable()
		{
			graphType = BaseGraphType.Biome;
			base.OnEnable();
		}

		public override void OnDisable()
		{
			base.OnDisable();
		}
		
		public float ProcessFrom(WorldGraph graph)
		{
			if (!isReadyToProcess)
				return -1;
			
			var iNode = (inputNode as NodeBiomeGraphInput);
			var savedRealMode = IsRealMode();
			var savedBiomeDataMode = iNode.inputDataMode;
			
			SetRealMode(graph.IsRealMode());
			iNode.inputDataMode = NodeBiomeGraphInput.BiomeDataInputMode.WorldGraph;

			float ret = Process();

			iNode.inputDataMode = savedBiomeDataMode;
			SetRealMode(savedRealMode);

			return ret;
		}
	}
}