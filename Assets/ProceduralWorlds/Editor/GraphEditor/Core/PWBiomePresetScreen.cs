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
				{"Earth-like", plainTexture, "Plains / Prairies", BuildPlain},
				{"Earth-like", mountainTexture, "Mountains", BuildMountains},
				{"Earth-like", mesaTexture, "Mesas", BuildMesa},
				{"Earth-like", swamplandTexture, "Swamplands", BuildSwampland},
			};

			PresetCellBoard	presetBoard = new PresetCellBoard
			{

			};
	
			LoadPresetBoard(presetBoard);
		}
	
		void ImportGraphTextAsset(string path)
		{
			var file = Resources.Load< TextAsset >(path);
	
			PWGraphBuilder.FromGraph(biomeGraph)
				.ImportCommands(file.text.Split('\n'))
				.Execute();
		}
	
		void BuildPlain()
		{
			ImportGraphTextAsset("GraphPresets/Biome/PlainTest1");
		}
	
		//TODO: preset for mountain biome
		void BuildMountains()
		{
	
		}
	
		//TODO: preset for mesa biome
		void BuildMesa()
		{
	
		}
	
		//TODO: preset for swampland biome
		void BuildSwampland()
		{
	
		}
	}
}