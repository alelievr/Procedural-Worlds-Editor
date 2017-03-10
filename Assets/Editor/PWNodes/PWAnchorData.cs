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
			visible = true;
			locked = false;
			multiple = false;
			anchorRects = new Dictionary< int, Rect >(){{0, new Rect()}};
			this.name = name;
			id = propDataIDs++;
		}
	}
}