using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using ProceduralWorlds.Core;
using ProceduralWorlds.Nodes;
using ProceduralWorlds.Editor;
using ProceduralWorlds.Biomator;
using System.IO;

namespace ProceduralWorlds.Tests.Graphs
{
	public class BaseGraphPresetTests
	{

		static string worldGraphPresetPath = "GraphPresets/World/Full";
		string biomeGraphPresetPath = "GraphPresets/Biome/Full";
	
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
		}
		
		//TODO: reactivate this test when the biome graph text files will be up to date
		public void BiomeGraphPresets()
		{
			TextAsset[] biomeGraphPresets = Resources.LoadAll< TextAsset >(biomeGraphPresetPath);

			foreach (var biomeGraphPreset in biomeGraphPresets)
			{
				string[] commands = biomeGraphPreset.text.Split('\n');

				var graph = GraphBuilder.NewGraph< BiomeGraph >()
					.ImportCommands(commands)
					.Custom(g => { (g as BiomeGraph).surfaceType = BiomeSurfaceType.Color; })
					.Execute()
					.GetGraph();
				
				graph.Process();
				
				//TODO: check output
			}
		}
	}
}