using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//Left info bar + current selected node Info
public partial class PWMainGraphEditor {

	void DrawLeftBar(Rect currentRect)
	{
		Event	e = Event.current;
		GUI.DrawTexture(currentRect, defaultBackgroundTexture);

		//add the texturepreviewRect size:
		Rect previewRect = new Rect(0, 0, currentRect.width, currentRect.width);
		mainGraph.leftBarScrollPosition = EditorGUILayout.BeginScrollView(mainGraph.leftBarScrollPosition, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(currentRect.width), GUILayout.ExpandHeight(true));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Height(currentRect.height - currentRect.width - 4), GUILayout.ExpandWidth(true));
			{
				EditorGUILayout.Space();

				chunkRenderDistance = EditorGUILayout.IntSlider("chunk Render distance", chunkRenderDistance, 0, 24);
				terrainMaterializer.renderDistance = chunkRenderDistance;

				if (mainGraph == null)
					OnEnable();

				GUI.SetNextControlName("PWName");
				mainGraph.name = EditorGUILayout.TextField("ProceduralWorld name: ", mainGraph.name);

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
				graph.seed = EditorGUILayout.IntField("Seed", graph.seed);
				
				//chunk size:
				EditorGUI.BeginChangeCheck();
				GUI.SetNextControlName("chunk size");
				graph.chunkSize = EditorGUILayout.IntField("Chunk size", graph.chunkSize);
				graph.chunkSize = Mathf.Clamp(graph.chunkSize, 1, 1024);
				
				terrainMaterializer.chunkSize = graph.chunkSize;

				//step:
				EditorGUI.BeginChangeCheck();
				float min = 0.1f;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("step", prefixLabelStyle);
				graph.step = graph.PWGUI.Slider(graph.step, ref min, ref graph.maxStep, 0.01f, false, true);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Separator();

				mainGraph.geologicTerrainStep = graph.PWGUI.Slider("Geological terrain step: ", mainGraph.geologicTerrainStep, 4, 64);
				mainGraph.geologicDistanceCheck = graph.PWGUI.IntSlider("Geological search distance: ", mainGraph.geologicDistanceCheck, 1, 4);

				EditorGUILayout.Separator();

				EditorGUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("Force reload"))
						mainGraph.RaiseOnForceReload();
					if (GUILayout.Button("Force reload Once"))
						mainGraph.RaiseOnForceReloadOnce();
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
