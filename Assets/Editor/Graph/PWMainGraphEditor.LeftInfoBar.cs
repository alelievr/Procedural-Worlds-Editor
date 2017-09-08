using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Left info bar + current selected node Info
public partial class PWMainGraphEditor {

	void DrawLeftBar(Rect currentRect)
	{
		Event	e = Event.current;
		GUI.DrawTexture(currentRect, defaultBackgroundTexture);

		//add the texturepreviewRect size:
		Rect previewRect = new Rect(0, 0, currentRect.width, currentRect.width);
		currentGraph.leftBarScrollPosition = EditorGUILayout.BeginScrollView(currentGraph.leftBarScrollPosition, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(currentRect.width), GUILayout.ExpandHeight(true));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Height(currentRect.height - currentRect.width - 4), GUILayout.ExpandWidth(true));
			{
				EditorGUILayout.Space();

				chunkRenderDistance = EditorGUILayout.IntSlider("chunk Render distance", chunkRenderDistance, 0, 24);
				terrainMaterializer.renderDistance = chunkRenderDistance;

				if (currentGraph == null)
					OnEnable();

				GUI.SetNextControlName("PWName");
				currentGraph.externalName = EditorGUILayout.TextField("ProceduralWorld name: ", currentGraph.externalName);

				if ((e.type == EventType.MouseDown || e.type == EventType.Ignore)
					&& !GUILayoutUtility.GetLastRect().Contains(e.mousePosition)
					&& GUI.GetNameOfFocusedControl() == "PWName")
					GUI.FocusControl(null);

				//preview texture:
				GUI.DrawTexture(previewRect, previewCameraRenderTexture);

				//preview controls:
				if (e.type == EventType.MouseDown && previewRect.Contains(e.mousePosition))
					previewMouseDrag = true;

				if (e.type == EventType.MouseDrag && previewMouseDrag)
				{
					//mouse controls:
					e.Use();
					MovePreviewCamera(new Vector2(-e.delta.x / 8, e.delta.y / 8));
				}

				//seed
				EditorGUI.BeginChangeCheck();
				GUI.SetNextControlName("seed");
				parentGraph.seed = EditorGUILayout.IntField("Seed", parentGraph.seed);
				if (EditorGUI.EndChangeCheck())
				{
					parentGraph.UpdateSeed(parentGraph.seed);
					graphNeedReload = true;
				}
				
				//chunk size:
				EditorGUI.BeginChangeCheck();
				GUI.SetNextControlName("chunk size");
				parentGraph.chunkSize = EditorGUILayout.IntField("Chunk size", parentGraph.chunkSize);
				parentGraph.chunkSize = Mathf.Clamp(parentGraph.chunkSize, 1, 1024);
				if (EditorGUI.EndChangeCheck())
				{
					parentGraph.UpdateChunkSize(parentGraph.chunkSize);
					graphNeedReload = true;
				}
				terrainMaterializer.chunkSize = parentGraph.chunkSize;

				//step:
				EditorGUI.BeginChangeCheck();
				float min = 0.1f;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("step", prefixLabelStyle);
				parentGraph.PWGUI.Slider(ref parentGraph.step, ref min, ref parentGraph.maxStep, 0.01f, false, true);
				EditorGUILayout.EndHorizontal();
				if (EditorGUI.EndChangeCheck())
				{
					parentGraph.UpdateStep(parentGraph.step);
					graphNeedReload = true;
				}

				EditorGUILayout.Separator();

				EditorGUI.BeginChangeCheck();
				parentGraph.PWGUI.Slider("Geological terrain step: ", ref parentGraph.geologicTerrainStep, 4, 64);
				parentGraph.PWGUI.IntSlider("Geological search distance: ", ref parentGraph.geologicDistanceCheck, 1, 4);
				if (EditorGUI.EndChangeCheck())
					graphNeedReload = true;

				EditorGUILayout.Separator();

				EditorGUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("Force reload"))
					{
						parentGraph.ForeachAllNodes(n => n.forceReload = true, true, true);
						graphNeedReload = true;
						EvaluateComputeOrder();
						Debug.Log("graph reloaded !");
					}
					if (GUILayout.Button("Force reload Once"))
					{
						parentGraph.ForeachAllNodes(n => n.forceReload = true, true, true);
						graphNeedReload = true;
						graphNeedReloadOnce = true;
						EvaluateComputeOrder();
						Debug.Log("graph fully reloaded !");
					}
				}
				EditorGUILayout.EndHorizontal();

			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndScrollView();
		
		//free focus of the selected fields
		if (e.type == EventType.MouseDown)
			GUI.FocusControl(null);
	}
	
}
