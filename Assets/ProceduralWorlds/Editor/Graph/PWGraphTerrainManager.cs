using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using System;

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

		Dictionary< MaterializerType, Type > materializerTypes = new Dictionary< MaterializerType, Type >()
		{
			{MaterializerType.SquareTileMap, typeof(PWTopDown2DTerrainSquare)},
		};


		public PWGraphTerrainManager(PWGraph graph)
		{
			this.graph = graph;
		}

		public void DrawTerrainSettings(Rect settingsRect, MaterializerType type)
		{
			if (terrain == null)
				terrain = GameObject.FindObjectOfType< PWTerrainBase >();

			if (terrain == null)
				return ;
			
			terrainReference = terrain;
			
			Type expectedType = materializerTypes[type];

			if (terrainReference.GetType() != expectedType)
			{
				GameObject go = terrainReference.gameObject;
				GameObject.DestroyImmediate(terrainReference);
				terrainReference = go.AddComponent(expectedType) as PWTerrainBase;
			}

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