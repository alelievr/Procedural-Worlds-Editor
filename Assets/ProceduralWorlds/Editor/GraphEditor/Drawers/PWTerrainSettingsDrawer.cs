using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using System;
using PW;

namespace PW.Editor
{
	public class PWChunkLoaderDrawer : PWDrawer
	{
		PWMainGraph		mainGraph;

		public override void OnEnable()
		{
			mainGraph =  target as PWMainGraph;
		}

		new public void OnGUI(Rect r)
		{
			base.OnGUI(r);

			var terrain = PWTerrainPreviewManager.instance.terrainBase;

			if (terrain == null)
				return ;
			
			terrain.renderDistance = EditorGUILayout.IntSlider("chunk Render distance", terrain.renderDistance, 0, 24);

			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Generate terrain"))
					ReloadChunks(terrain);
			}
			EditorGUILayout.EndHorizontal();
		}

		//Warning: this will destroy all loaded chunks and regenerate them
		public void ReloadChunks(PWTerrainGenericBase terrain)
		{
			if (EditorApplication.isPlaying || EditorApplication.isPaused)
			{
				Debug.LogError("[Editor Terrain Manager] can't reload chunks from the editor in play mode");
				return ;
			}

			if (mainGraph != null)
			{
				//if the graph we have is not the same / have been modified since last generation, we replace it
				if (terrain.graph != null && terrain.graph.GetHashCode() != mainGraph.GetHashCode())
					GameObject.DestroyImmediate(terrain.graph);
				
				terrain.InitGraph(mainGraph.Clone() as PWMainGraph);
				
				terrain.DestroyAllChunks();

				//updateChunks will regenerate all deleted chunks
				terrain.UpdateChunks();
			}
		}
	}
}