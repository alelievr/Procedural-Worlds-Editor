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
	[System.Serializable]
	public class PWGraphSettingsPanel : PWGraphPanel
	{
		//Graph reference:
		PWGraph					graph;

		//Settings bar datas:
		Vector2					scrollbarPosition;
		[SerializeField]
		PWTerrainPreviewPanel	terrainPreview = new PWTerrainPreviewPanel();

		public Action< Rect >	onDraw;

		readonly string			graphProcessKey = "UpdateGraphProperties";

		[SerializeField]
		PWTerrainPreviewType	previewType = PWTerrainPreviewType.TopDownPlanarView;

			// delayedChanges.BindCallback(graphProcessKey, (unused) => { graph.Process(); });

		public void LoadStyles()
		{
		}

		void DrawGraphSettings(Rect currentRect)
		{
			Event	e = Event.current;
			EditorGUILayout.Space();

			GUI.SetNextControlName("PWName");
			graph.name = EditorGUILayout.TextField("ProceduralWorld name: ", graph.name);

			EditorGUILayout.Separator();

			//No need for the moment
			/*if (GUILayout.Button("Cleanup graphs"))
			{
				PWGraph[] graphs = Resources.FindObjectsOfTypeAll< PWGraph >();

				foreach (var graph in graphs)
					if (graph.objectName.Contains("(Clone)"))
					{
						Debug.Log("destroyed graph: " + graph);
						GameObject.DestroyImmediate(graph, false);
					}
			}*/

			//reload and force reload buttons
			EditorGUILayout.BeginHorizontal();
			{
				//TODO: impement add-on like pattern
				// if (GUILayout.Button("Force reload"))
					// graph.RaiseOnForceReload();
				// if (GUILayout.Button("Force reload Once"))
					// graph.RaiseOnForceReloadOnce();
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
		
		PWTerrainPreviewType GetPreviewTypeFromTerrainType(PWGraphTerrainType terrainType)
		{
			switch (terrainType)
			{
				case PWGraphTerrainType.SideView2D:
					return PWTerrainPreviewType.SideView;
				case PWGraphTerrainType.TopDown2D:
				case PWGraphTerrainType.Planar3D:
					return PWTerrainPreviewType.TopDownPlanarView;
				// case PWGraphTerrainType.Spherical3D:
					// return PWTerrainPreviewType.TopDownSphericalView;
				// case PWGraphTerrainType.Cubic3D:
					// return PWTerrainPreviewType.TopDownCubicView;
				default:
					return PWTerrainPreviewType.TopDownPlanarView;
			}
		}
	
		public void DrawTerrainPreview(Rect currentRect)
		{
			//draw terrain preview
			EditorGUILayout.BeginVertical(GUILayout.Height(currentRect.width));
			{
				Rect previewRect = EditorGUILayout.GetControlRect(false, currentRect.width);

				terrainPreview.DrawTerrainPreview(previewRect, previewType);
			}
			EditorGUILayout.EndVertical();
		}

		public override void Draw(Rect rect)
		{
			Profiler.BeginSample("[PW] Rendering settings bar");

			GUI.DrawTexture(rect, PWColorTheme.defaultBackgroundTexture);
	
			//add the texturePreviewRect size:
			scrollbarPosition = EditorGUILayout.BeginScrollView(scrollbarPosition, GUILayout.ExpandWidth(true));
			{
				base.Draw(rect);
			}
			EditorGUILayout.EndScrollView();
			
			//free focus of the selected fields
			if (Event.current.type == EventType.MouseDown)
				GUI.FocusControl(null);

			Profiler.EndSample();
		}

		public override void DrawDefault(Rect currentRect)
		{
			DrawTerrainPreview(currentRect);

			EditorGUILayout.BeginHorizontal();
			{
				previewType = (PWTerrainPreviewType)EditorGUILayout.EnumPopup("Camera mode", previewType);
			}
			EditorGUILayout.EndHorizontal();
			
			//draw main graph settings
			EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			{
				DrawGraphSettings(currentRect);
			}
			EditorGUILayout.EndVertical();
		}
	}
}