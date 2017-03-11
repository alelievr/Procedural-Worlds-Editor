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
		Gone,
	}

	public class PWAnchorData
	{
		public bool				enabled;
		public Color			color;
		public string			name;
		public PWVisibility		visibility;
		public bool				locked; //if prop is driven by external window output.
		public PWAnchorType		anchorType;
		public Vector2			offset;
		public int				id;
		public Type				type;
		public Rect				anchorRect { get {return anchorRects[0];} set {anchorRects[0] = value;}}
		public Dictionary< int, Rect > anchorRects;
		public int				windowId;

		public bool				multiple;
		public Type[]			allowedTypes;
		public int				minMultipleValues;
		public int				maxMultipleValues;
		public PWValues			multipleValueInstance;

		static int				propDataIDs = 0;

		public PWAnchorData(string name)
		{
			enabled = true;
			visibility = PWVisibility.Visible;
			locked = false;
			multiple = false;
			anchorRects = new Dictionary< int, Rect >(){{0, new Rect()}};
			this.name = name;
			id = propDataIDs++;
		}

		public PWAnchorData Clone()
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

			return ret;
		}
	}
}