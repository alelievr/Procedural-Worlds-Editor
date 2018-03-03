using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Core
{
	public static class PWColor
	{
		
		public static readonly Color	orange = new Color(1.000f, 0.647f, 0.000f);
		
		public static Color	HexToColor(int color, bool missingAlpha = true)
		{
			byte alpha = (missingAlpha) ? (byte)0xFF : (byte)((color >> 24) & 0xFF);
			
			return new Color32(
				(byte)((color >> 16) & 0xFF),
				(byte)((color >>  8) & 0xFF),
				(byte)((color >>  0) & 0xFF),
				alpha);
		}
	
		public static int ColorToHex(Color c, bool alpha)
		{
			byte	a = (alpha) ? (byte)(c.a * 255) : (byte)0;
			byte	r = (byte)(c.r * 255);
			byte	g = (byte)(c.g * 255);
			byte	b = (byte)(c.b * 255);
	
			return ((a << 24) | (r << 16) | (g << 8) | (b));
		}
	
		public static void ColorToByte(Color c, out byte r, out byte g, out byte b, out byte a)
		{
			a = (byte)(c.a * 255);
			r = (byte)(c.r * 255);
			g = (byte)(c.g * 255);
			b = (byte)(c.b * 255);
		}
	
		public static Color ByteToColor(byte r, byte g, byte b, byte a)
		{
			return new Color32(r, g, b, a);
		}
	}
}
