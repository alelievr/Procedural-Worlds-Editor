using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using PW.Core;
using PW.Node;

namespace PW.Tests.Graphs
{
	public class PWGraphPresetTests
	{

		static string mainGraphPresetPath = "GraphPresets/Main/Full";
		// string biomeGraphPresetPath = "GraphPresets/Biome";
	
		[Test]
		public static void PWMainGraphPresets()
		{
			TextAsset[] mainGraphPresets = Resources.LoadAll< TextAsset >(mainGraphPresetPath);

			foreach (var mainGraphPreset in mainGraphPresets)
			{
				string[] commands = mainGraphPreset.text.Split('\n');

				var graph = PWGraphBuilder.NewGraph< PWMainGraph >()
					.ImportCommands(commands)
					.Execute()
					.GetGraph();

				graph.UpdateComputeOrder();
				
				//TODO: process the graph and check output

				// Assert.That(graph.GetOutput< FinalTerrain >() != null)
			}
		}
		
		/* 
		[Test]
		public void PWBiomeGraphPresets()
		{
			TextAsset[] biomeGraphPresets = Resources.LoadAll< TextAsset >(biomeGraphPresetPath);

			//TODO: generate a previewGraph for all the biomes here
			//generate the biome graph in the preview
			//assign each biomeGraph to the cerated biome node
			//assign the previewGraph to each biomeGraph
			//Process()

			foreach (var biomeGraphPreset in biomeGraphPresets)
			{
				string[] commands = biomeGraphPreset.text.Split('\n');

				var graph = PWGraphBuilder.NewGraph< PWBiomeGraph >()
					.ImportCommands(commands)
					.Custom((g) => {
						(g.inputNode as PWNodeBiomeGraphInput).previewGraph = previewgraph;
					})
					.Execute()
					.GetGraph();
				
				//TODO: process the graph and check output
			}
		}
		*/
	}
}