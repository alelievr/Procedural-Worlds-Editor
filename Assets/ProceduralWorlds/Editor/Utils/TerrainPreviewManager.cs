using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using ProceduralWorlds.Core;
using ProceduralWorlds;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

namespace ProceduralWorlds.Editor
{
	[InitializeOnLoad]
	public class TerrainPreviewManager
	{
		public static TerrainPreviewManager instance;

		public static readonly bool	instantiatePreviewAsPrefab = false;
		
		static readonly string		memoryTerrainStorageAsset = "memoryTerrainStorage";

		public GameObject			previewRoot { get; private set; }
		public Camera				previewCamera {get; private set; }
		public RenderTexture		previewTexture { get; private set; }
		public TerrainGenericBase	terrainBase { get; private set; }
		
		Dictionary< MaterializerType, Type > materializerTypes = new Dictionary< MaterializerType, Type >()
		{
			{MaterializerType.SquareTileMap, typeof(TopDown2DTerrainSquare)},
		};
	
		static TerrainPreviewManager()
		{
			instance = new TerrainPreviewManager();
		}

		//Private constructor so we can only use this class using it's instance
		private TerrainPreviewManager()
		{
			UpdateSceneObjects();
			EditorApplication.playModeStateChanged += PlayModeChanged;
			EditorSceneManager.sceneOpened += SceneOpenedCallback;
		}

		void PlayModeChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.EnteredEditMode)
				UpdateSceneObjects();
		}

		void SceneOpenedCallback(Scene scene, OpenSceneMode mode)
		{
			UpdateSceneObjects();
		}

		public void UpdateSceneObjects()
		{
			var rootPreview = GameObject.FindObjectOfType< PreviewTerrainRoot >();

			if (rootPreview == null)
				return ;
			
			previewCamera = rootPreview.GetComponentInChildren< Camera >();

			if (previewTexture == null)
			{
				previewTexture = new RenderTexture(1000, 1000, 0, RenderTextureFormat.ARGB32);
				previewTexture.hideFlags = HideFlags.HideAndDontSave;
			}
			if (previewCamera != null)
				previewCamera.targetTexture = previewTexture;

			terrainBase = GameObject.FindObjectOfType< TerrainGenericBase >();

			//Store chunks into memory
			if (terrainBase != null && terrainBase.terrainStorage == null)
				terrainBase.terrainStorage = Resources.Load< TerrainStorage >(memoryTerrainStorageAsset);
		}

		public void UpdatePreviewPrefab(string newPrefabName)
		{
			var roots = GameObject.FindObjectsOfType< PreviewTerrainRoot >();
			for (int i = 0; i < roots.Length; i++)
				GameObject.DestroyImmediate(roots[i].gameObject);
			
			if (instantiatePreviewAsPrefab)
				previewRoot = PrefabUtility.InstantiatePrefab(Resources.Load< Object >(newPrefabName)) as GameObject;
			else
				previewRoot = GameObject.Instantiate(Resources.Load< GameObject >(newPrefabName));
			previewRoot.name = newPrefabName;

			//Instantiate the resource file
			UpdateSceneObjects();
		}

		public void UpdateTerrainMaterializer(MaterializerType materializerType)
		{
			if (terrainBase == null)
				return ;
			
			terrainBase.DestroyAllChunks();
			var go = terrainBase.gameObject;
			GameObject.DestroyImmediate(terrainBase);
			terrainBase = go.AddComponent(materializerTypes[materializerType]) as TerrainGenericBase;
		}

		public void UpdateChunkLoaderPosition(Vector3 position)
		{
			if (terrainBase != null)
			{
				terrainBase.transform.position = position;
				terrainBase.UpdateChunks();
			}
		}

		~TerrainPreviewManager()
		{
			if (previewTexture != null)
				GameObject.DestroyImmediate(previewTexture);
		}
	}
}