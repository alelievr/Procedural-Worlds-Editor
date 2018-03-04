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
	public class PWGraphSettingsPanel : PWLayoutPanel
	{
		//Settings bar datas:
		Vector2					scrollbarPosition;
		[SerializeField]
		PWTerrainPreviewPanel	terrainPreview = new PWTerrainPreviewPanel();

		const string			graphProcessKey = "UpdateGraphProperties";

		[SerializeField]
		PWTerrainPreviewType	previewType = PWTerrainPreviewType.TopDownPlanarView;

		public override void OnEnable()
		{
			delayedChanges.BindCallback(graphProcessKey, (unused) => { graphRef.Process(); });
		}

		void DrawGraphSettings(Rect currentRect)
		{
			Event	e = Event.current;
			EditorGUILayout.Space();

			GUI.SetNextControlName("PWName");
			graphRef.name = EditorGUILayout.TextField("ProceduralWorld name: ", graphRef.name);

			EditorGUILayout.Separator();

			//reload and force reload buttons
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Force reload"))
					graphEditor.Reload();
				if (GUILayout.Button("Force reload Once"))
					graphEditor.ReloadOnce();
			}
			EditorGUILayout.EndHorizontal();
			
			//unfocus all fields if we click outsize of the settings bar
			if ((e.type == EventType.MouseDown || e.type == EventType.Ignore)
				&& !GUILayoutUtility.GetLastRect().Contains(e.mousePosition)
				&& GUI.GetNameOfFocusedControl() == "PWName")
				GUI.FocusControl(null);
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
			
			//draw main graphRef settings
			EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			{
				DrawGraphSettings(currentRect);
			}
			EditorGUILayout.EndVertical();
		}
	}
}