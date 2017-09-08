using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using System;

//Top Left terrain preview
public partial class PWMainGraphEditor : PWGraphEditor {
	
	//preview fields
	GameObject			previewScene;
	Camera				previewCamera;
	RenderTexture		previewCameraRenderTexture;

	GameObject GetLoadedPreviewScene(params PWGraphTerrainType[] allowedTypes)
	{
		GameObject		ret;

		Func< string, PWGraphTerrainType, GameObject >	TestSceneNametype = (string name, PWGraphTerrainType type) =>
		{
			ret = GameObject.Find(name);
			if (ret == null)
				return null;
			foreach (var at in allowedTypes)
				if (type == at)
					return ret;
			return null;
		};
		ret = TestSceneNametype(PWConstants.previewSideViewSceneName, PWGraphTerrainType.SideView2D);
		if (ret != null)
			return ret;
		ret = TestSceneNametype(PWConstants.previewTopDownSceneName, PWGraphTerrainType.TopDown2D);
		if (ret != null)
			return ret;
		ret = TestSceneNametype(PWConstants.preview3DSceneName, PWGraphTerrainType.Planar3D);
		if (ret != null)
			return ret;
		return null;
	}

	void ProcessPreviewScene(PWGraphTerrainType outputType)
	{
		if (previewScene == null)
		{
			//TODO: do the preview for Density field 1D
			switch (outputType)
			{
				case PWGraphTerrainType.Density2D:
				case PWGraphTerrainType.SideView2D:
					previewScene = GetLoadedPreviewScene(PWGraphTerrainType.Density2D, PWGraphTerrainType.SideView2D);
					if (previewScene == null)
						previewScene = Instantiate(Resources.Load(PWConstants.previewSideViewSceneName, typeof(GameObject)) as GameObject);
					previewScene.name = PWConstants.previewTopDownSceneName;
					break ;
				case PWGraphTerrainType.TopDown2D:
					previewScene = GetLoadedPreviewScene(PWGraphTerrainType.TopDown2D);
					if (previewScene == null)
						previewScene = Instantiate(Resources.Load(PWConstants.previewTopDownSceneName, typeof(GameObject)) as GameObject);
					previewScene.name = PWConstants.previewTopDownSceneName;
					break ;
				default: //for 3d previewScenes:
					previewScene = GetLoadedPreviewScene(PWGraphTerrainType.Cubic3D, PWGraphTerrainType.Density3D, PWGraphTerrainType.Planar3D, PWGraphTerrainType.Spherical3D);
					if (previewScene == null)
						previewScene = Instantiate(Resources.Load(PWConstants.preview3DSceneName, typeof(GameObject)) as GameObject);
					previewScene.name = PWConstants.preview3DSceneName;
					break ;
			}
		}

		if (previewCamera == null)
			previewCamera = previewScene.GetComponentInChildren< Camera >();
		if (previewCameraRenderTexture == null)
			previewCameraRenderTexture = new RenderTexture(800, 800, 10000, RenderTextureFormat.ARGB32);
		if (previewCamera != null && previewCameraRenderTexture != null)
			previewCamera.targetTexture = previewCameraRenderTexture;
		if (terrainMaterializer == null)
			terrainMaterializer = previewScene.GetComponentInChildren< PWTerrainBase >();
	}

	void MovePreviewCamera(Vector2 move)
	{
		previewCamera.transform.position += new Vector3(move.x, 0, move.y);
		terrainMaterializer.position = previewCamera.transform.position;
	}
}
