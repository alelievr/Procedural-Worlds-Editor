using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds;
using ProceduralWorlds.Core;
using ProceduralWorlds.IsoSurfaces;
using System;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(GenericBaseTerrain))]
	public abstract class BaseTerrainInspector : UnityEditor.Editor
	{
		GenericBaseTerrain baseTerrain;

		public void OnEnable()
		{
			baseTerrain = target as GenericBaseTerrain;
			OnEditorEnable();
		}

		public override void OnInspectorGUI()
		{
			baseTerrain.debug = EditorGUILayout.Toggle("Debug", baseTerrain.debug);
			baseTerrain.generateChunksOnLoad = EditorGUILayout.Toggle("Generate on load", baseTerrain.generateChunksOnLoad);

			EditorGUILayout.Space();

			baseTerrain.renderDistance = EditorGUILayout.IntSlider("Render distance", baseTerrain.renderDistance, 0, 24);
			baseTerrain.loadPatternMode = (ChunkLoadPatternMode)EditorGUILayout.EnumPopup("Load pattern mode", baseTerrain.loadPatternMode);
			baseTerrain.terrainStorage = EditorGUILayout.ObjectField("Chunk storage", baseTerrain.terrainStorage, typeof(TerrainStorage), false) as TerrainStorage;
			baseTerrain.terrainScale = EditorGUILayout.Slider("Terrain scale", baseTerrain.terrainScale, 0.01f, 10f);
			baseTerrain.graphAsset = EditorGUILayout.ObjectField("World Graph", baseTerrain.graphAsset, typeof(WorldGraph), false) as WorldGraph;
			
			EditorGUILayout.Space();

			OnEditorGUI();
			
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Generate terrain"))
					ReloadChunks();
				if (GUILayout.Button("Cleanup terrain"))
					baseTerrain.DestroyAllChunks();
			}
			EditorGUILayout.EndHorizontal();
		}
		
		//Warning: this will destroy all loaded chunks and regenerate them
		public void ReloadChunks()
		{
			if (EditorApplication.isPlaying || EditorApplication.isPaused)
			{
				Debug.LogError("[ChunkLoader] can't reload chunks in play mode");
				return ;
			}

			if (baseTerrain.graph == null || baseTerrain.terrainStorage == null)
			{
				Debug.LogError("[ChunkLoader] World graph or terrain storage is null in terrain materializer");
				return ;
			}

			try {
				baseTerrain.ReloadChunks(baseTerrain.graphAsset);
			} catch (Exception e) {
				Debug.LogError(e);
			}
		}

		public void OnSceneGUI()
		{
			OnEditorSceneGUI();
		}

		public abstract void OnEditorGUI();
		public abstract void OnEditorEnable();
		public virtual void OnEditorSceneGUI() {}
	}
}