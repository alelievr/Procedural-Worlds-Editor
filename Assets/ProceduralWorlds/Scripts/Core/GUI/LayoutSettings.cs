using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Core
{
	[System.Serializable]
	public class LayoutSetting : IPWCloneable< LayoutSetting >
	{
		public Rect		windowRect;

		public float	minWidthPercent;
		public float	maxWidthPercent;
		
		public float	minHeightPercent;
		public float	maxHeightPercent;

		public float	separatorPositionPercent;

		public float	minWidth
		{
			get { return minWidthPercent * windowRect.width; }
			set { minWidthPercent = value / windowRect.width; }
		}

		public float	maxWidth
		{
			get { return maxWidthPercent * windowRect.width; }
			set { maxWidthPercent = value / windowRect.width; }
		}
		
		public float	minHeight
		{
			get { return minHeightPercent * windowRect.height; }
			set { minHeightPercent = value / windowRect.height; }
		}

		public float	maxHeight
		{
			get { return maxHeightPercent * windowRect.height; }
			set { maxHeightPercent = value / windowRect.height; }
		}

		public bool		canBeResized;

		public float	separatorPosition
		{
			get { return separatorPositionPercent * windowRect.width; }
			set { separatorPositionPercent = value / windowRect.width; }
		}
		public float	separatorWidth;

		public bool		vertical;
		public bool		leftBar;
		
		public bool		initialized;

		public LayoutSetting(Rect window)
		{
			windowRect = window;
		}

		public LayoutSetting Clone(LayoutSetting reuseExisting)
		{
			LayoutSetting setting = new LayoutSetting(windowRect);

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