using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;
using System.Linq;
using ProceduralWorlds.Core;
using ProceduralWorlds;
using System;

namespace ProceduralWorlds.Editor
{
	public class BaseGraphNodeSelectorPanel : LayoutPanel
	{
		//node selector datas
		Vector2 	selectorScrollPosition;
		string		searchString = "";

		
		//Styles
		GUIStyle	nodeSelectorTitleStyle;
		GUIStyle	nodeSelectorCaseStyle;
		
		GUIStyle	toolbarSearchCancelButtonStyle;
		GUIStyle	toolbarSearchTextStyle;
		GUIStyle	toolbarStyle;
		
		GUIStyle	panelBackgroundStyle;

		//current window rect:
		Rect		windowRect;

		public override void OnLoadStyle()
		{
			using (DefaultGUISkin.Get())
			{
				toolbarStyle = new GUIStyle("Toolbar");
				toolbarSearchTextStyle = new GUIStyle("ToolbarSeachTextField");
				toolbarSearchCancelButtonStyle = new GUIStyle("ToolbarSeachCancelButton");
			}
			
			nodeSelectorTitleStyle = new GUIStyle("NodeSelectorTitle");
			nodeSelectorCaseStyle = new GUIStyle("NodeSelectorCase");
			panelBackgroundStyle = new GUIStyle("PanelBackground");
		}

		void DefaultNodeClickAction(Type t)
		{
			graphRef.CreateNewNode(t, -graphRef.panPosition + windowRect.center);
		}
		
		Rect DrawSelectorCase(string name, ColorSchemeName colorSchemeName, bool title = false)
		{
			if (title)
			{
				GUI.color = ColorTheme.GetSelectorHeaderColor(colorSchemeName);
				GUILayout.Label(name, nodeSelectorTitleStyle);
				GUI.color = Color.white;
			}
			else
				GUILayout.Label(name, nodeSelectorCaseStyle);
	
			return GUILayoutUtility.GetLastRect();
		}
	
		void DrawSelector()
		{
			EditorGUIUtility.labelWidth = 0;
			EditorGUIUtility.fieldWidth = 0;
			GUILayout.BeginHorizontal(toolbarStyle);
			{
				searchString = GUILayout.TextField(searchString, toolbarSearchTextStyle);
				if (GUILayout.Button("", toolbarSearchCancelButtonStyle))
				{
					// Remove focus if cleared
					searchString = "";
					GUI.FocusControl(null);
				}
			}
			GUILayout.EndHorizontal();
			
			foreach (var nodeCategory in NodeTypeProvider.GetAllowedNodesForGraph(graphRef.graphType))
			{
				DrawSelectorCase(nodeCategory.title, nodeCategory.colorSchemeName, true);
				foreach (var nodeCase in nodeCategory.typeInfos.Where(n => n.name.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0))
				{
					Rect clickableRect = DrawSelectorCase(nodeCase.name, nodeCategory.colorSchemeName);

					if (e.type == EventType.MouseDown && e.button == 0 && clickableRect.Contains(Event.current.mousePosition))
					{
						Vector2 pos = graphEditor.position.center - graphEditor.graph.panPosition;
						graphEditor.graph.CreateNewNode(nodeCase.type, pos);
					}
				}
			}
		}
	
		public override void DrawDefault(Rect currentRect)
		{
			Profiler.BeginSample("[PW] Rendering node selector");

			//draw selector bar background:
			GUI.DrawTexture(currentRect, ColorTheme.defaultBackgroundTexture);

			windowRect = new Rect(0, 0, currentRect.xMin + currentRect.width, currentRect.yMin + currentRect.height);
	
			selectorScrollPosition = EditorGUILayout.BeginScrollView(selectorScrollPosition, GUILayout.ExpandWidth(true));
			{
				EditorGUILayout.BeginVertical(panelBackgroundStyle);
				{
					DrawSelector();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndScrollView();

			Profiler.EndSample();
		}
	}
}