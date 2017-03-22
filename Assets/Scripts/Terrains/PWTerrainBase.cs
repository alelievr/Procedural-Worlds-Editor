using UnityEngine;

namespace PW
{
	public enum PWChunkLoadMode
	{
		CUBIC,
		// PRIORITY_CUBIC,
		// PRIORITY_CIRCLE,
	}

	public abstract class PWTerrainBase : MonoBehaviour {
		public Vector3			position;
		public int				viewDistance;
		public PWChunkLoadMode	loadMode;
		public PWNodeGraph		graph;
		public GameObject		terrainRoot;
		
		private ChunkStorage< object > loadedChunks = new ChunkStorage< object >();
		private PWNodeGraphOutput	graphOutput;
	
		public void InitGraph(PWNodeGraph graph = null)
		{
			if (graph != null)
				this.graph = graph;
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
			//TODO: initialize graph for computing.
		}

		public object RequestChunk(Vector3 pos, int seed)
		{
			//TODO: set current seed / position for the graph:
			
			graph.ProcessGraph();
			return graphOutput.inputValues.At(0);
		}

		public virtual void RenderChunk(object chunkData, Vector3 pos)
		{
			//do nothing here, the inherited function will render it.
		}
	
		public void	UpdateChunks()
		{
			//TODO: load current chunk (no render distance for the moment)

			if (!loadedChunks.isLoaded(position))
			{
				var data = RequestChunk(position, 42);
				if (data == null)
					return ;
				loadedChunks.AddChunk(position, data);
				RenderChunk(data, position);
			}
		}
	}
}