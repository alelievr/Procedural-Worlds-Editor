using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;

namespace PW.Editor
{
	public abstract class PWLayoutPanel
	{
		public delegate void			OnGUI(Rect rect);

		public List< PWLayoutPanel >	childPanels = new List< PWLayoutPanel >();

		public PWLayoutSeparator		separator;

		protected PWGraphEditor			graphEditor;
		protected PWGraph				graphRef { get { return graphEditor.graph; } }
		
		protected DelayedChanges		delayedChanges = new DelayedChanges();

		public OnGUI					onGUI;

		Rect							oldWindowRect;

		[System.NonSerialized]
		bool							first = true;

		public void Initialize(PWGraphEditor graphEditor)
		{
			this.graphEditor = graphEditor;
			onGUI = DrawDefault;

			OnEnable();
		}

		public virtual void Draw(Rect rect)
		{
			if (first)
			{
				OnLoadStyle();
				first = false;
			}
			onGUI(rect);
			delayedChanges.Update();

			if (oldWindowRect != Rect.zero && oldWindowRect != graphEditor.position)
				OnWindowResize(oldWindowRect);

			oldWindowRect = graphEditor.position;
		}

		public virtual void OnEnable() {}

		public virtual void OnWindowResize(Rect oldWindowRect) {}

		public virtual void OnLoadStyle() {}

		public abstract void DrawDefault(Rect rect);
	}
}