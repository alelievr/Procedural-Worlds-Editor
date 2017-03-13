using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW
{
	[System.SerializableAttribute]
	public enum PWLinkType
	{
		BasicData,
		ThreeChannel,
		FourChannel,
		Sampler2D,
		Sampler3D,
	}

	[System.SerializableAttribute]
	public class PWLink
	{
		//distant link:
		public int					distantWindowId;
		public int					distantAnchorId;
		public string				distantName;
		public int					localWindowId;
		public int					localAnchorId;
		public string				localName;
		public SerializableColor	color;
		public PWLinkType			linkType;
	
		public PWLink(int dWin, int dAttr, string dName, int lWin, int lAttr, string lName, Color c)
		{
			distantWindowId = dWin;
			distantAnchorId = dAttr;
			distantName = dName;
			localAnchorId = lAttr;
			localWindowId = lWin;
			localName = lName;
			color = (SerializableColor)c;
		}
	}
}