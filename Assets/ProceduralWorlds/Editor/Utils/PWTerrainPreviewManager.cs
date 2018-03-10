using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using PW.Core;
using PW;

using Object = UnityEngine.Object;

namespace PW.Editor
{
	[InitializeOnLoad]
	public class PWTerrainPreviewManager
	{
		public static PWTerrainPreviewManager instance;

		public GameObject			previewRoot { get; private set; }
		public Camera				previewCamera {get; private set; }
		public RenderTexture		previewTexture { get; private set; }
		public PWTerrainGenericBase	terrainBase { get; private set; }
		
		Dictionary< MaterializerType, Type > materializerTypes = new Dictionary< MaterializerType, Type >()
		{
			{MaterializerType.SquareTileMap, typeof(PWTopDown2DTerrainSquare)},
		};
	
		static PWTerrainPreviewManager()
		{
			instance = new PWTerrainPreviewManager();
		}

		//Private constructor so we can only use this class using it's instance
		private PWTerrainPreviewManager()
		{
			UpdateObjects();
			EditorApplication.playModeStateChanged += PlayModeChanged;
		}

		void PlayModeChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.EnteredEditMode)
				UpdateObjects();
		}

		void UpdateObjects()
		{
			var rootPreview = GameObject.FindObjectOfType< PWPreviewTerrainRoot >();

			if (rootPreview == null)
				return ;
			
			previewCamera = rootPreview.GetComponentInChildren< Camera >();

			if (previewTexture == null)
			{
				previewTexture = new RenderTexture(1000, 1000, 0, RenderTextureFormat.ARGB32);
				previewTexture.hideFlags = HideFlags.HideAndDontSave;
			}
			previewCamera.targetTexture = previewTexture;

			terrainBase = GameObject.FindObjectOfType< PWTerrainGenericBase >();

			//Store chunks into memory
			if (terrainBase != null && terrainBase.terrainStorage == null)
				terrainBase.terrainStorage = Resources.Load< PWTerrainStorage >(PWConstants.memoryTerrainStorageAsset);
		}

		public void UpdatePreviewPrefab(string newPrefabName)
		{
			var roots = GameObject.FindObjectsOfType< PWPreviewTerrainRoot >();
			for (int i = 0; i < roots.Length; i++)
				GameObject.DestroyImmediate(roots[i].gameObject);
			
			previewRoot = PrefabUtility.InstantiatePrefab(Resources.Load< Object >(newPrefabName)) as GameObject;
			previewRoot.name = newPrefabName;

			//Instantiate the resource file
			UpdateObjects();
		}

		public void UpdateTerrainMaterializer(MaterializerType materializerType)
		{
			if (terrainBase == null)
				return ;
			
			terrainBase.DestroyAllChunks();
			var go = terrainBase.gameObject;
			GameObject.DestroyImmediate(terrainBase);
			terrainBase = go.AddComponent(materializerTypes[materializerType]) as PWTerrainGenericBase;
		}

		public void UpdateChunkLoaderPosition(Vector3 position)
		{
			if (terrainBase != null)
				terrainBase.transform.position = position;
		}

		~PWTerrainPreviewManager()
		{
			if (previewTexture != null)
				GameObject.DestroyImmediate(previewTexture);
		}
	}
}