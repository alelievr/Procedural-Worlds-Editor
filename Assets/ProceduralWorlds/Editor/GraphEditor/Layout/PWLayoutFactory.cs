using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using System.Linq;

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

			layout.AddVerticalResizablePanel(graphEditor);

			if (graphEditor.graph)
				layout.UpdateLayoutSettings(graphEditor.graph.layoutSettings);
			
			layout.AddPanel(CreateLayoutPanel< PWGraphSettingsPanel >(graphEditor));

			return layout;
		}
	}
}