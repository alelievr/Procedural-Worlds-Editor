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
	
		public PWBiomePresetScreen(PWBiomeGraph biomeGraph)
		{
			this.biomeGraph = biomeGraph;
	
			plainTexture = Resources.Load< Texture2D >("");
			mountainTexture = Resources.Load< Texture2D >("");
			mesaTexture = Resources.Load< Texture2D >("");
			swamplandTexture = Resources.Load< Texture2D >("");
			
			PresetCellList	earthLikePresets = new PresetCellList()
			{
				{"Plains", plainTexture, "Plains / Prairies", SelectPlains},
				{"Mountains", mountainTexture, "Mountains", SelectMountains},
				{"Mesas", mesaTexture, "Mesas", SelectMesas},
				{"Swamplands", swamplandTexture, "Swamplands", SelectSwamplands},
			};

			PresetCellList presets = new PresetCellList
			{
				{"Earth-like", null, "Earth like biomes", SelectEarthLikeBiomes, true, earthLikePresets}
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

		#region Earth like biomes
	
		void SelectEarthLikeBiomes() {}

		void SelectMesas() {}

		void SelectMountains() {}

		void SelectPlains() {}

		void SelectSwamplands() {}

		#endregion

		public override void OnBuildPressed()
		{
			ImportGraphTextAsset("GraphPresets/Biome/PlainTest1");
		}
	}
}