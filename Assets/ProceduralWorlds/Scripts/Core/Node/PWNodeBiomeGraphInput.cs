using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Biomator;
using System;
using System.Linq;
using PW.Node;

namespace PW.Core
{
	public class PWNodeBiomeGraphInput : PWNode
	{
		public enum BiomeDataInputMode
		{
			Standalone,
			MainGraph,
		}

		[PWOutput("Partial Biome data")]
		public PartialBiome			outputPartialBiome;
		
		[PWOutput]
		public PWArray< object >	outputValues = new PWArray< object >();

		public PWMainGraph			previewGraph = null;

		public BiomeDataInputGenerator inputDataGenerator = new BiomeDataInputGenerator();

		public BiomeDataInputMode	inputDataMode;

		[System.NonSerialized]
		public int					calls;

		public override void OnNodeCreation()
		{
			name = "Biome input";
		}

		public override void OnNodeProcess()
		{
			calls++;

			if (calls > 10)
				return ;
		
			if (inputDataMode == BiomeDataInputMode.MainGraph || graphRef.IsRealMode())
			{
				if (outputPartialBiome != null)
					return ;
					
				//check if the preview graph have a reference of this graph.
				if (!previewGraph.FindNodesByType< PWNodeBiome >().Any(b => b.biomeGraph == graphRef))
					throw new Exception("[PWBiomeGraph] the specified preview graph (" + previewGraph + ") does not contains a reference of this biome graph");
				
				//we process the graph to provide the outputPartialBiome
				//it require that biomeGraph to be contained in the previewGraph.
				previewGraph.Process();
	
				//if the graph we process does not contains an instance of our biome graph
				if (outputPartialBiome == null)
					throw new Exception("[PWBiomeGraph] there is a problem with the biome switch graph in (" + previewGraph + ") ");
			}
			else
			{
				outputPartialBiome = inputDataGenerator.GeneratePartialBiome2D(biomeGraphRef);
			}
			
		}

	}
}