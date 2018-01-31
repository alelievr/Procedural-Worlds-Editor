using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;

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
		
		PresetCellList	presets = new PresetCellList()
		{
			{"Earth-like", plainTexture, "Plains / Prairies", BuildPlain},
			{"Earth-like", mountainTexture, "Mountains", BuildMountains},
			{"Earth-like", mesaTexture, "Mesas", BuildMesa},
			{"Earth-like", swamplandTexture, "Swamplands", BuildSwampland},
		};

		LoadPresetList(presets);
	}

	void BuildPlain()
	{

	}

	void BuildMountains()
	{

	}

	void BuildMesa()
	{

	}

	void BuildSwampland()
	{

	}
}
