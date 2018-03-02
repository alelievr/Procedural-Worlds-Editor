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
		PWLayoutPanel				mainPanel;

		static Event				e { get { return Event.current; } }

		PWGraphEditor				graphEditor;

		PWGraph						oldGraph;

		public delegate void		DrawPanelDelegate(Rect panelRect);

		PWLayoutPanel				lastPanel;

		//Private constructor so the only way to create an instance of this class is PWLayoutFactory
		public PWLayout(PWGraphEditor graphEditor)
		{
			this.graphEditor = graphEditor;
		}

		void DrawLayoutPanel(PWLayoutPanel panel)
		{
			var setting = panel.separator.GetLayoutSetting();


			if (setting == null)
				return ;

			if (setting.second)
			{
				panel.separator.Begin();
				foreach (var p in panel.childPanels)
					DrawLayoutPanel(p);
				Rect r = panel.separator.Split();
				panel.Draw(r);
				EditorGUILayout.LabelField("GKLJKLHRJHFIUHSLKJFEOJ");
				panel.separator.End();
			}
			else
			{
				Rect r = panel.separator.Begin();
				EditorGUILayout.LabelField("IOFJEOIJEIOGJOEIG");
				panel.Draw(r);
				panel.separator.Split();
				foreach (var p in panel.childPanels)
					DrawLayoutPanel(p);
				panel.separator.End();
			}
		}

		public void DrawLayout()
		{
			if (oldGraph != null && oldGraph != graphEditor.graph && graphEditor.graph != null)
				UpdateLayoutSettings(graphEditor.graph.layoutSettings);

			DrawLayoutPanel(mainPanel);

			oldGraph = graphEditor.graph;
		}

		PWLayoutPanel AddPanel(PWLayoutSetting defaultSetting, PWLayoutSeparator sep, PWLayoutPanel panel, PWLayoutPanel parentPanel)
		{
			sep.Initialize(graphEditor);

			sep.UpdateLayoutSetting(defaultSetting);
		
			if (parentPanel == null)
				parentPanel = lastPanel;
			
			if (parentPanel == panel)
				return null;

			if (mainPanel == null)
				mainPanel = panel;
			else
				parentPanel.childPanels.Add(panel);

			panel.separator = sep;
			lastPanel = panel;

			return panel;
		}

		public PWLayoutPanel AddVerticalResizablePanel(PWLayoutSetting defaultSetting, PWLayoutPanel panel, PWLayoutPanel parentPanel = null)
		{
			return AddPanel(defaultSetting, new ResizablePanelSeparator(PWLayoutOrientation.Vertical), panel, parentPanel);
		}

		public PWLayoutPanel AddHorizontalResizablePanel(PWLayoutSetting defaultSetting, PWLayoutPanel panel, PWLayoutPanel parentPanel = null)
		{
			return AddPanel(defaultSetting, new ResizablePanelSeparator(PWLayoutOrientation.Horizontal), panel, parentPanel);
		}

		public PWLayoutPanel AddVerticalStaticPanel(PWLayoutSetting defaultSetting, PWLayoutPanel panel, PWLayoutPanel parentPanel = null)
		{
			return AddPanel(defaultSetting, new StaticPanelSeparator(PWLayoutOrientation.Vertical), panel, parentPanel);
		}

		public PWLayoutPanel AddHorizontalStaticPanel(PWLayoutSetting defaultSetting, PWLayoutPanel panel, PWLayoutPanel parentPanel = null)
		{
			return AddPanel(defaultSetting, new StaticPanelSeparator(PWLayoutOrientation.Horizontal), panel, parentPanel);
		}

		public void UpdateLayoutSettings(PWLayoutSettings layoutSettings)
		{
			int		index = 0;

			var panels = new Stack< PWLayoutPanel >();

			panels.Push(mainPanel);

			while (panels.Count != 0)
			{
				var panel = panels.Pop();

				if (layoutSettings.settings.Count <= index)
					layoutSettings.settings.Add(new PWLayoutSetting());
				
				var layoutSetting = layoutSettings.settings[index];

				var newLayout = panel.separator.UpdateLayoutSetting(layoutSetting);

				//if the old layout was not initialized, we assign the new into the layout list of the graph.
				if (newLayout != null)
					layoutSettings.settings[index] = newLayout;

				foreach (var p in panel.childPanels)
					panels.Push(p);

				index++;
			}
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