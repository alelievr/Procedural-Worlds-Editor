using System.Collections.Generic;
using UnityEngine;

public static class PWColorPalette  {

	static Dictionary< string, Color > colors = new Dictionary< string, Color >{
		{"blueNode", HexToColor(0x4864ef)},
		{"greenNode", HexToColor(0x30b030)},
		{"yellowNode", HexToColor(0xe8ed55)},
		{"orangeNode", HexToColor(0xff9e4f)},
		{"redNode", HexToColor(0xde4747)},
		{"cyanNode", HexToColor(0x55efea)},
		{"purpleNode", HexToColor(0x814bed)},
		{"pinkNode", HexToColor(0xf659d7)},
		{"greyNode", HexToColor(0x646464)},
		{"whiteNode", HexToColor(0xFFFFFF)},
		{"selectedNode", HexToColor(0x101868)},
		{"borderNode", HexToColor(0x777777)},
		{"blueAnchor", HexToColor(0x4864ef)},
		{"greenAnchor", HexToColor(0x30b030)},
		{"yellowAnchor", HexToColor(0xe8ed55)},
		{"orangeAnchor", HexToColor(0xff9e4f)},
		{"redAnchor", HexToColor(0xde4747)},
		{"cyanAnchor", HexToColor(0x55efea)},
		{"purpleAnchor", HexToColor(0x814bed)},
		{"pinkAnchor", HexToColor(0xf659d7)},
		{"greyAnchor", HexToColor(0x646464)},
		{"whiteAnchor", HexToColor(0xFFFFFF)},
		{"defaultAnchor", new Color(.75f, .75f, .75f, 1)},
	};

	public static Color	GetColor(string key)
	{
		return colors[key];
	}
	
	public static Color	HexToColor(int color)
	{
		byte alpha = (byte)((color >> 24) & 0xFF);
		if (alpha == 0x00)
			alpha = 0xFF;
		
		return new Color32(
			(byte)((color >> 16) & 0xFF),
			(byte)((color >>  8) & 0xFF),
			(byte)((color >>  0) & 0xFF),
			alpha);
	}
}
