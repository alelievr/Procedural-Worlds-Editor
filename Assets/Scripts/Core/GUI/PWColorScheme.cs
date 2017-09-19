using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PW.Core;

namespace PW.Core
{
	public class PWColorPaletteInfo
	{
		public Color	nodeColor;
		public Color	textColor;
		public Color	selectorColor;
	}
	
	public static class PWColorScheme {

		public static Color disabledAnchorColor = new Color(.7f, .7f, .7f);

		static Dictionary< string, Color > colors = new Dictionary< string, Color >{
			{"blueNode", PWColor.HexToColor(0x4864ef)},
			{"greenNode", PWColor.HexToColor(0x30b030)},
			{"yellowNode", PWColor.HexToColor(0xe8ed55)},
			{"orangeNode", PWColor.HexToColor(0xff9e4f)},
			{"redNode", PWColor.HexToColor(0xde4747)},
			{"cyanNode", PWColor.HexToColor(0x55efea)},
			{"purpleNode", PWColor.HexToColor(0x814bed)},
			{"pinkNode", PWColor.HexToColor(0xf659d7)},
			{"greyNode", PWColor.HexToColor(0x646464)},
			{"whiteNode", PWColor.HexToColor(0xFFFFFF)},
			{"selectedNode", PWColor.HexToColor(0x101868)},
			{"borderNode", PWColor.HexToColor(0x777777)},
			{"blueAnchor", PWColor.HexToColor(0x4864ef)},
			{"greenAnchor", PWColor.HexToColor(0x30b030)},
			{"yellowAnchor", PWColor.HexToColor(0xe8ed55)},
			{"orangeAnchor", PWColor.HexToColor(0xff9e4f)},
			{"redAnchor", PWColor.HexToColor(0xde4747)},
			{"cyanAnchor", PWColor.HexToColor(0x55efea)},
			{"purpleAnchor", PWColor.HexToColor(0x814bed)},
			{"pinkAnchor", PWColor.HexToColor(0xf659d7)},
			{"greyAnchor", PWColor.HexToColor(0x646464)},
			{"whiteAnchor", PWColor.HexToColor(0xFFFFFF)},
			{"selected", PWColor.HexToColor(0x101868)},
			{"defaultAnchor", new Color(.75f, .75f, .75f, 1)},
			{"transparentBackground", new Color(.75f, .75f, .75f, .5f)}
		};
	
		static bool			IsType(Type t, params Type[] types)
		{
			foreach (var type in types)
				if (t == type || t.IsSubclassOf(type))
					return true;
			return false;
		}
		
		public static Color GetAnchorColorByType(Type t)
		{
			if (IsType(t, typeof(int), typeof(float), typeof(Vector2), typeof(Vector3),
				typeof(Vector4), typeof(Texture2D), typeof(Mesh), typeof(GameObject),
				typeof(Material), typeof(Color), typeof(BiomeSurfaceMaps)))
				return GetColor("redAnchor");
			else if (IsType(t, typeof(ChunkData)))
				return GetColor("blueAnchor");
			else if (IsType(t, typeof(BiomeData), typeof(BiomeTerrain), typeof(BiomeSurfaces)))
				return GetColor("purpleAnchor");
			else if (IsType(t, typeof(Biome)))
				return GetColor("cyanAnchor");
			else if (IsType(t, typeof(Sampler)))
				return GetColor("greenAnchor");
			else if (IsType(t, typeof(PWValues), typeof(PWValue)))
				return GetColor("whiteAnchor");
			else
				return GetColor("defaultAnchor");
		}
	
	
		public static Color	GetColor(string key)
		{
			return colors[key];
		}		
	}
}