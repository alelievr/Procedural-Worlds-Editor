using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using PW.Core;

namespace PW.Editor
{
	public static class PWLayoutFactory
	{
		static T CreateLayoutPanel< T >(PWGraphEditor graphEditor) where T : PWLayoutPanel, new()
		{
			PWLayoutPanel panel = new T() as PWLayoutPanel;

			panel.Initialize(graphEditor);
			
			return panel as T;
		}

		public static PWLayout Create2ResizablePanelLayout(PWGraphEditor graphEditor)
		{
			PWLayout		layout = new PWLayout(graphEditor);
			Rect			pos = graphEditor.position;

			var settingsPanel = CreateLayoutPanel< PWGraphSettingsPanel >(graphEditor);
			var nodeSelectorPanel = CreateLayoutPanel< PWGraphNodeSelectorPanel >(graphEditor);
			var optionPanel = CreateLayoutPanel< PWGraphOptionPanel >(graphEditor);

			float minWidth = 40;
			int	p20 = Mathf.FloorToInt(graphEditor.position.width * .2f);
			int	p50 = Mathf.FloorToInt(graphEditor.position.width * .5f);
			int	p80 = Mathf.FloorToInt(graphEditor.position.width * .8f);

			var resizablePanel1Settings = new PWLayoutSetting {
				separatorPosition = p20,
				separatorWidth = 4,
				minWidth = minWidth,
				maxWidth = p50,
				initialized = true,
			};
			var resizablePanel2Settings = new PWLayoutSetting {
				separatorPosition = p80,
				separatorWidth = 4,
				minWidth = p50,
				maxWidth = graphEditor.position.width - minWidth,
				initialized = true,
				// second = true,
			};
			var staticPanelSettings = new PWLayoutSetting {
				separatorPosition = EditorGUIUtility.singleLineHeight,
				initialized = true,
			};

			layout.AddVerticalResizablePanel(resizablePanel1Settings, settingsPanel);
			// layout.AddHorizontalStaticPanel(staticPanelSettings, optionPanel);
			layout.AddVerticalResizablePanel(resizablePanel2Settings, nodeSelectorPanel);
			
			// Debug.Log("Sep2: " + optionPanel.separator.GetLayoutSetting());


			if (graphEditor.graph != null)
				layout.UpdateLayoutSettings(graphEditor.graph.layoutSettings);
			
			return layout;
		}
	}
}