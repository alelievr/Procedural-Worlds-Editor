using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;

namespace PW.Editor
{
	public abstract class PWGraphPanel
	{
		public delegate void		OnGUI(Rect rect);

		protected PWGraphEditor		graphEditor;
		protected PWGraph			graphRef { get { return graphEditor.graph; } }
		
		protected DelayedChanges	delayedChanges = new DelayedChanges();

		public OnGUI				onGUI;

		public void Initialize(PWGraphEditor graphEditor)
		{
			this.graphEditor = graphEditor;
			onGUI = DrawDefault;
		}

		public virtual void Draw(Rect rect)
		{
			onGUI(rect);
			delayedChanges.Update();
		}

		public abstract void DrawDefault(Rect rect);
	}
}