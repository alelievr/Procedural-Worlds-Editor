using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PW;

namespace PW
{
	[System.SerializableAttribute]
	public struct PWAnchorInfo {
	
		public Rect			anchorRect;
		public bool			mouseAbove;
		public Color		anchorColor;
		public Type			fieldType;
		public PWAnchorType	anchorType;
		public int			windowId;
		public int			anchorId;
	
		public PWAnchorInfo(Rect anchorRect, Color anchorColor,
			Type fieldType, PWAnchorType anchorType,
			int windowId, int anchorId)
		{
			this.anchorRect = anchorRect;
			this.anchorColor = anchorColor;
			this.mouseAbove = false;
			this.fieldType = fieldType;
			this.anchorType = anchorType;
			this.windowId = windowId;
			this.anchorId = anchorId;
		}
	}
}