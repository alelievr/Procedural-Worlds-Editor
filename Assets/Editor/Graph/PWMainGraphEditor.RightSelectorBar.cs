using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//Right node selector
public partial class PWMainGraphEditor {

	void AddToSelector(string key, string color, GUIStyle windowColor, GUIStyle windowColorSelected, params object[] objs)
	{
		if (!nodeSelectorList.ContainsKey(key))
			nodeSelectorList[key] = new PWNodeStorageCategory(color);
		for (int i = 0; i < objs.Length; i += 2)
			nodeSelectorList[key].nodes.Add(new PWNodeStorage((string)objs[i], (Type)objs[i + 1], windowColor, windowColorSelected));
	}
	
	void InitializeNodeSelector()
	{
		//setup nodeList:
		foreach (var n in nodeSelectorList)
			n.Value.nodes.Clear();
		
		AddToSelector("Primitive types", "redNode", redNodeWindow, redNodeWindowSelected,
			"Slider", typeof(PWNodeSlider),
			"Constant", typeof(PWNodeConstant),
			"Color", typeof(PWNodeColor),
			"Surface maps", typeof(PWNodeSurfaceMaps),
			"GameObject", typeof(PWNodeGameObject),
			"Material", typeof(PWNodeMaterial),
			"Texture2D", typeof(PWNodeTexture2D),
			"Mesh", typeof(PWNodeMesh)
		);
		AddToSelector("Operations", "yellowNode", yellowNodeWindow, yellowNodeWindowSelected,
			"Add", typeof(PWNodeAdd),
			"Curve", typeof(PWNodeCurve)
		);
		AddToSelector("Biomes", "greenNode", greenNodeWindow, greenNodeWindowSelected,
			"Water Level", typeof(PWNodeWaterLevel),
			"To Biome data", typeof(PWNodeBiomeData),
			"Biome switch", typeof(PWNodeBiomeSwitch),
			"Biome Binder", typeof(PWNodeBiomeBinder),
			"Biome blender", typeof(PWNodeBiomeBlender),
			"Biome surface", typeof(PWNodeBiomeSurface),
			"Biome terrain", typeof(PWNodeBiomeTerrain),
			"Biome temperature map", typeof(PWNodeBiomeTemperature),
			"Biome wetness map", typeof(PWNodeBiomeWetness)
		);
		AddToSelector("Landforms", "cyanNode", cyanNodeWindow, cyanNodeWindowSelected,
			"Terrain detail", typeof(PWNodeTerrainDetail)
		);
		AddToSelector("Noises And Masks", "blueNode", blueNodeWindow, blueNodeWindowSelected,
			"Perlin noise 2D", typeof(PWNodePerlinNoise2D),
			"Circle Noise Mask", typeof(PWNodeCircleNoiseMask)
		);
		AddToSelector("Materializers", "purpleNode", purpleNodeWindow, purpleNodeWindowSelected,
			"SideView 2D terrain", typeof(PWNodeSideView2DTerrain),
			"TopDown 2D terrain", typeof(PWNodeTopDown2DTerrain)
		);
		AddToSelector("Debug", "orangeNode", orangeNodeWindow, orangeNodeWindowSelected,
			"DebugLog", typeof(PWNodeDebugLog)
		);
		AddToSelector("Custom", "whiteNode", whiteNodeWindow, whiteNodeWindowSelected);
	}
	
	Rect DrawSelectorCase(string name, string color, bool title = false)
	{
		if (title)
		{
			GUI.color = PWColorPalette.GetColor(color);
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
		currentGraph.selectorScrollPosition = EditorGUILayout.BeginScrollView(currentGraph.selectorScrollPosition, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginVertical(panelBackgroundStyle);
			{
				EditorGUIUtility.labelWidth = 0;
				EditorGUIUtility.fieldWidth = 0;
				GUILayout.BeginHorizontal(toolbarStyle);
				{
					currentGraph.searchString = GUILayout.TextField(currentGraph.searchString, toolbarSearchTextStyle);
					if (GUILayout.Button("", toolbarSearchCancelButtonStyle))
					{
						// Remove focus if cleared
						currentGraph.searchString = "";
						GUI.FocusControl(null);
					}
				}
				GUILayout.EndHorizontal();
				
				foreach (var nodeCategory in nodeSelectorList)
				{
					DrawSelectorCase(nodeCategory.Key, nodeCategory.Value.color, true);
					foreach (var nodeCase in nodeCategory.Value.nodes.Where(n => n.name.IndexOf(currentGraph.searchString, System.StringComparison.OrdinalIgnoreCase) >= 0))
					{
						Rect clickableRect = DrawSelectorCase(nodeCase.name, nodeCategory.Value.color);
	
						if (Event.current.type == EventType.MouseDown && clickableRect.Contains(Event.current.mousePosition))
							CreateNewNode(nodeCase.nodeType, -currentGraph.graphDecalPosition + position.size / 2, true);
					}
				}
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndScrollView();
	}

}
