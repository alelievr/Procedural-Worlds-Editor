using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using PW.Core;
using PW;
using System;

namespace PW.Editor
{
	public class PWGraphNodeSelectorBar
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


		//current event
		Event		e { get { return Event.current; } }
		//current window rect:
		Rect		windowRect;


		//Graph reference
		PWGraph		graph;


		//node selector callbacks
		public Action< Type > OnNodeClicked;

		public PWGraphNodeSelectorBar(PWGraph graph)
		{
			this.graph = graph;
			OnNodeClicked = DefaultNodeClickAction;
		}

		public void LoadStyles()
		{
			using (new DefaultGUISkin())
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
			graph.CreateNewNode(t, -graph.panPosition + windowRect.center);
		}
		
		Rect DrawSelectorCase(string name, PWColorSchemeName colorSchemeName, bool title = false)
		{
			if (title)
			{
				GUI.color = PWColorTheme.GetSelectorHeaderColor(colorSchemeName);
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
			
			foreach (var nodeCategory in PWNodeTypeProvider.GetAllowedNodesForGraph(graph.graphType))
			{
				DrawSelectorCase(nodeCategory.title, nodeCategory.colorSchemeName, true);
				foreach (var nodeCase in nodeCategory.typeInfos.Where(n => n.name.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0))
				{
					Rect clickableRect = DrawSelectorCase(nodeCase.name, nodeCategory.colorSchemeName);

					if (e.type == EventType.MouseDown && e.button == 0 && clickableRect.Contains(Event.current.mousePosition))
						OnNodeClicked(nodeCase.type);
				}
			}
		}
	
		public void DrawNodeSelector(Rect currentRect)
		{
			//draw selector bar background:
			GUI.DrawTexture(currentRect, PWColorTheme.defaultBackgroundTexture);

			windowRect = new Rect(0, 0, currentRect.xMin + currentRect.width, currentRect.yMin + currentRect.height);
	
			//Pyramid of layouts:
			// GUI.BeginClip(currentRect);
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(currentRect.width), GUILayout.Height(currentRect.height));
				{
					selectorScrollPosition = EditorGUILayout.BeginScrollView(selectorScrollPosition, GUILayout.ExpandWidth(true));
					{
						EditorGUILayout.BeginVertical(panelBackgroundStyle);
						{
							DrawSelector();
						}
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndScrollView();
				}
				EditorGUILayout.EndHorizontal();
			}
			// GUI.EndClip();
		}
	}
}