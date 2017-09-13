using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW;
using PW.Core;

[System.Serializable]
public class PWNodeLink
{
	//GUID to identify the node in the LinkTable
	public string			GUID;

	//ColorPalette of the link
	public PWColorPalette	colorPalette;
	//link type
	public PWLinkType		type;
	//link hightlight mode 
	public PWLinkHighlight	highlight;
	//is selected ?
	public bool				selected;

	//anchor where the link is connected:
	[System.NonSerialized]
	public PWAnchorField	fromAnchor;
	[System.NonSerialized]
	public PWAnchorField	toAnchor;

	override public string ToString()
	{
		return "link [" + GUID + "]";
	}
}
