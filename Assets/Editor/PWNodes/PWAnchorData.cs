using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PW
{
	public enum NodeAnchorType
	{
		Input,
		Output,
		None,
	}

	public class PWAnchorData
	{
		public bool				enabled;
		public Color			color;
		public string			name;
		public bool				visible;
		public bool				locked; //if prop is driven by external window output.
		public NodeAnchorType	anchorType;
		public Vector2			offset;
		public int				id;
		public Type				type;
		public Rect				anchorRect;
		public int				windowId;

		static int				propDataIDs = 0;

		public PWAnchorData(string name)
		{
			enabled = true;
			visible = true;
			locked = false;
			this.name = name;
			id = propDataIDs++;
		}
	}
}