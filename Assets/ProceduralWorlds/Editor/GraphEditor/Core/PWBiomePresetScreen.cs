using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;

namespace PW.Editor
{
	public class PWBiomePresetScreen : PWPresetScreen
	{
		Texture2D	plainTexture;
		Texture2D	mountainTexture;
		Texture2D	mesaTexture;
		Texture2D	swamplandTexture;
	
		PWBiomeGraph	biomeGraph;

		readonly string	graphFilePrefix = "GraphPresets/Biome/Parts/";
	
		public PWBiomePresetScreen(PWBiomeGraph biomeGraph)
		{
			this.biomeGraph = biomeGraph;
	
			plainTexture = Resources.Load< Texture2D >("");
			mountainTexture = Resources.Load< Texture2D >("");
			mesaTexture = Resources.Load< Texture2D >("");
			swamplandTexture = Resources.Load< Texture2D >("");
			
			PresetCellList	earthLikePresets = new PresetCellList()
			{
				{"Plains / Prairies", plainTexture, "Earth/Plain"},
				{"Mountains", mountainTexture, "Earth/Mountain", false},
				{"Mesas", mesaTexture, "Earth/Mesa", false},
				{"Swamplands", swamplandTexture, "Earth/Swampland", false},
			};

			PresetCellList presets = new PresetCellList
			{
				{"Earth like biomes", null, (string)null, true, earthLikePresets}
			};

			LoadPresetList(presets);
		}
	
		void ImportGraphTextAsset(string path)
		{
			var file = Resources.Load< TextAsset >(path);
	
			PWGraphBuilder.FromGraph(biomeGraph)
				.ImportCommands(file.text.Split('\n'))
				.Execute();
		}

		public override void OnBuildPressed()
		{
			foreach (var graphPartFile in graphPartFiles)
				ImportGraphTextAsset(graphFilePrefix + graphPartFile);

			PWGraphBuilder.FromGraph(biomeGraph).Execute();
		}
	}
}