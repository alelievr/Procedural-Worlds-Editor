using UnityEngine;
using System;
using System.Linq;

namespace PW
{
	[System.SerializableAttribute]
	public struct PWAnchorInfo {
	
		public string		fieldName;
		public Rect			anchorRect;
		public bool			mouseAbove;
		public Color		anchorColor;
		public Type			fieldType;
		public PWAnchorType	anchorType;
		public int			nodeId;
		public int			anchorId;
		public bool			generic;
		public string		classAQName;
		public int			propIndex;
		public SerializableType[]		allowedTypes;
		public PWLinkType	linkType;
		public int			linkCount;
	
		public PWAnchorInfo(string name, Rect anchorRect, Color anchorColor,
			Type fieldType, PWAnchorType anchorType,
			int nodeId, int anchorId,
			string classAQName, int propIndex,
			bool generic, Type[] allowedTypes,
			PWLinkType linkType, int linkCount) : this(name, anchorRect,
				anchorColor, fieldType,
				anchorType, nodeId,
				anchorId, classAQName, propIndex,
				generic, allowedTypes.Cast< SerializableType >().ToArray(),
				linkType, linkCount) {}
		
		public PWAnchorInfo(string fieldName, Rect anchorRect, Color anchorColor,
			Type fieldType, PWAnchorType anchorType,
			int nodeId, int anchorId,
			string classAQName, int propIndex,
			bool generic, SerializableType[] allowedTypes,
			PWLinkType linkType, int linkCount)
		{
			this.fieldName = fieldName;
			this.anchorRect = anchorRect;
			this.anchorColor = anchorColor;
			this.mouseAbove = false;
			this.fieldType = fieldType;
			this.anchorType = anchorType;
			this.nodeId = nodeId;
			this.anchorId = anchorId;
			this.generic = generic;
			this.allowedTypes = allowedTypes;
			this.classAQName = classAQName;
			this.propIndex = propIndex;
			this.linkType = linkType;
			this.linkCount = linkCount;
		}
	}
}