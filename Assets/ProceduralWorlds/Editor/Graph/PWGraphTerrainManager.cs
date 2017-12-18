using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Editor
{
	public class PWGraphTerrainManager
	{
		//terrain base game object reference
		public static PWTerrainBase	terrainReference;

		//Graph reference
		PWGraph				graph;

		PWTerrainBase		terrain;

		Event				e { get { return Event.current; } }


		public PWGraphTerrainManager(PWGraph graph)
		{
			this.graph = graph;
		}

		public void LoadStyles()
		{

		}
	
		public void DrawTerrainSettings(Rect settingsRect)
		{
			if (terrain == null)
				terrain = GameObject.FindObjectOfType< PWTerrainBase >();

			if (terrain == null)
				return ;

			terrainReference = terrain;

			terrain.renderDistance = EditorGUILayout.IntSlider("chunk Render distance", terrain.renderDistance, 0, 24);

			if (GUILayout.Button("Generate terrain"))
				ReloadChunks();
			
			terrain.chunkSize = graph.chunkSize;
		}

		//Warning: this will destroy all loaded chunks and regenerate them
		public void ReloadChunks()
		{
			if (EditorApplication.isPlaying || EditorApplication.isPaused)
			{
				Debug.LogError("[Editor Terrain Manager] can't reload chunks from the editor in play mode");
				return ;
			}

			PWMainGraph mainGraph = graph as PWMainGraph;

			if (mainGraph != null)
			{
				terrain.DestroyAllChunks();

				//if the graph we have is not the same / have been modified since last generation, we replace it
				if (terrain.graph != null && terrain.graph.GetHashCode() != graph.GetHashCode())
					GameObject.DestroyImmediate(terrain.graph);
				
				terrain.InitGraph(graph.Clone() as PWMainGraph);

				//updateChunks will regenerate all deleted chunks
				terrain.UpdateChunks();
			}
		}
	}
}