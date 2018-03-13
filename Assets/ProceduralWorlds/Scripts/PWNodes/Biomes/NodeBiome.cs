using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Node
{
	public class NodeBiome : BaseNode
	{

		[Input]
		public BiomeData	inputBiomeData;

		[Output]
		public PartialBiome	outputBiome;

		public BiomeGraph	biomeGraph;

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