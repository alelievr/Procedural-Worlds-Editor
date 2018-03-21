using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	public class BiomePresetScreen : PresetScreen
	{
		readonly BiomeGraphEditor	biomeGraphEditor;
		BiomeGraph					biomeGraph { get { return biomeGraphEditor.biomeGraph; } }

		readonly string	graphFilePrefix = "GraphPresets/Biome/Parts/";
	
		public BiomePresetScreen(BiomeGraphEditor biomeGraphEditor, bool loadStyle = true)
		{
			this.biomeGraphEditor = biomeGraphEditor;
	
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

			if (loadStyle)
				LoadStyle();
		}

		public override void OnBuildPressed()
		{
			GraphBuilder builder = GraphBuilder.FromGraph(biomeGraph);

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