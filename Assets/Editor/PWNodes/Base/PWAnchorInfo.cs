using UnityEngine;
using System;

namespace PW
{
	[System.SerializableAttribute]
	public struct PWAnchorInfo {
	
		public string		name;
		public Rect			anchorRect;
		public bool			mouseAbove;
		public Color		anchorColor;
		public Type			fieldType;
		public PWAnchorType	anchorType;
		public int			windowId;
		public int			anchorId;
		public bool			generic;
		public Type[]		allowedTypes;
	
		public PWAnchorInfo(string name, Rect anchorRect, Color anchorColor,
			Type fieldType, PWAnchorType anchorType,
			int windowId, int anchorId,
			bool generic, Type[] allowedTypes)
		{
			this.name = name;
			this.anchorRect = anchorRect;
			this.anchorColor = anchorColor;
			this.mouseAbove = false;
			this.fieldType = fieldType;
			this.anchorType = anchorType;
			this.windowId = windowId;
			this.anchorId = anchorId;
			this.generic = generic;
			this.allowedTypes = allowedTypes;
		}
	}
}