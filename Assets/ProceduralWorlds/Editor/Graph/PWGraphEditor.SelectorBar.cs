using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using System.Linq;

//Selector bar part of the graph editor
public partial class PWGraphEditor
{
	Vector2 	selectorScrollPosition;
	string		searchString = "";
	
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
		
		foreach (var nodeCategory in PWNodeTypeProvider.GetAllowedNodesForGraph(graph.GetType()))
		{
			DrawSelectorCase(nodeCategory.title, nodeCategory.colorSchemeName, true);
			foreach (var nodeCase in nodeCategory.typeInfos.Where(n => n.name.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0))
			{
				Rect clickableRect = DrawSelectorCase(nodeCase.name, nodeCategory.colorSchemeName);

				if (Event.current.type == EventType.MouseDown && clickableRect.Contains(Event.current.mousePosition))
					graph.CreateNewNode(nodeCase.type, -graph.panPosition + position.size / 2);
			}
		}
	}

	public void OnNodeSelectorGUI(Rect currentRect)
	{
		//draw selector bar background:
		GUI.DrawTexture(currentRect, defaultBackgroundTexture);

		//Pyramid of code:
		GUI.BeginClip(currentRect);
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
		GUI.EndClip();
	}

}
