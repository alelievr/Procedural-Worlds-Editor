using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;
using System;
using ProceduralWorlds;

namespace ProceduralWorlds.Editor
{
	public class ChunkLoaderDrawer : Drawer
	{
		WorldGraph			worldGraph;

		UnityEditor.Editor	terrainEditor;

		TerrainGenericBase	oldTerrain;

		public override void OnEnable()
		{
			worldGraph =  target as WorldGraph;

			var terrain = TerrainPreviewManager.instance.terrainBase;
			oldTerrain = terrain;

			if (terrain != null)
				ReloadChunks(terrain);
		}

		new public void OnGUI(Rect r)
		{
			base.OnGUI(r);

			var terrain = TerrainPreviewManager.instance.terrainBase;

			if (terrain == null)
			{
				if (TerrainPreviewManager.instance.previewRoot == null)
					EditorGUILayout.HelpBox("You must load the preview to activate chunk generation", MessageType.Warning);
				else
					EditorGUILayout.HelpBox("Terrain materializer type not supported (" + worldGraph.terrainPreviewType + ")", MessageType.Warning);
				return ;
			}

			if (terrainEditor == null || oldTerrain != terrain)
			{
				terrainEditor = UnityEditor.Editor.CreateEditor(terrain);
			}

			terrainEditor.OnInspectorGUI();

			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Focus"))
					Selection.activeObject = terrain;
				if (GUILayout.Button("Generate terrain"))
					ReloadChunks(terrain);
				if (GUILayout.Button("Cleanup terrain"))
					terrain.DestroyAllChunks();
			}
			EditorGUILayout.EndHorizontal();

			oldTerrain = terrain;
		}

		//Warning: this will destroy all loaded chunks and regenerate them
		public void ReloadChunks(TerrainGenericBase terrain)
		{
			if (EditorApplication.isPlaying || EditorApplication.isPaused)
			{
				Debug.LogError("[ChunkLoader] can't reload chunks in play mode");
				return ;
			}

			if (worldGraph != null)
			{
				try {
					terrain.ReloadChunks(worldGraph);
				} catch (Exception e) {
					Debug.LogError(e);
				}
			}
		}
	}
}