using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW;
using PW.Core;

namespace PW.Core
{
	[System.Serializable]
	public class PWNodeLink
	{
		//GUID to identify the node in the LinkTable
		public string			GUID;
	
		//ColorPalette of the link
		public PWColorSchemeName colorSchemeName;
		//link hightlight mode 
		public PWLinkHighlight	highlight;
		//is selected ?
		public bool				selected;
	
		//anchor where the link is connected:
		[System.NonSerialized]
		//from where the link is conected, this anchor will always be an Input anchor
		public PWAnchor			fromAnchor;
		[System.NonSerialized]
		//this anchor will always be an Output anchor.
		public PWAnchor			toAnchor;
		[System.NonSerialized]
		public PWNode			fromNode;
		[System.NonSerialized]
		public PWNode			toNode;

		//Control id for UnityEditor to select the link:
		[System.NonSerialized]
		public int				controlId = -1;


		//called once (when link is created only)
		public void Initialize(PWAnchor fromAnchor, PWAnchor toAnchor)
		{
			this.fromAnchor = fromAnchor;
			this.toAnchor = toAnchor;
			fromNode = fromAnchor.nodeRef;
			toNode = toAnchor.nodeRef;
			GUID = System.Guid.NewGuid().ToString();
		}

		//this function will be called twiced, from the two linked anchors
		// and so will receive two different anchor in parameter
		public void OnAfterDeserialize(PWAnchor anchor)
		{
			if (anchor.anchorType == PWAnchorType.Output)
				fromAnchor = anchor;
			else
				toAnchor = anchor;
			
			if (fromAnchor != null)
				fromNode = fromAnchor.nodeRef;
			if (toAnchor != null)
				toNode = toAnchor.nodeRef;
			
			//update link color:
			if (fromAnchor != null)
			{
				colorSchemeName = fromAnchor.colorSchemeName;
			}
		}

		public void ResetHighlight()
		{
			if (selected)
				highlight = PWLinkHighlight.Selected;
			else
				highlight = PWLinkHighlight.None;
		}
	
		override public string ToString()
		{
			return "link from [" + fromAnchor + "] to [" + toAnchor + "], GUID: " + GUID;
		}
	}
}