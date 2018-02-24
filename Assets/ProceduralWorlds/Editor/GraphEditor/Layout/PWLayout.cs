using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using System;
using Random = UnityEngine.Random;

namespace PW.Editor
{
	public class PWLayout
	{
		[System.NonSerialized]
		List< IPWLayoutSeparator >	layoutSeparators = new List< IPWLayoutSeparator >();
		[System.NonSerialized]
		List< PWLayoutPanel >		layoutPanels = new List< PWLayoutPanel >();

		Event						e { get { return Event.current; } }

		PWGraphEditor				graphEditor;

		PWGraph						oldGraph;

		public delegate void		DrawPanelDelegate(Rect panelRect);

		//Private constructor so the only way to create an instance of this class is PWLayoutFactory
		public PWLayout(PWGraphEditor graphEditor)
		{
			this.graphEditor = graphEditor;
		}

		public void DrawLayout()
		{
			if (oldGraph != null && oldGraph != graphEditor.graph && graphEditor.graph != null)
				UpdateLayoutSettings(graphEditor.graph.layoutSettings);

			int sepCount = layoutSeparators.Count;
			
			for (int i = 0; i < sepCount; i++)
			{
				var panel = layoutPanels[i];
				var separator = layoutSeparators[i];
			
				Rect r = separator.Begin();
				panel.Draw(r);
				separator.Split();
				separator.End();
			}

			oldGraph = graphEditor.graph;
		}

		public void AddVerticalResizablePanel(PWLayoutSetting defaultSetting)
		{
			IPWLayoutSeparator sep = new ResizableSplitView(true);

			sep.Initialize(graphEditor);
			sep.UpdateLayoutSetting(defaultSetting);

			layoutSeparators.Add(sep);
		}

		public void AddHorizontalResizablePanel(PWLayoutSetting defaultSetting)
		{
			IPWLayoutSeparator sep = new ResizableSplitView(false);

			sep.Initialize(graphEditor);
			layoutSeparators.Add(sep);
		}

		public void UpdateLayoutSettings(PWLayoutSettings layoutSettings)
		{
			int		index = 0;

			foreach (var layoutSeparator in layoutSeparators)
			{
				if (layoutSettings.settings.Count <= index)
					layoutSettings.settings.Add(new PWLayoutSetting());
				
				var layoutSetting = layoutSettings.settings[index];

				var newLayout = layoutSeparator.UpdateLayoutSetting(layoutSetting);

				//if the old layout was not initialized, we assign the new into the layout list of the graph.
				if (newLayout != null)
					layoutSettings.settings[index] = newLayout;


				index++;
			}
		}

		public void AddPanel(PWLayoutPanel panel)
		{
			layoutPanels.Add(panel);
		}

		/*public void Render2ResizablePanel(Rect position)
		{
			//split view and call delegates
			h1.Begin();
			Rect firstPanel = h2.Begin();
			// Debug.Log("firstPanel: " + firstPanel);
			if (onDrawSettingsBar != null)
				onDrawSettingsBar(firstPanel);
			Rect optionBarRect = h2.Split(resizeHandleColor);
			if (onDrawOptionBar != null)
				onDrawOptionBar(optionBarRect);
			optionBarRect = GUILayoutUtility.GetLastRect();
			h2.End();
			Rect secondPanel = h1.Split(resizeHandleColor);
			
			if (onDrawNodeSelector != null)
				onDrawNodeSelector(secondPanel);
			h1.End();
			
			if (e.type == EventType.Repaint)
			{
				//add the handleWidth to the panel for event mask + 2 pixel for UX
				firstPanel.width += h1.handleWidth + 2;
				secondPanel.xMin -= h2.handleWidth + 2;

				//update event masks with our GUI parts
				graphEditor.eventMasks[0] = firstPanel;
				graphEditor.eventMasks[1] = optionBarRect;
				graphEditor.eventMasks[2] = secondPanel;
			}

			//debug:
			// Random.InitState(42);
			// foreach (var mask in graphEditor.eventMasks)
				// EditorGUI.DrawRect(mask.Value, Random.ColorHSV());
		}*/

	}
}