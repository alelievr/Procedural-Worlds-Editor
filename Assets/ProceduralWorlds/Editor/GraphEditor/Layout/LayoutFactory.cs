using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	public static class LayoutFactory
	{
		static T CreateLayoutPanel< T >(BaseGraphEditor graphEditor) where T : LayoutPanel, new()
		{
			LayoutPanel panel = new T() as LayoutPanel;

			panel.Initialize(graphEditor);
			
			return panel as T;
		}

		public static Layout Create2ResizablePanelLayout(BaseGraphEditor graphEditor)
		{
			Layout		layout = new Layout(graphEditor);

			var settingsPanel = CreateLayoutPanel< BaseGraphSettingsPanel >(graphEditor);
			var nodeSelectorPanel = CreateLayoutPanel< BaseGraphNodeSelectorPanel >(graphEditor);
			var optionPanel = CreateLayoutPanel< BaseGraphOptionPanel >(graphEditor);

			float minWidth = 60;
			int	p20 = Mathf.FloorToInt(graphEditor.position.width * .2f);
			int	p15 = Mathf.FloorToInt(graphEditor.position.width * .15f);
			int	p50 = Mathf.FloorToInt(graphEditor.position.width * .5f);

			var resizablePanel1Settings = new LayoutSetting(graphEditor.position) {
				separatorPosition = p20,
				separatorWidth = 4,
				minWidth = minWidth,
				maxWidth = p50,
				initialized = true,
			};
			//the layout infos (width, min, max, ...) are inverted because leftBar is true
			var resizablePanel2Settings = new LayoutSetting(graphEditor.position) {
				separatorPosition = p15,
				separatorWidth = 4,
				minWidth = minWidth,
				maxWidth = p50,
				initialized = true,
				leftBar = true,
			};
			var staticPanelSettings = new LayoutSetting(graphEditor.position) {
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