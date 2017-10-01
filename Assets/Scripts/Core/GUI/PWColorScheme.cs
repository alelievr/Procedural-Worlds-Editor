using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PW.Core;

namespace PW.Core
{
	[System.Serializable]
	public class PWColorScheme {

		public PWColorScheme(byte r, byte g, byte b)
		{
			nodeColor = new Color32(r, g, b, 255);
			linkColor = nodeColor;
			selectorHeaderColor = nodeColor * 1.1f;
			selectorCellColor = nodeColor * 0.9f;
			anchorColor = nodeColor;
		}
		
		//node header color
		public Color	nodeColor;
		//anchor color
		public Color	anchorColor;
		//link color
		public Color	linkColor;
		//node selector header color
		public Color	selectorHeaderColor;
		//node selector cell color
		public Color	selectorCellColor;
	}
}