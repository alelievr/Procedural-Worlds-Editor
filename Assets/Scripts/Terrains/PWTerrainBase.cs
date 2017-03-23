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

		private	int				oldSeed = 0;
	
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
		}

		public object RequestChunk(Vector3 pos, int seed)
		{
			if (seed != oldSeed)
				graph.UpdateSeed(seed);

			graph.UpdateChunkPosition(pos);
			
			graph.ProcessGraph();

			oldSeed = seed;
			//TODO: add the possibility to retreive in Terrain materializers others output.
			return graphOutput.inputValues.At(0); //return the first value of output
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