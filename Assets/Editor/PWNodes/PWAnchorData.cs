using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PW
{
	public enum PWAnchorType
	{
		Input,
		Output,
		None,
	}

	public enum PWVisibility
	{
		Visible,
		Invisible,
		InvisibleWhenLinking,	//anchor is invisible while user is linking two anchors:
		Gone,					//anchor is invisible and his size is ignored
	}

	public enum PWAnchorHighlight
	{
		None,
		AttachNew,				//link will be attached to unlinked anchor
		AttachReplace,			//link will replace actual anchor link
		AttachMultiple,			//link will be added to anchor links
	}

	public class PWAnchorData
	{
		//name of the attached propery / name specified in PW I/O.
		public string				name;
		//anchor type (input / output)
		public PWAnchorType			anchorType;
		//anchor field type
		public Type					type;
		//if the anchor is rendered as multiple
		public bool					multiple;
		//list of rendered anchors:
		public Dictionary< int, PWAnchorMultiData >	multi;
		//accessor for the first anchor data:
		public PWAnchorMultiData	first { get{ return multi[0]; } set{ multi[0] = value; } }

		//properties for multiple anchors:
		public Type[]				allowedTypes;
		public int					minMultipleValues;
		public int					maxMultipleValues;
		public PWValues				multipleValueInstance;
		//current number of rendered anchors:
		public int					multipleRenderedAnchorNumber;

		static int					propDataIDs = 0;

		public class PWAnchorMultiData
		{
			public PWAnchorHighlight	highlighMode;
			public Rect					anchorRect;
			public bool					enabled;
			public Color				color;
			public string				name;
			public PWVisibility			visibility;
			//if prop is driven by external window output.
			public bool					locked;
			//the visual offset of the anchor
			public Vector2				offset;
			//the id of the actual anchor
			public int					id;
			//window id of the anchor
			public int					windowId;

			public PWAnchorMultiData(int windowId, Color color)
			{
				id = propDataIDs++;
				locked = false;
				enabled = true;
				highlighMode = PWAnchorHighlight.None;
				visibility = PWVisibility.Visible;
				this.windowId = windowId;
				this.color = color;
			}
		}

		public PWAnchorData(string name)
		{
			multiple = false;
			multipleRenderedAnchorNumber = -1;

			multi = new Dictionary< int, PWAnchorMultiData >(){{0, new PWAnchorMultiData(0, Color.white)}};
			multi[0].enabled = true;
			multi[0].visibility = PWVisibility.Visible;
			multi[0].locked = false;
			multi[0].name = name;
			multi[0].id = propDataIDs++;
		}

		/*public PWAnchorData Clone()
		{
			PWAnchorData ret = new PWAnchorData(name);

			ret.enabled = enabled;
			ret.color = color;
			ret.visibility = visibility;
			ret.locked = locked;
			ret.anchorType = anchorType;
			ret.offset = offset;
			ret.id = id;
			ret.type = type;
			ret.anchorRects = new Dictionary< int, Rect >{{0, anchorRect}};
			ret.windowId = windowId;
			ret.multiple = multiple;
			ret.allowedTypes = allowedTypes;
			ret.minMultipleValues = minMultipleValues;
			ret.maxMultipleValues = maxMultipleValues;
			ret.multipleValueInstance = multipleValueInstance;
			ret.multipleId = multipleId;

			return ret;
		}*/
	}
}