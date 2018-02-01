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
	public enum PWGraphTerrainPreviewType
	{
		SideView,
		TopDownPlanarView,
		FreeCamera,
		// TopDownSphericalView,
		// TopDownCubicView,
	}

	[System.Serializable]
	public class PWGraphTerrainPreview
	{
		//TODO: protection for multiple graph windows opened at same time

		//preview fields
		GameObject				previewScene;
		Camera					previewCamera;
		RenderTexture			previewCameraRenderTexture;

		[SerializeField]
		PWGraphTerrainPreviewType	loadedPreviewType;

		Event					e { get { return Event.current; } }

		bool					previewMouseDrag = false;

		[System.NonSerialized]
		bool					first = true;

		Dictionary< PWGraphTerrainPreviewType, string > previewTypeToPrefabNames = new Dictionary< PWGraphTerrainPreviewType, string >()
		{
			{ PWGraphTerrainPreviewType.TopDownPlanarView, PWConstants.previewTopDownPrefabName},
			{ PWGraphTerrainPreviewType.SideView, PWConstants.previewSideViewPrefabName},
			{ PWGraphTerrainPreviewType.FreeCamera, PWConstants.previewFree3DPrefabName},
		};

		void ReloadPreviewPrefab(PWGraphTerrainPreviewType newPreviewType)
		{
			string		previewObjectName = previewTypeToPrefabNames[newPreviewType];

			//find and delete old preview object if existing
			if ((previewScene = GameObject.Find(previewTypeToPrefabNames[loadedPreviewType])) != null)
				GameObject.DestroyImmediate(previewScene);
			if ((previewScene = GameObject.Find(previewTypeToPrefabNames[newPreviewType])) != null)
				GameObject.DestroyImmediate(previewScene);
			
			//instantiate the new object prefab
			previewScene = PrefabUtility.InstantiatePrefab(Resources.Load< Object >(previewObjectName)) as GameObject;
			previewScene.name = previewObjectName;

			loadedPreviewType = newPreviewType;
		}

		void UpdatePreviewScene(PWGraphTerrainPreviewType previewType)
		{
			//if preview scene was destroyed or preview type was changed, reload preview game objects
			if (previewScene == null || loadedPreviewType != previewType)
				ReloadPreviewPrefab(previewType);
	
			if (previewCamera == null)
				previewCamera = previewScene.GetComponentInChildren< Camera >();
			if (previewCameraRenderTexture == null)
			{
				previewCameraRenderTexture = new RenderTexture(800, 800, 10000, RenderTextureFormat.ARGB32);
				previewCamera.targetTexture = previewCameraRenderTexture;
			}
		}

		public void DrawTerrainPreview(Rect previewRect, PWGraphTerrainPreviewType previewType)
		{
			Profiler.BeginSample("[PW] Rendering terrain preview");

			UpdatePreviewScene(previewType);

			if (previewCamera != null && first)
				previewCamera.Render();

			//draw preview texture:
			GUI.DrawTexture(previewRect, previewCameraRenderTexture);

			//activate drag when mouse down inside preview rect:
			if (e.type == EventType.MouseDown && previewRect.Contains(e.mousePosition))
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

				//move the terrain materializer so it generate terrain around the camera
				if (PWGraphTerrainManager.terrainReference != null)
					PWGraphTerrainManager.terrainReference.position = previewCamera.transform.position;
				e.Use();
			}

			first = false;

			Profiler.EndSample();
		}
		
	}
}