using UnityEngine;
using PW.Core;

namespace PW.Editor
{
	public abstract class PWLayoutSeparator
	{
		protected PWLayoutSetting	layoutSetting;
		
		protected Event				e { get { return Event.current; } }

		public virtual void Initialize(PWGraphEditor graphEditor) {}

		public virtual PWLayoutSetting UpdateLayoutSetting(PWLayoutSetting ls)
		{
			if (ls == null)
				return null;

			if (!ls.initialized)
				return this.layoutSetting;

			this.layoutSetting = ls;
			
			return null;
		}

		public virtual PWLayoutSetting GetLayoutSetting()
		{
			return layoutSetting;
		}

		public abstract Rect Begin();

		public abstract void End();
	}
}