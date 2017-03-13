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
		public string				distantClassAQName;
		public int					distantIndex;
		public int					localWindowId;
		public int					localAnchorId;
		public string				localName;
		public string				localClassAQName;
		public SerializableColor	color;
		public PWLinkType			linkType;
	
		public PWLink(int dWin, int dAttr, string dName, string dCName, int dIndex, int lWin, int lAttr, string lName, string lCName, Color c)
		{
			distantWindowId = dWin;
			distantAnchorId = dAttr;
			distantName = dName;
			distantClassAQName = dCName;
			distantIndex = dIndex;
			localAnchorId = lAttr;
			localWindowId = lWin;
			localClassAQName = lCName;
			localName = lName;
			color = (SerializableColor)c;
		}
	}
}