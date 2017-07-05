using UnityEngine;

namespace PW.Core
{
	[System.SerializableAttribute]
	public enum PWLinkType
	{
		BasicData,
		TwoChannel,
		ThreeChannel,
		FourChannel,
		Sampler2D,
		Sampler3D,
		ChunkData,
	}

	[System.SerializableAttribute]
	public enum PWLinkHighlight
	{
		None,
		Selected,
		Delete,
		DeleteAndReset,
	}

	[System.SerializableAttribute]
	public class PWLink
	{
		//distant datas:
		public int					distantNodeId;
		public int					distantAnchorId;
		public string				distantName;
		public string				distantClassAQName;
		public int					distantIndex;
		//local datas:
		public int					localNodeId;
		public int					localAnchorId;
		public string				localName;
		public string				localClassAQName;
		public int					localIndex;
		//link datas:
		public SerializableColor	color;
		public PWLinkType			linkType;
		public PWLinkHighlight		linkHighlight;
		public bool					hover;
		public bool					selected;
		public PWNodeProcessMode		mode;
	
		public PWLink(int dWin, int dAttr, string dName, string dCName, int dIndex, int lWin, int lAttr, string lName, string lCName, int lIndex, Color c, PWLinkType lt)
		{
			distantNodeId = dWin;
			distantAnchorId = dAttr;
			distantName = dName;
			distantClassAQName = dCName;
			distantIndex = dIndex;
			localAnchorId = lAttr;
			localNodeId = lWin;
			localClassAQName = lCName;
			localIndex = lIndex;
			localName = lName;
			color = (SerializableColor)c;
			linkHighlight = PWLinkHighlight.None;
			linkType = lt;
		}
	}
}
