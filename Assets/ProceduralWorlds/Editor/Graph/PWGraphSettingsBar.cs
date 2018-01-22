using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
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

		DelayedChanges			delayedChanges = new DelayedChanges();

		public Action< Rect >	onDrawAdditionalSettings;

		readonly string			graphProcessKey = "UpdateGraphProperties";

		public PWGraphSettingsBar(PWGraph graph)
		{
			this.graph = graph;
			delayedChanges.BindCallback(graphProcessKey, (unused) => { graph.Process(); Debug.Log("graph chunk size: " + graph.chunkSize); });
		}

		public void LoadStyles()
		{
			prefixLabelStyle = new GUIStyle("PrefixLabel");
		}

		void DrawGraphSettings(Rect currentRect, Event e)
		{
			EditorGUILayout.Space();

			GUI.SetNextControlName("PWName");
			graph.name = EditorGUILayout.TextField("ProceduralWorld name: ", graph.name);

			EditorGUI.BeginChangeCheck();
			{
				//seed
				GUI.SetNextControlName("seed");
				graph.seed = EditorGUILayout.IntField("Seed", graph.seed);
				
				//chunk size:
				GUI.SetNextControlName("chunk size");
				graph.chunkSize = EditorGUILayout.IntField("Chunk size", graph.chunkSize);
				graph.chunkSize = Mathf.Clamp(graph.chunkSize, 1, 1024);
	
				//step:
				float min = 0.1f;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("step", prefixLabelStyle);
				graph.step = graph.PWGUI.Slider(graph.step, ref min, ref graph.maxStep, 0.01f, false, true);
				EditorGUILayout.EndHorizontal();
			}
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(graphProcessKey);

			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Is real mode: " + graph.IsRealMode());
			EditorGUILayout.LabelField("Instance ID: " + graph.GetInstanceID());

			EditorGUILayout.Separator();

			if (GUILayout.Button("Cleanup graphs"))
			{
				PWGraph[] graphs = Resources.FindObjectsOfTypeAll< PWGraph >();

				foreach (var graph in graphs)
					if (graph.objectName.Contains("(Clone)"))
					{
						Debug.Log("destroyed graph: " + graph);
						GameObject.DestroyImmediate(graph, false);
					}
			}

			//reload and force reload buttons
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Force reload"))
					graph.RaiseOnForceReload();
				if (GUILayout.Button("Force reload Once"))
					graph.RaiseOnForceReloadOnce();
			}
			EditorGUILayout.EndHorizontal();
			
			//unfocus all fields if we click outsize of the settings bar
			if ((e.type == EventType.MouseDown || e.type == EventType.Ignore)
				&& !GUILayoutUtility.GetLastRect().Contains(e.mousePosition)
				&& GUI.GetNameOfFocusedControl() == "PWName")
				GUI.FocusControl(null);

			//update the delayed changes
			delayedChanges.Update();
		}

		public void DrawTerrainPreview(Rect currentRect)
		{
			//draw terrain preview
			EditorGUILayout.BeginVertical(GUILayout.Height(currentRect.width));
			{
				Rect previewRect = EditorGUILayout.GetControlRect(false, currentRect.width);

				//TODO: way to specify camera type info the graph
				terrainPreview.DrawTerrainPreview(previewRect, PWGraphTerrainPreviewType.TopDownPlanarView);
			}
			EditorGUILayout.EndVertical();
		}

		public void DrawSettingsBar(Rect currentRect)
		{
			Event	e = Event.current;

			Profiler.BeginSample("[PW] Rendering settings bar");
			
			GUI.DrawTexture(currentRect, PWColorTheme.defaultBackgroundTexture);
	
			//add the texturePreviewRect size:
			scrollbarPosition = EditorGUILayout.BeginScrollView(scrollbarPosition, GUILayout.ExpandWidth(true));
			{
				DrawTerrainPreview(currentRect);
				
				//draw main graph settings
				EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
				{
					DrawGraphSettings(currentRect, e);
				}
				EditorGUILayout.EndVertical();

				//call the method to draw additional things determined by the graph.
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

			Profiler.EndSample();
		}
	}
}