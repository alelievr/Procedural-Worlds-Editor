using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Core
{
	public static class ColorTheme
	{

		public static readonly Color		selectedColor = new Color(0.000f, 0.000f, 0.804f);
		public static readonly Color		deletedColor = new Color(1, 0, 0);
		public static readonly Color		disabledAnchorColor = new Color(.2f, .2f, .2f);
		public static readonly Color		defaultBackgroundColor = new Color32(57, 57, 57, 255);

		public class ColorSchemeDict : Dictionary< ColorSchemeName, ColorScheme > {}

		//nodes, link and anchor colors:
		static ColorSchemeDict colorSchemes = new ColorSchemeDict
		{
			{ColorSchemeName.Default, new ColorScheme(220, 220, 220)},

			{ColorSchemeName.Turquoise, new ColorScheme(26, 188, 156)},
			{ColorSchemeName.Emerald, new ColorScheme(46, 204, 113)},
			{ColorSchemeName.PeterRiver, new ColorScheme(52, 152, 219)},
			{ColorSchemeName.Amethyst, new ColorScheme(155, 89, 182)},
			{ColorSchemeName.WetAsphalt, new ColorScheme(52, 73, 94)},
			{ColorSchemeName.SunFlower,new ColorScheme(241, 196, 15)},
			{ColorSchemeName.Carrot, new ColorScheme(230, 126, 34)},
			{ColorSchemeName.Alizarin, new ColorScheme(231, 76, 60)},
			{ColorSchemeName.Clouds, new ColorScheme(236, 240, 241)},
			{ColorSchemeName.Concrete, new ColorScheme(149, 165, 166)},

			{ColorSchemeName.Pumpkin, new ColorScheme(211, 84, 0)}
        };
		
        static Texture2D _defaultBackgroundTexture;
        public static Texture2D defaultBackgroundTexture
        {
            get
            {
                if (_defaultBackgroundTexture == null)
                    _defaultBackgroundTexture = Utils.CreateTexture2DColor(defaultBackgroundColor);

                return _defaultBackgroundTexture;
            }
        }

		public static Color GetLinkColor(ColorSchemeName csn)
		{
			return colorSchemes[csn].linkColor;
		}

		public static Color GetAnchorColor(ColorSchemeName csn)
		{
			return colorSchemes[csn].anchorColor;
		}
		
		public static Color GetNodeColor(ColorSchemeName csn)
		{
			return colorSchemes[csn].nodeColor;
		}
		
		public static Color GetSelectorHeaderColor(ColorSchemeName csn)
		{
			return colorSchemes[csn].selectorHeaderColor;
		}
		
		public static Color GetSelectorCellColor(ColorSchemeName csn)
		{
			return colorSchemes[csn].selectorCellColor;
		}

		static Dictionary< ColorSchemeName, List< Type > > anchorColorSchemeNames = new Dictionary< ColorSchemeName, List< Type > >
		{
			{
				ColorSchemeName.Alizarin, new List< Type > {
				typeof(float), typeof(Color), typeof(Vector2),
				typeof(Vector3), typeof(Vector4), typeof(int),
				typeof(GameObject), typeof(Mesh), typeof(Material),
				typeof(Texture2D) }
			}, {
				ColorSchemeName.Amethyst, new List< Type > {}
			}, {
				ColorSchemeName.Emerald, new List< Type >{ typeof(Sampler) }
			}, {
				ColorSchemeName.Carrot, new List< Type >{
					typeof(Biome), typeof(BiomeData), typeof(PartialBiome) }
			}, {
				ColorSchemeName.Pumpkin, new List< Type >{ typeof(BlendedBiomeTerrain) }
			}, {
				ColorSchemeName.SunFlower, new List< Type > { typeof(WorldChunk),
				typeof(BiomeSurfaceMaps), typeof(BiomeSurfaceColor),
				typeof(BiomeSurfaceMaterial), typeof(BiomeSurfaceSwitch),
				typeof(BiomeDetails), typeof(BiomeDetail) }
			}, {
				ColorSchemeName.Concrete, new List< Type > { typeof(TerrainDetail) }
			}, {
				ColorSchemeName.PeterRiver, new List< Type > {}
			}, {
				ColorSchemeName.Turquoise, new List< Type >{ typeof(BiomeSurfaceGraph) }
			},
		};

		public static ColorSchemeName GetAnchorColorSchemeName(Type fieldType)
		{
			if (fieldType.IsGenericType)
				fieldType = fieldType.GetGenericArguments()[0];
			
			foreach (var kp in anchorColorSchemeNames)
				foreach (var type in kp.Value)
					if (type == fieldType || fieldType.IsSubclassOf(type))
						return kp.Key;
			
			return ColorSchemeName.Default;
		}
	}
}