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
	public class TerrainPreviewDrawer : Drawer
	{
		TerrainPreviewType		loadedPreviewType;

		bool					previewMouseDrag;
		float					cameraMoveDirection;

		Texture2D				recenterIcon;

		[System.NonSerialized]
		bool					first = true;

		BaseGraph				graphRef;

		readonly Dictionary< TerrainPreviewType, string > previewTypeToPrefabNames = new Dictionary< TerrainPreviewType, string >
		{
			{TerrainPreviewType.TopDownPlanarView, "PWPreviewTopDown2D"},
			{TerrainPreviewType.SideView, "PWPreviewSideView2D"},
		};
		
		public override void OnEnable()
		{
			graphRef = target as BaseGraph;

			loadedPreviewType = graphRef.terrainPreviewType;

			TerrainPreviewManager.instance.UpdateSceneObjects();

			recenterIcon = Resources.Load< Texture2D >("Icons/ic_recenter");
		}

		void UpdatePreviewObjects()
		{
			//if preview scene was destroyed or preview type was changed, reload preview game objects
			if (loadedPreviewType != graphRef.terrainPreviewType)
			{
				TerrainPreviewManager.instance.UpdatePreviewPrefab(previewTypeToPrefabNames[graphRef.terrainPreviewType]);
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

			Camera previewCamera = TerrainPreviewManager.instance.previewCamera;

			if (previewCamera == null)
			{
				DisplayLoadCameraButton(previewRect);
				return ;
			}

			if (previewCamera != null && first)
				previewCamera.Render();

			//draw preview texture:
			var renderTexture = TerrainPreviewManager.instance.previewTexture;
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
			TerrainPreviewManager.instance.UpdateChunkLoaderPosition(previewCamera.transform.position);

			first = false;

			Profiler.EndSample();
		}

		void DisplayLoadCameraButton(Rect cameraRect)
		{
			Rect buttonRect = cameraRect;
			buttonRect.height = EditorGUIUtility.singleLineHeight * 2;

			if (GUI.Button(buttonRect, "Load preview"))
				TerrainPreviewManager.instance.UpdatePreviewPrefab(previewTypeToPrefabNames[graphRef.terrainPreviewType]);
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

			Vector3 pos = previewCamera.transform.position;
			if (recenterIconRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
			{
				switch (graphRef.terrainPreviewType)
				{
					case TerrainPreviewType.TopDownPlanarView:
						previewCamera.transform.position = new Vector3(0, pos.y, 0);
						break ;
					case TerrainPreviewType.SideView:
						previewCamera.transform.position = new Vector3(0, 0, pos.z);
						break ;
				}
			}
		}
	}
}