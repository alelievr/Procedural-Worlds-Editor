using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Core
{
	[System.Serializable]
	public class PWLayoutSetting : IPWCloneable< PWLayoutSetting >
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

		public PWLayoutSetting Clone(PWLayoutSetting reuseExisting)
		{
			PWLayoutSetting setting = new PWLayoutSetting();

			setting.minHeight = minHeight;
			setting.maxHeight = maxHeight;

			setting.minWidth = minWidth;
			setting.maxWidth = maxWidth;

			setting.canBeResized = canBeResized;

			setting.separatorPosition = separatorPosition;
			setting.separatorWidth = separatorWidth;

			setting.vertical = vertical;
			setting.leftBar = leftBar;

			setting.initialized = initialized;

			return setting;
		}
	}

	[System.Serializable]
	public class PWLayoutSettings
	{
		public List< PWLayoutSetting >	settings = new List< PWLayoutSetting >();

		public void Reset()
		{
			settings.Clear();
		}
	}
}