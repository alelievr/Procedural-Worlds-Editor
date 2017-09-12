using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PW.Core
{
	[System.SerializableAttribute]
	public enum PWAnchorType
	{
		Input,
		Output,
		None,
	}

	[System.SerializableAttribute]
	public enum PWAnchorHighlight
	{
		None,
		AttachNew,				//link will be attached to unlinked anchor
		AttachReplace,			//link will replace actual anchor link
		AttachAdd,				//link will be added to anchor links
	}

	[System.SerializableAttribute]
	public class PWAnchor
	{
		//node instance where the anchor is.
		public PWNode						nodeRef;
		
		//list of rendered anchors:
		public List< PWAnchorData >			fieldAnchors = new List< PWAnchorData >();
		
		//instance of the field
		public object						fieldInstance;

		//name of the attached propery / name specified in PW I/O.
		public string						fieldName;
		//anchor type (input / output)
		public PWAnchorType					anchorType;
		//anchor field type
		public SerializableType				fieldType;

		//anchor name if specified in PWInput or PWOutput else null
		public string						anchorName;
		//the visual offset of the anchor
		public Vector2						offset;
		//the visual padding between multiple anchor of the same field
		public int							multiPadding;

		//properties for multiple anchors:
		public SerializableType[]			allowedTypes;
		//min allowed input values
		public int							minMultipleValues;
		//max allowed values
		public int							maxMultipleValues;

		//if the anchor value is required to compute result
		public bool							required;
		//if the anchor is selected
		public bool							selected;

		[System.SerializableAttribute]
		public class PWAnchorData
		{
			//anchor connections:
			public PWNodeLink			links;
			//anchor instance attached to this anchorfield
			public PWAnchor				anchorRef;

			//anchor name
			public string				name = null;
			//enabled ?
			public bool					enabled = true;
			//links connected to this anchor
			public int					linkCount = 0;
			//link type for visual bezier curve style
			public PWLinkType			linkType = PWLinkType.BasicData;

			//hightlight mode (for replace / new / delete link visualization)
			public PWAnchorHighlight	highlighMode = PWAnchorHighlight.None;
			//visual rect of the anchor
			public Rect					anchorRect;
			//anchor color
			public PWColorPalette		colorPalette;
			//anchor visibility
			public PWVisibility			visibility = PWVisibility.Visible;
			//override default y anchor position
			public float				forcedY = -1;

			public PWAnchorData() {}
		}

		public void RemoveAllAnchors()
		{
			fieldAnchors.Clear();
		}
	}
}
