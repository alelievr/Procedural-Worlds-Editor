using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PW
{
	[System.SerializableAttribute]
	public enum PWAnchorType
	{
		Input,
		Output,
		None,
	}

	[System.SerializableAttribute]
	public enum PWVisibility
	{
		Visible,
		Invisible,
		InvisibleWhenLinking,	//anchor is invisible while user is linking two anchors:
		Gone,					//anchor is invisible and his size is ignored
	}

	[System.SerializableAttribute]
	public enum PWAnchorHighlight
	{
		None,
		AttachNew,				//link will be attached to unlinked anchor
		AttachReplace,			//link will replace actual anchor link
		AttachMultiple,			//link will be added to anchor links
	}

	[System.SerializableAttribute]
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
		public bool					displayHiddenMultipleAnchors;

		static int					propDataIDs = 0;

		[System.SerializableAttribute]
		public class PWAnchorMultiData
		{
			public PWAnchorHighlight	highlighMode;
			public Rect					anchorRect;
			public bool					enabled;
			public SerializableColor	color;
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
			//external link connected to this anchor
			public int					linkCount;
			//if anchor is an additional hidden anchor (only visible when creating a new link)
			public bool					additional;

			public PWAnchorMultiData(int windowId, Color color)
			{
				id = propDataIDs++;
				locked = false;
				additional = false;
				enabled = true;
				linkCount = 0;
				highlighMode = PWAnchorHighlight.None;
				visibility = PWVisibility.Visible;
				this.windowId = windowId;
				this.color = (SerializableColor)color;
			}
		}

		public PWAnchorData(string name)
		{
			multiple = false;
			displayHiddenMultipleAnchors = false;

			multi = new Dictionary< int, PWAnchorMultiData >(){{0, new PWAnchorMultiData(0, Color.white)}};
			multi[0].enabled = true;
			multi[0].visibility = PWVisibility.Visible;
			multi[0].locked = false;
			multi[0].name = name;
			multi[0].id = propDataIDs++;
		}

		public int AddNewAnchor()
		{
			return AddNewAnchor(first.windowId, first.color);
		}

		public int AddNewAnchor(int winId, Color c)
		{
			int		index = 1;
			while (true)
			{
				if (!multi.ContainsKey(index))
				{
					multi[index] = new PWAnchorMultiData(winId, c);
					multipleValueInstance.Add(null);
					return index;
				}
				index++;
			}
		}
	}
}