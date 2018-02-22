using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiome : PWNode
	{

		[PWInput]
		public BiomeData	inputBiomeData;

		[PWOutput]
		public PartialBiome	outputBiome;

		public PWBiomeGraph	biomeGraph;

		public override void OnNodeCreation()
		{
			name = "Biome";
		}

		public override void OnNodeEnable()
		{
			outputBiome = new PartialBiome();
		}

		public override void OnNodeProcessOnce()
		{
			if (biomeGraph == null)
			{
				Debug.LogError("[PWWBiomeNode] Null biome graph when processing once a biome node");
				return ;
			}

			outputBiome.biomeDataReference = inputBiomeData;
			outputBiome.biomeGraph = biomeGraph;
		}

		public override void OnNodeProcess()
		{
			if (biomeGraph == null)
			{
				Debug.LogError("[PWBiomeNode] Null biome graph when processing a biome node");
				return ;
			}

			//we set the biomeData to the biomeGraph so it can process
			biomeGraph.SetInput("inputBiomeData", inputBiomeData);
			
			outputBiome.biomeDataReference = inputBiomeData;
			outputBiome.biomeGraph = biomeGraph;
		}
		
	}
}