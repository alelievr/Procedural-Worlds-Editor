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
		//distant datas:
		public int					distantWindowId;
		public int					distantAnchorId;
		public string				distantName;
		public string				distantClassAQName;
		public int					distantIndex;
		//local datas:
		public int					localWindowId;
		public int					localAnchorId;
		public string				localName;
		public string				localClassAQName;
		public int					localIndex;
		//link datas:
		public SerializableColor	color;
		public PWLinkType			linkType;
		public bool					hover;
		public bool					selected;
	
		public PWLink(int dWin, int dAttr, string dName, string dCName, int dIndex, int lWin, int lAttr, string lName, string lCName, int lIndex, Color c)
		{
			distantWindowId = dWin;
			distantAnchorId = dAttr;
			distantName = dName;
			distantClassAQName = dCName;
			distantIndex = dIndex;
			localAnchorId = lAttr;
			localWindowId = lWin;
			localClassAQName = lCName;
			localIndex = lIndex;
			localName = lName;
			color = (SerializableColor)c;
		}
	}
}