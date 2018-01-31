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
		string mainGraphPresetPath = "GraphPresets/Main";
		string biomeGraphPresetPath = "GraphPresets/Biome";
	
		[Test]
		public void PWMainGraphPresets()
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
				
				Assert.That(graph.nodes.All(n => n.canWork == true));

				//TODO: process the graph and check output

				// Assert.That(graph.GetOutput< FinalTerrain >() != null)
			}
		}
		
		[Test]
		public void PWBiomeGraphPresets()
		{
			TextAsset[] biomeGraphPresets = Resources.LoadAll< TextAsset >(biomeGraphPresetPath);

			foreach (var biomeGraphPreset in biomeGraphPresets)
			{
				string[] commands = biomeGraphPreset.text.Split('\n');

				var graph = PWGraphBuilder.NewGraph< PWBiomeGraph >()
					.ImportCommands(commands)
					.Execute()
					.GetGraph();
				
				Assert.That(graph.nodes.All(n => n.canWork == true));
				
				//TODO: process the graph and check output
			}
		}
	
	
	}
}