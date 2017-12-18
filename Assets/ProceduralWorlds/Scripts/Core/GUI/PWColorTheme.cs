using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

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
			{PWColorSchemeName.Default, new PWColorScheme(0, 0, 0)},

			{PWColorSchemeName.Turquoise, new PWColorScheme(26, 188, 156)},
			{PWColorSchemeName.Emerald, new PWColorScheme(46, 204, 113)},
			{PWColorSchemeName.PeterRiver, new PWColorScheme(52, 152, 219)},
			{PWColorSchemeName.Amethyst, new PWColorScheme(155, 89, 182)},
			{PWColorSchemeName.WetAsphalt, new PWColorScheme(52, 73, 94)},
			{PWColorSchemeName.SunFlower,new PWColorScheme(241, 196, 15) },
			{PWColorSchemeName.Carrot, new PWColorScheme(230, 126, 34)},
			{PWColorSchemeName.Alizarin, new PWColorScheme(231, 76, 60)},
			{PWColorSchemeName.Clouds, new PWColorScheme(236, 240, 241)},
			{PWColorSchemeName.Concrete, new PWColorScheme(149, 165, 166)},
		};

		static Texture2D _defaultBackgroundTexture;
		public static Texture2D	defaultBackgroundTexture
		{
			get {
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
	}
}