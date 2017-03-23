using UnityEngine;

namespace PW
{
	public enum PWChunkLoadMode
	{
		CUBIC,
		// PRIORITY_CUBIC,
		// PRIORITY_CIRCLE,
	}

	[System.SerializableAttribute]
	public abstract class PWTerrainBase : MonoBehaviour {
		public Vector3			position;
		public int				viewDistance;
		[SerializeField]
		public PWChunkLoadMode	loadMode;
		[SerializeField]
		public PWNodeGraph		graph;
		public GameObject		terrainRoot;
		public bool				initialized {get {return graph != null && terrainRoot != null && graphOutput != null;}}
		
		[SerializeField]
		private ChunkStorage< object, object > loadedChunks = new ChunkStorage< object, object >();
		[SerializeField]
		private PWNodeGraphOutput	graphOutput = null;
	
		public void InitGraph(PWNodeGraph graph = null)
		{
			if (graph != null)
				this.graph = graph;
			Debug.Log("initgraph graph: " + graph);
			graphOutput = graph.outputNode as PWNodeGraphOutput;
			if (!graph.realMode)
				terrainRoot = GameObject.Find("PWPreviewTerrain");
			if (terrainRoot == null)
			{
				terrainRoot = GameObject.Find(PWConstants.RealModeRootObjectName);
				if (terrainRoot == null)
				{
					terrainRoot = new GameObject(PWConstants.RealModeRootObjectName);
					terrainRoot.transform.position = Vector3.zero;
				}
			}
		}

		public object RequestChunk(Vector3 pos, int seed)
		{
			//TODO: set current seed / position for the graph:
			//graph.SetSeed() and graph.SetPosition();
			
			graph.ProcessGraph();
			Debug.Log("graph output: " + graphOutput);
			Debug.Log("graph output after processing: " + graphOutput.inputValues);
			return graphOutput.inputValues.At(0);
		}

		public virtual object RenderChunk(object chunkData, Vector3 pos)
		{
			//do nothing here, the inherited function will render it.
			return null;
		}

		public virtual void UpdateChunkRender(object chunkData, object userStoredObject, Vector3 pos)
		{
			//do nothing here, the inherited function will update render.
		}
	
		//Instanciate / update ALL chunks (must be called to refresh a whole terrain)
		public void	UpdateChunks()
		{
			//TODO: view distance loading algorithm.

			if (!loadedChunks.isLoaded(position))
			{
				var data = RequestChunk(position, 42);
				if (data == null)
					return ;
				var userChunkData = RenderChunk(data, position);
				loadedChunks.AddChunk(position, data, userChunkData);
			}
			else
			{
				var chunk = loadedChunks[position];
				UpdateChunkRender(chunk.first, chunk.second, position);
			}
		}
	}
}