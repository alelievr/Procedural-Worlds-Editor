using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;
using PW.Core;
using System;
using Object = UnityEngine.Object;

namespace PW.Editor
{
	[System.Serializable]
	public class TerrainPreviewDrawer : PWDrawer
	{
		[SerializeField]
		PWTerrainPreviewType	loadedPreviewType;

		bool					previewMouseDrag;
		float					cameraMoveDirection;

		[System.NonSerialized]
		bool					first = true;

		PWGraph					graphRef;

		Dictionary< PWTerrainPreviewType, string > previewTypeToPrefabNames = new Dictionary< PWTerrainPreviewType, string >()
		{
			{PWTerrainPreviewType.TopDownPlanarView, PWConstants.previewTopDownPrefabName},
			{PWTerrainPreviewType.SideView, PWConstants.previewSideViewPrefabName},
		};
		
		public override void OnEnable()
		{
			graphRef = target as PWGraph;

			PWTerrainPreviewManager.instance.UpdatePreviewPrefab(previewTypeToPrefabNames[graphRef.terrainPreviewType]);
		}

		void UpdatePreviewObjects()
		{
			//if preview scene was destroyed or preview type was changed, reload preview game objects
			if (loadedPreviewType != graphRef.terrainPreviewType)
			{
				PWTerrainPreviewManager.instance.UpdatePreviewPrefab(previewTypeToPrefabNames[graphRef.terrainPreviewType]);
				loadedPreviewType = graphRef.terrainPreviewType;
			}
		}

		new public void OnGUI(Rect rect)
		{
			base.OnGUI(rect);

			EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			{
				Rect previewRect = EditorGUILayout.GetControlRect(false, rect.width);

				DrawTerrainPreview(previewRect);

				graphRef.terrainPreviewType = (PWTerrainPreviewType)EditorGUILayout.EnumPopup("Camera mode", graphRef.terrainPreviewType);
			}
			EditorGUILayout.EndVertical();
		}

		public void DrawTerrainPreview(Rect previewRect)
		{
			Profiler.BeginSample("[PW] Rendering terrain preview");

			UpdatePreviewObjects();

			Camera previewCamera = PWTerrainPreviewManager.instance.previewCamera;

			if (previewCamera != null && first)
				previewCamera.Render();

			//draw preview texture:
			var renderTexture = PWTerrainPreviewManager.instance.previewTexture;
			if (renderTexture != null)
				GUI.DrawTexture(previewRect, renderTexture);

			if (previewRect.Contains(e.mousePosition))
			{
				switch (loadedPreviewType)
				{
					case PWTerrainPreviewType.SideView:
					case PWTerrainPreviewType.TopDownPlanarView:
						TopDownCameraControls(previewCamera);
						break ;
					default:
						break ;
				}
				
				//move the terrain materializer so it generate terrain around the camera
				PWTerrainPreviewManager.instance.UpdateChunkLoaderPosition(previewCamera.transform.position);
			}

			first = false;

			Profiler.EndSample();
		}

		void TopDownCameraControls(Camera previewCamera)
		{
			//activate drag when mouse down inside preview rect:
			if (e.type == EventType.MouseDown)
			{
				previewMouseDrag = true;
				e.Use();
			}

			if (previewMouseDrag && e.rawType == EventType.MouseUp)
				previewMouseDrag = false;

			//mouse controls:
			if (e.type == EventType.MouseDrag && previewMouseDrag)
			{
				Vector2 move = new Vector2(-e.delta.x / 8, e.delta.y / 8);

				//camera pan movement
				previewCamera.transform.position += new Vector3(move.x, 0, move.y);

				e.Use();
			}
		}
	}
}