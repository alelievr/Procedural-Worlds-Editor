using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	public class BiomePresetScreen : PresetScreen
	{
		readonly BiomeGraph	biomeGraph;

		readonly string	graphFilePrefix = "GraphPresets/Biome/Parts/";
	
		public BiomePresetScreen(BiomeGraph biomeGraph)
		{
			this.biomeGraph = biomeGraph;
	
			Texture2D plainTexture = Resources.Load< Texture2D >("");
			Texture2D mountainTexture = Resources.Load< Texture2D >("");
			Texture2D mesaTexture = Resources.Load< Texture2D >("");
			Texture2D swamplandTexture = Resources.Load< Texture2D >("");
			
			PresetCellList	earthLikePresets = new PresetCellList()
			{
				{"Biome preset"},
				{"Plains / Prairies", plainTexture, "Earth/Plain"},
				{"Mountains", mountainTexture, "Earth/Mountain", false},
				{"Mesas", mesaTexture, "Earth/Mesa", false},
				{"Swamplands", swamplandTexture, "Earth/Swampland", false},
			};

			PresetCellList presets = new PresetCellList
			{
				{"Biome category"},
				{"Earth like biomes", null, (string)null, true, earthLikePresets}
			};

			LoadPresetList(presets);
		}
	
		void ImportGraphTextAsset(string path)
		{
			var file = Resources.Load< TextAsset >(path);
	
			BaseGraphBuilder.FromGraph(biomeGraph)
				.ImportCommands(file.text.Split('\n'))
				.Execute();
		}

		public override void OnBuildPressed()
		{
			BaseGraphBuilder builder = BaseGraphBuilder.FromGraph(biomeGraph);

			foreach (var graphPartFile in graphPartFiles)
			{
				var file = Resources.Load< TextAsset >(graphFilePrefix + graphPartFile);
				builder.ImportCommands(file.text.Split('\n'));
			}

			builder.Execute();

			biomeGraph.presetChoosed = true;
		}
	}
}