using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Core
{
	[System.SerializableAttribute]
	public class PWAnchor
	{
		//GUID of the anchor, used to identify anchors in NodeLinkTable
		public string				GUID;


		//AnchorField instance attached to this anchor
		[System.NonSerialized]
		public PWAnchorField		anchorFieldRef;
		//Node instance attached to this anchor
		[System.NonSerialized]
		public PWNode				nodeRef;


		//anchor connections:
		[System.NonSerialized]
		public List< PWNodeLink >	links = new List< PWNodeLink >();


		//anchor name
		public string				name = null;
		//enabled ?
		public bool					enabled = true;
		//number of links connected to this anchor
		public int					linkCount = 0;
		//link type for visual bezier curve style
		public PWLinkType			linkType = PWLinkType.BasicData;


		//hightlight mode (for replace / new / delete link visualization)
		public PWAnchorHighlight	highlighMode = PWAnchorHighlight.None;
		//visual rect of the anchor
		public Rect					rect;
		//anchor color
		public PWColorPalette		colorPalette;
		//anchor visibility
		public PWVisibility			visibility = PWVisibility.Visible;
		//override default y anchor position
		public float				forcedY = -1;


		//anchor type
		public PWAnchorType			anchorType { get { return anchorFieldRef.anchorType; } }

		
		//Editor variable:
		[System.NonSerialized]
		public bool					isLinkable = true;


		public void OnAfterDeserialized(PWAnchorField anchorField)
		{
			Initialize(anchorField);

			//we use the LinkTable in the graph to get the only instance of link stored
			//	to know why, take a look at the PWGraph.cs file.
			var nodeLinkTable = nodeRef.graphRef.nodeLinkTable;
			var linkGUIDs = nodeLinkTable.GetLinkGUIDsFromAnchorGUID(GUID);

			//here we set the anchor references in the link cauz they can't be serialized.
			foreach (var linkGUID in linkGUIDs)
			{
				var linkInstance = nodeLinkTable.GetLinkFromGUID(linkGUID);

				if (anchorFieldRef.anchorType == PWAnchorType.Input)
					linkInstance.fromAnchor = this;
				else
					linkInstance.toAnchor = this;
				
				links.Add(linkInstance);
			}
			
			//propagate the OnAfterDeserialize event.
			foreach (var link in links)
				link.OnAfterDeserialize();
		}

		//called only once (when the anchor is created)
		public void Initialize(PWAnchorField anchorField)
		{
			GUID = System.Guid.NewGuid().ToString();
			anchorFieldRef = anchorField;
			nodeRef = anchorField.nodeRef;
		}

		override public string ToString()
		{
			return "Anchor [" + GUID + "]";
		}
	}
}