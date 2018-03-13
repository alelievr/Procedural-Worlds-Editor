using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;
using System;
using ProceduralWorlds;

namespace ProceduralWorlds.Editor
{
	public class ChunkLoaderDrawer : PWDrawer
	{
		WorldGraph		worldGraph;

		public override void OnEnable()
		{
			worldGraph =  target as WorldGraph;
		}

		new public void OnGUI(Rect r)
		{
			base.OnGUI(r);

			var terrain = PWTerrainPreviewManager.instance.terrainBase;

			if (terrain == null)
			{
				EditorGUILayout.HelpBox("Terrain materializer type not supported (" + worldGraph.terrainPreviewType + ")", MessageType.Warning);
				return ;
			}
			
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
				Debug.LogError("[Editor Terrain Manager] can't reload chunks in play mode");
				return ;
			}

			if (worldGraph != null)
			{
				//if the graph we have is not the same / have been modified since last generation, we replace it
				if (terrain.graph != null && terrain.graph.GetHashCode() != worldGraph.GetHashCode())
					GameObject.DestroyImmediate(terrain.graph);
				
				terrain.InitGraph(worldGraph.Clone() as WorldGraph);
				
				terrain.DestroyAllChunks();

				//updateChunks will regenerate all deleted chunks
				terrain.UpdateChunks();
			}
		}
	}
}