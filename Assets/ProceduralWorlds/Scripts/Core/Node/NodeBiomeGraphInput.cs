using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Biomator;
using System;
using System.Linq;
using ProceduralWorlds.Nodes;

namespace ProceduralWorlds.Core
{
	public class NodeBiomeGraphInput : BaseNode
	{
		public enum BiomeDataInputMode
		{
			Standalone,
			WorldGraph,
		}

		[Output("Partial Biome data")]
		public PartialBiome			outputPartialBiome;
		
		[Output]
		public PWArray< object >	outputValues = new PWArray< object >();

		[SerializeField]
		WorldGraph					_previewGraph;
		public WorldGraph			previewGraph
		{
			get { return _previewGraph; }
			set
			{
				_previewGraph = value;
				if (value != null)
					inputDataMode = BiomeDataInputMode.WorldGraph;
				}
		}

		public BiomeDataInputGenerator inputDataGenerator = new BiomeDataInputGenerator();

		public BiomeDataInputMode	inputDataMode = BiomeDataInputMode.Standalone;

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
			
			if (inputDataMode == BiomeDataInputMode.WorldGraph || graphRef.IsRealMode())
			{
				if (outputPartialBiome != null)
					return ;
				
				if (previewGraph == null)
					throw new InvalidOperationException("[BiomeGraph] can't proces a graph in worldGraph data input mode with a null main graph");
				
				//check if the preview graph have a reference of this graph.
				if (!previewGraph.FindNodesByType< NodeBiome >().Any(b => b.biomeGraph == graphRef))
					throw new InvalidOperationException("[BiomeGraph] the specified preview graph (" + previewGraph + ") does not contains a reference of this biome graph");
				
				//we process the graph to provide the outputPartialBiome
				//it require that biomeGraph to be contained in the previewGraph.
				previewGraph.ProcessFrom(biomeGraphRef);
	
				//if the graph we process does not contains an instance of our biome graph
				if (outputPartialBiome == null)
					throw new InvalidOperationException("[BiomeGraph] " + graphRef + " there is a problem with the biome switch graph in (" + previewGraph + ") ");
			}
			else
			{
				outputPartialBiome = inputDataGenerator.GeneratePartialBiome2D(biomeGraphRef);
			}
		}

	}
}