using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Core
{
	[System.Serializable]
	public class PWLayoutSetting
	{
		public float	minWidth;
		public float	maxWidth;
		
		public float	minHeight;
		public float	maxHeight;

		public bool		canBeResized;

		public float	separatorPosition;
		public float	separatorWidth;

		public bool		vertical;
		public bool		leftBar;
		
		public bool		initialized;
	}

	[System.Serializable]
	public class PWLayoutSettings
	{
		public List< PWLayoutSetting >	settings = new List< PWLayoutSetting >();
	}
}