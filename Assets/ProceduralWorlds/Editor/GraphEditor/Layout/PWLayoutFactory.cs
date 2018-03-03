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

			float minWidth = 60;
			int	p20 = Mathf.FloorToInt(graphEditor.position.width * .2f);
			int	p15 = Mathf.FloorToInt(graphEditor.position.width * .15f);
			int	p50 = Mathf.FloorToInt(graphEditor.position.width * .5f);

			var resizablePanel1Settings = new PWLayoutSetting {
				separatorPosition = p20,
				separatorWidth = 4,
				minWidth = minWidth,
				maxWidth = p50,
				initialized = true,
			};
			//the layout infos (width, min, max, ...) are inverted because leftBar is true
			var resizablePanel2Settings = new PWLayoutSetting {
				separatorPosition = p15,
				separatorWidth = 4,
				minWidth = minWidth,
				maxWidth = p50,
				initialized = true,
				leftBar = true,
			};
			var staticPanelSettings = new PWLayoutSetting {
				separatorPosition = EditorGUIUtility.singleLineHeight,
				initialized = true,
			};

			layout.BeginHorizontal();
			{
				layout.ResizablePanel(resizablePanel1Settings, settingsPanel);
				layout.AutoSizePanel(staticPanelSettings, optionPanel);
				layout.ResizablePanel(resizablePanel2Settings, nodeSelectorPanel);
			}
			layout.EndHorizontal();

			if (graphEditor.graph != null)
				layout.UpdateLayoutSettings(graphEditor.graph.layoutSettings);
			
			return layout;
		}
	}
}