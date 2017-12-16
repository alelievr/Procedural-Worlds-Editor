using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Editor
{
	public class PWGraphSettingsBar
	{
		//Graph reference:
		PWGraph		graph;

		//Settings bar datas:
		Vector2		scrollbarPosition;
		
		//Style datas:
		Texture2D	defaultBackgroundTexture;
		GUIStyle	prefixLabelStyle;

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
			
			GUI.DrawTexture(currentRect, defaultBackgroundTexture);
	
			//add the texturepreviewRect size:
			Rect previewRect = new Rect(0, 0, currentRect.width, currentRect.width);
			scrollbarPosition = EditorGUILayout.BeginScrollView(scrollbarPosition, GUILayout.ExpandWidth(true));
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Height(currentRect.width), GUILayout.ExpandHeight(true));
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginVertical(GUILayout.Height(currentRect.height - currentRect.width - 4), GUILayout.ExpandWidth(true));
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
	
					EditorGUILayout.Separator();

					//TODO: draw graph child specific GUI
	
					mainGraph.geologicTerrainStep = graph.PWGUI.Slider("Geological terrain step: ", mainGraph.geologicTerrainStep, 4, 64);
					mainGraph.geologicDistanceCheck = graph.PWGUI.IntSlider("Geological search distance: ", mainGraph.geologicDistanceCheck, 1, 4);
	
					EditorGUILayout.Separator();
	
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
			}
			EditorGUILayout.EndScrollView();
			
			//free focus of the selected fields
			if (e.type == EventType.MouseDown)
				GUI.FocusControl(null);
		}
	}
}