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

			var resizablePanel1Settings = new PWLayoutSetting {
				separatorPosition = 200,
				separatorWidth = 4,
				minWidth = 40,
				maxWidth = 500,
				initialized = true,
			};
			var resizablePanel2Settings = new PWLayoutSetting{
				separatorPosition = 800,
				separatorWidth = 4,
				minWidth = 80,
				maxWidth = 1000,
				initialized = true,
			};

			layout.AddVerticalResizablePanel(resizablePanel1Settings);
			layout.AddPanel(settingsPanel);
			layout.AddVerticalResizablePanel(resizablePanel2Settings);
			layout.AddPanel(nodeSelectorPanel);

			if (graphEditor.graph != null)
				layout.UpdateLayoutSettings(graphEditor.graph.layoutSettings);
				

			return layout;
		}
	}
}