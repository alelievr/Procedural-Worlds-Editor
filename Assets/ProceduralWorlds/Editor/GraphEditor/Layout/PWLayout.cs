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
		List< IPWLayoutSeparator >	layoutSeparators = new List< IPWLayoutSeparator >();
		List< PWLayoutPanel >		layoutPanels = new List< PWLayoutPanel >();

		Event		e { get { return Event.current; } }

		public delegate void	DrawPanelDelegate(Rect panelRect);

		//Private constructor so the only way to create an instance of this class is PWLayoutFactory
		public PWLayout(PWGraphEditor graphEditor)
		{

		}

		public void DrawLayout()
		{

		}

		public void AddVerticalResizablePanel(PWGraphEditor graphEditor)
		{
			layoutSeparators.Add(new ResizableSplitView(true));
		}

		public void AddHorizontalResizablePanel(PWGraphEditor graphEditor)
		{
			layoutSeparators.Add(new ResizableSplitView(false));
		}

		public void UpdateLayoutSettings(PWLayoutSettings layoutSettings)
		{
			foreach (var layoutSeparator in layoutSeparators)
				layoutSeparator.UpdateLayoutSettings(layoutSettings);
		}

		public void AddPanel(PWLayoutPanel panel)
		{
			layoutPanels.Add(panel);
		}

		public void Render2ResizablePanel(PWGraphEditor graphEditor, Rect position)
		{
			/*//update min and max positions for resizable panel 
			h1.UpdateMinMax(position.width / 2, position.width - 3);
			h2.UpdateMinMax(50, position.width / 2);
	
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
				// EditorGUI.DrawRect(mask.Value, Random.ColorHSV());*/
		}

	}
}