using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using PW.Biomator;

namespace PW.Core
{
	public static class PWColorTheme
	{

		public static Color		selectedColor = new Color(0.000f, 0.000f, 0.804f);
		public static Color		deletedColor = new Color(1, 0, 0);
		public static Color		disabledAnchorColor = new Color(.2f, .2f, .2f);
		public static Color		defaultBackgroundColor = new Color32(57, 57, 57, 255);

		public class PWColorSchemeDict : Dictionary< PWColorSchemeName, PWColorScheme > {}

		//nodes, link and anchor colors:
		static PWColorSchemeDict colorSchemes = new PWColorSchemeDict
		{
			{PWColorSchemeName.Default, new PWColorScheme(220, 220, 220)},

			{PWColorSchemeName.Turquoise, new PWColorScheme(26, 188, 156)},
			{PWColorSchemeName.Emerald, new PWColorScheme(46, 204, 113)},
			{PWColorSchemeName.PeterRiver, new PWColorScheme(52, 152, 219)},
			{PWColorSchemeName.Amethyst, new PWColorScheme(155, 89, 182)},
			{PWColorSchemeName.WetAsphalt, new PWColorScheme(52, 73, 94)},
			{PWColorSchemeName.SunFlower,new PWColorScheme(241, 196, 15)},
			{PWColorSchemeName.Carrot, new PWColorScheme(230, 126, 34)},
			{PWColorSchemeName.Alizarin, new PWColorScheme(231, 76, 60)},
			{PWColorSchemeName.Clouds, new PWColorScheme(236, 240, 241)},
			{PWColorSchemeName.Concrete, new PWColorScheme(149, 165, 166)},

			{PWColorSchemeName.Pumpkin, new PWColorScheme(211, 84, 0)}
        };
		
        static Texture2D _defaultBackgroundTexture;
        public static Texture2D defaultBackgroundTexture
        {
            get
            {
                if (_defaultBackgroundTexture == null)
                    _defaultBackgroundTexture = PWUtils.CreateTexture2DColor(defaultBackgroundColor);

                return _defaultBackgroundTexture;
            }
        }

		static PWColorTheme()
		{
			//bake datas
		}

		public static Color GetLinkColor(PWColorSchemeName csn)
		{
			return colorSchemes[csn].linkColor;
		}

		public static Color GetAnchorColor(PWColorSchemeName csn)
		{
			return colorSchemes[csn].anchorColor;
		}
		
		public static Color GetNodeColor(PWColorSchemeName csn)
		{
			return colorSchemes[csn].nodeColor;
		}
		
		public static Color GetSelectorHeaderColor(PWColorSchemeName csn)
		{
			return colorSchemes[csn].selectorHeaderColor;
		}
		
		public static Color GetSelectorCellColor(PWColorSchemeName csn)
		{
			return colorSchemes[csn].selectorCellColor;
		}

		static Dictionary< PWColorSchemeName, List< Type > > anchorColorSchemeNames = new Dictionary< PWColorSchemeName, List< Type > >()
		{
			{
				PWColorSchemeName.Alizarin, new List< Type >() {
				typeof(float), typeof(Color), typeof(Vector2),
				typeof(Vector3), typeof(Vector4), typeof(int),
				typeof(GameObject), typeof(Mesh), typeof(Material),
				typeof(Texture2D) }
			}, {
				PWColorSchemeName.Amethyst, new List< Type >() {}
			}, {
				PWColorSchemeName.Emerald, new List< Type >() { typeof(Sampler) }
			}, {
				PWColorSchemeName.Carrot, new List< Type >() {
					typeof(Biome), typeof(BiomeData), typeof(PartialBiome) }
			}, {
				PWColorSchemeName.Pumpkin, new List< Type >() { typeof(BlendedBiomeTerrain) }
			}, {
				PWColorSchemeName.SunFlower, new List< Type >() { typeof(FinalTerrain),
				typeof(BiomeSurfaceMaps), typeof(BiomeSurfaceColor),
				typeof(BiomeSurfaceMaterial), typeof(BiomeSurfaceSwitch) }
			}, {
				PWColorSchemeName.Concrete, new List< Type >() { typeof(TerrainDetail) }
			}, {
				PWColorSchemeName.PeterRiver, new List< Type >() {}
			}, {
				PWColorSchemeName.Turquoise, new List< Type >() { typeof(BiomeTerrain), typeof(BiomeSurfaceGraph) }
			},
		};

		public static PWColorSchemeName GetAnchorColorSchemeName(Type fieldType)
		{
			if (fieldType.IsGenericType)
				fieldType = fieldType.GetGenericArguments()[0];
			
			foreach (var kp in anchorColorSchemeNames)
				foreach (var type in kp.Value)
					if (type == fieldType || fieldType.IsSubclassOf(type))
						return kp.Key;
			
			return PWColorSchemeName.Default;
		}
	}
}