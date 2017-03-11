using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW
{
	[System.SerializableAttribute]
	public class PWLink
	{
		//distant link:
		public int					distantWindowId;
		public int					distantAnchorId;
		public int					localWindowId;
		public int					localAnchorId;
		public SerializableColor	color;
	
		public PWLink(int dWin, int dAttr, int lWin, int lAttr, Color c)
		{
			distantWindowId = dWin;
			distantAnchorId = dAttr;
			localAnchorId = lAttr;
			localWindowId = lWin;
			color = (SerializableColor)c;
		}
	}
}