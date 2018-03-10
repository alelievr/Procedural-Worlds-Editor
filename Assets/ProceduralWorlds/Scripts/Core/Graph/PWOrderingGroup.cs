using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace PW.Core
{
	[System.Serializable]
	public class PWOrderingGroup
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
		public List< PWNode >		innerNodes = new List< PWNode >();

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
