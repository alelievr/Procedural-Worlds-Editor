using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using PW.Core;
using PW;

namespace PW.Editor
{
	public class PWGraphSettingsBar
	{
		//Graph reference:
		PWGraph					graph;

		//Settings bar datas:
		Vector2					scrollbarPosition;
		[SerializeField]
		PWGraphTerrainPreview	terrainPreview = new PWGraphTerrainPreview();
		
		//Style datas:
		GUIStyle				prefixLabelStyle;

		public Action< Rect >	onDrawAdditionalSettings;

		public PWGraphSettingsBar(PWGraph graph)
		{
			this.graph = graph;
		}

		public void LoadStyles()
		{
			prefixLabelStyle = new GUIStyle("PrefixLabel");
		}

		public void DrawSettingsBar(Rect currentRect)
		{
			Event	e = Event.current;
			
			GUI.DrawTexture(currentRect, PWColorTheme.defaultBackgroundTexture);
	
			//add the texturePreviewRect size:
			scrollbarPosition = EditorGUILayout.BeginScrollView(scrollbarPosition, GUILayout.ExpandWidth(true));
			{
				EditorGUILayout.BeginVertical(GUILayout.Height(currentRect.width));
				{
					Rect previewRect = EditorGUILayout.GetControlRect(false, currentRect.width);
	
					//TODO: way to specify camera type info the graph
					terrainPreview.DrawTerrainPreview(previewRect, PWGraphTerrainPreviewType.TopDownPlanarView);
				}
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
				{
					EditorGUILayout.Space();
	
					GUI.SetNextControlName("PWName");
					graph.name = EditorGUILayout.TextField("ProceduralWorld name: ", graph.name);
	
					if ((e.type == EventType.MouseDown || e.type == EventType.Ignore)
						&& !GUILayoutUtility.GetLastRect().Contains(e.mousePosition)
						&& GUI.GetNameOfFocusedControl() == "PWName")
						GUI.FocusControl(null);
	
					//seed
					EditorGUI.BeginChangeCheck();
					GUI.SetNextControlName("seed");
					graph.seed = EditorGUILayout.IntField("Seed", graph.seed);
					
					//chunk size:
					EditorGUI.BeginChangeCheck();
					GUI.SetNextControlName("chunk size");
					graph.chunkSize = EditorGUILayout.IntField("Chunk size", graph.chunkSize);
					graph.chunkSize = Mathf.Clamp(graph.chunkSize, 1, 1024);
	
					//step:
					EditorGUI.BeginChangeCheck();
					float min = 0.1f;
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("step", prefixLabelStyle);
					graph.step = graph.PWGUI.Slider(graph.step, ref min, ref graph.maxStep, 0.01f, false, true);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.LabelField("Is real mode: " + graph.IsRealMode());
					EditorGUILayout.LabelField("Instance ID: " + graph.GetInstanceID());
	
					EditorGUILayout.Separator();
	
					EditorGUILayout.Separator();

					if (GUILayout.Button("Cleaup graphs"))
					{
						PWGraph[] graphs = Resources.FindObjectsOfTypeAll< PWGraph >();

						foreach (var graph in graphs)
							if (graph.objectName.Contains("(Clone)"))
							{
								Debug.Log("destroyed graph: " + graph);
								GameObject.DestroyImmediate(graph, false);
							}
					}
	
					EditorGUILayout.BeginHorizontal();
					{
						if (GUILayout.Button("Force reload"))
							graph.RaiseOnForceReload();
						if (GUILayout.Button("Force reload Once"))
							graph.RaiseOnForceReloadOnce();
					}
					EditorGUILayout.EndHorizontal();
	
				}
				EditorGUILayout.EndVertical();

				if (onDrawAdditionalSettings != null)
				{
					Rect r = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
					{
						onDrawAdditionalSettings(r);
					}
					EditorGUILayout.EndVertical();
				}
			}
			EditorGUILayout.EndScrollView();
			
			//free focus of the selected fields
			if (e.type == EventType.MouseDown)
				GUI.FocusControl(null);
		}
	}
}