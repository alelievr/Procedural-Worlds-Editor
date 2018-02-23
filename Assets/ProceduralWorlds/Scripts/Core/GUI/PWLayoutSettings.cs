using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Core
{
	[System.Serializable]
	public class PWLayoutSettings
	{
		public float	minWidth;
		public float	maxWidth;
		
		public float	minHeight;
		public float	maxHeight;

		public bool		canBeResized;

		public float	separatorPosition;
		public float	separatorWidth;

		public bool		vertical;
	}
}