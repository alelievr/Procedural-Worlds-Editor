using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Node;
using PW.Core;
using System.Linq;

//Right node selector
public partial class PWMainGraphEditor {

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

	void DrawSelector(Rect currentRect)
	{
		GUI.DrawTexture(currentRect, defaultBackgroundTexture);
		mainGraph.selectorScrollPosition = EditorGUILayout.BeginScrollView(mainGraph.selectorScrollPosition, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginVertical(panelBackgroundStyle);
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
				
				foreach (var nodeCategory in PWNodeTypeProvider.GetAllowedNodesForGraph(graph.GetType()))
				{
					DrawSelectorCase(nodeCategory.title, nodeCategory.colorSchemeName, true);
					foreach (var nodeCase in nodeCategory.typeInfos.Where(n => n.name.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0))
					{
						Rect clickableRect = DrawSelectorCase(nodeCase.name, nodeCategory.colorSchemeName);
	
						if (Event.current.type == EventType.MouseDown && clickableRect.Contains(Event.current.mousePosition))
							graph.CreateNewNode(nodeCase.type, -mainGraph.panPosition + position.size / 2);
					}
				}
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndScrollView();
	}
}
	