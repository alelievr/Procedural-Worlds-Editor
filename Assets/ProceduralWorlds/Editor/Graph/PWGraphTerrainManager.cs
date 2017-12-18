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
					
			terrain.chunkSize = graph.chunkSize;
		}

		//Warning: this will destroy all loaded chunks and regenerate them
		public void ReloadChunks()
		{
			if (graph)
			{

			}
	/*		if (e.type == EventType.Layout)
			{
				if (graph.graphNeedReload)
				{
					graphNeedReload = false;
					
					terrainMaterializer.DestroyAllChunks();

					//load another instance of the current graph to separate calls:
					if (terrainMaterializer.graph != null && terrainMaterializer.graph.GetHashCode() != graph.GetHashCode())
						DestroyImmediate(terrainMaterializer.graph);
					terrainMaterializer.InitGraph(CloneGraph(graph));

					Debug.Log("graph: " + graph.GetHashCode() + " , terrainMat: " + terrainMaterializer.graph.GetHashCode());
					//process the instance of the graph in our editor so we can see datas on chunk 0, 0, 0
					graph.realMode = false;
					graph.ForeachAllNodes(n => n.Updategraph(graph));
					graph.UpdateChunkPosition(Vector3.zero);

					if (graphNeedReloadOnce)
						graph.ProcessGraphOnce();
					graphNeedReloadOnce = false;

					graph.ProcessGraph();
				}
				//updateChunks will update and generate new chunks if needed.
				//TODOMAYBE: remove this when workers will be added to the Terrain.
				terrainMaterializer.UpdateChunks();
			}*/
		}
	}
}