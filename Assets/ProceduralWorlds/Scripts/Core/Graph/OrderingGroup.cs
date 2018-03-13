using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ProceduralWorlds.Core
{
	[System.Serializable]
	public class OrderingGroup
	{
	
		public Rect					orderGroupRect;
		public string				name;
		public SerializableColor	color;

		[System.NonSerialized]
		public bool					resizing = false;

		[System.NonSerialized]
		public bool					moving = false;
		[System.NonSerialized]
		public int					resizingCallbackId;

		[System.NonSerialized]
		public List< BaseNode >		innerNodes = new List< BaseNode >();

		public void Initialize(Vector2 pos)
		{
			orderGroupRect = new Rect();
			orderGroupRect.position = pos;
			orderGroupRect.size = new Vector2(240, 120);
			name = "ordering group";
			color = (SerializableColor)Color.white;
		}

	}
}
