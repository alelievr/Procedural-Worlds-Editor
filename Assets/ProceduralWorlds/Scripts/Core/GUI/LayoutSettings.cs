using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Core
{
	[System.Serializable]
	public class LayoutSetting : IPWCloneable< LayoutSetting >
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

		public LayoutSetting Clone(LayoutSetting reuseExisting)
		{
			LayoutSetting setting = new LayoutSetting();

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
	public class LayoutSettings
	{
		public List< LayoutSetting >	settings = new List< LayoutSetting >();

		public void Reset()
		{
			settings.Clear();
		}
	}
}