using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW
{
	[System.SerializableAttribute]
	public class PWLink
	{
		//distant link:
		public int		windowId;
		public int		distantAnchorId;
		public Color	color;
	
		//connected local property:
		public int		localAnchorId;
	
		public PWLink(int dWin, int dAttr, int lAttr, Color c)
		{
			windowId = dWin;
			distantAnchorId = dAttr;
			localAnchorId = lAttr;
			color = c;
		}
	}
}