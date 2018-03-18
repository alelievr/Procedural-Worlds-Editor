using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;
using ProceduralWorlds.Editor;
using System.IO;

namespace ProceduralWorlds.Tests.Graphs
{
	public class BaseGraphPresetTests
	{

		static string worldGraphPresetPath = "GraphPresets/World/Full";
		// string biomeGraphPresetPath = "GraphPresets/Biome";
	
		[Test]
		public static void WorldGraphPresets()
		{
			TextAsset[] worldGraphPresets = Resources.LoadAll< TextAsset >(worldGraphPresetPath);

			foreach (var worldGraphPreset in worldGraphPresets)
			{
				string[] commands = worldGraphPreset.text.Split('\n');

				var graph = GraphBuilder.NewGraph< WorldGraph >()
					.ImportCommands(commands)
					.Execute()
					.GetGraph();

				graph.UpdateComputeOrder();
			}
		}

		[Test]
		public static void WorldGraphCreationPreset()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >().GetGraph() as WorldGraph;

			var we = WorldGraphEditor.CreateInstance< WorldGraphEditor >();
			we.graph = graph;

			string tmpFolderPath = Application.dataPath + "/Tests_TMP/";
			string tmpBiomeFolderPath = tmpFolderPath + "/Biomes/";

			if (!Directory.Exists(tmpFolderPath))
				Directory.CreateDirectory(tmpFolderPath);
			if (!Directory.Exists(tmpBiomeFolderPath))
				Directory.CreateDirectory(tmpBiomeFolderPath);
			

			graph.assetFilePath = tmpFolderPath.Substring(Application.dataPath.Length - "Assets/".Length + 1) + "wg.asset";
			var wps = new WorldPresetScreen(we, false);

			wps.OnBuildPressed();

			graph.Process();

			Directory.Delete(tmpFolderPath, true);
		}
		
		/* 
		[Test]
		public void BiomeGraphPresets()
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

				var graph = GraphBuilder.NewGraph< BiomeGraph >()
					.ImportCommands(commands)
					.Custom((g) => {
						(g.inputNode as NodeBiomeGraphInput).previewGraph = previewgraph;
					})
					.Execute()
					.GetGraph();
				
				//TODO: process the graph and check output
			}
		}
		*/
	}
}