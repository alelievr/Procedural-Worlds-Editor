using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;
using ProceduralWorlds.Core;
using System;
using Object = UnityEngine.Object;

namespace ProceduralWorlds.Editor
{
	[System.Serializable]
	public class TerrainPreviewDrawer : PWDrawer
	{
		[SerializeField]
		TerrainPreviewType	loadedPreviewType;

		bool					previewMouseDrag;
		float					cameraMoveDirection;

		Texture2D				recenterIcon;

		[System.NonSerialized]
		bool					first = true;

		BaseGraph					graphRef;

		Dictionary< TerrainPreviewType, string > previewTypeToPrefabNames = new Dictionary< TerrainPreviewType, string >()
		{
			{TerrainPreviewType.TopDownPlanarView, PWConstants.previewTopDownPrefabName},
			{TerrainPreviewType.SideView, PWConstants.previewSideViewPrefabName},
		};
		
		public override void OnEnable()
		{
			graphRef = target as BaseGraph;

			PWTerrainPreviewManager.instance.UpdatePreviewPrefab(previewTypeToPrefabNames[graphRef.terrainPreviewType]);

			recenterIcon = Resources.Load< Texture2D >("Icons/ic_recenter");
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

				graphRef.terrainPreviewType = (TerrainPreviewType)EditorGUILayout.EnumPopup("Camera mode", graphRef.terrainPreviewType);
			}
			EditorGUILayout.EndVertical();
		}

		public void DrawTerrainPreview(Rect previewRect)
		{
			Profiler.BeginSample("[PW] Rendering terrain preview");

			UpdatePreviewObjects();

			Camera previewCamera = PWTerrainPreviewManager.instance.previewCamera;

			if (previewCamera == null)
			{
				DisplayLoadCameraButton();
				return ;
			}

			if (previewCamera != null && first)
				previewCamera.Render();

			//draw preview texture:
			var renderTexture = PWTerrainPreviewManager.instance.previewTexture;
			if (renderTexture != null)
				GUI.DrawTexture(previewRect, renderTexture);

			DrawIcons(previewRect, previewCamera);

			switch (loadedPreviewType)
			{
				case TerrainPreviewType.SideView:
				case TerrainPreviewType.TopDownPlanarView:
					TopDownCameraControls(previewRect, previewCamera);
					break ;
				default:
					break ;
			}
			
			//move the terrain materializer so it generate terrain around the camera
			PWTerrainPreviewManager.instance.UpdateChunkLoaderPosition(previewCamera.transform.position);

			first = false;

			Profiler.EndSample();
		}

		void DisplayLoadCameraButton()
		{
			EditorGUILayout.LabelField("TODO: preview load button");
		}

		void TopDownCameraControls(Rect previewRect, Camera previewCamera)
		{
			//activate drag when mouse down inside preview rect:
			if (previewRect.Contains(e.mousePosition) && e.type == EventType.MouseDown)
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

		void DrawIcons(Rect previewRect, Camera previewCamera)
		{
			//Recenter icon:
			Rect recenterIconRect = previewRect;
			recenterIconRect.position += new Vector2(4, 6);
			recenterIconRect.size = new Vector2(15, 15);
			GUI.DrawTexture(recenterIconRect, recenterIcon);

			if (recenterIconRect.Contains(e.mousePosition))
				previewCamera.transform.position = Vector3.zero;
		}
	}
}