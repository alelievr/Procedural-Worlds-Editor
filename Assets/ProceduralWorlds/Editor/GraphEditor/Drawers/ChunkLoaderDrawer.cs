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

		public override void OnEnable()
		{
			worldGraph =  target as WorldGraph;

			var terrain = TerrainPreviewManager.instance.terrainBase;

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

			if (terrainEditor == null)
			{
				terrainEditor = UnityEditor.Editor.CreateEditor(terrain);
			}
			
			EditorGUI.BeginChangeCheck();
			{
				terrainEditor.OnInspectorGUI();
				// terrain.renderDistance = EditorGUILayout.IntSlider("chunk Render distance", terrain.renderDistance, 0, 24);
				// terrain.terrainScale = EditorGUILayout.Slider("Scale", terrain.terrainScale, 0.01f, 10);
				// terrain.loadPatternMode = (ChunkLoadPatternMode)EditorGUILayout.EnumPopup("Load pattern mode", terrain.loadPatternMode);
			}
			if (EditorGUI.EndChangeCheck())
				ReloadChunks(terrain);

			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Generate terrain"))
					ReloadChunks(terrain);
				if (GUILayout.Button("Cleanup terrain"))
					terrain.DestroyAllChunks();
			}
			EditorGUILayout.EndHorizontal();
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
				terrain.ReloadChunks(worldGraph);
		}
	}
}