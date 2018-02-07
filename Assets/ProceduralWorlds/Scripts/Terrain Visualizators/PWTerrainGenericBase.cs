using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PW.Core;

namespace PW
{
	public enum PWChunkLoadPatternMode
	{
		CUBIC,
		// FRUSTUM,
		// PRIORITY_CUBIC,
		// PRIORITY_CIRCLE,
	}

	public abstract class PWTerrainGenericBase : MonoBehaviour
	{
		public Vector3					position;
		public int						renderDistance;
		public PWChunkLoadPatternMode	loadPatternMode;
		public PWMainGraph				graph;
		public PWTerrainStorage			terrainStorage;
		public float					terrainScale = .1f; //10 cm per point
		
		public int						chunkSize { get; private set; }
		
		[HideInInspector]
		public GameObject		terrainRoot;
		public bool				initialized {get {return graph != null && terrainRoot != null && graphOutput != null;}}
		
		[SerializeField]
		[HideInInspector]
		protected PWNodeGraphOutput	graphOutput = null;

		protected	int			oldSeed = 0;
		protected int			WorkerGenerationId = 42;
		
		void Start ()
		{
			InitGraph(graph);
		}
		
		public void InitGraph(PWMainGraph graph)
		{
			if (graph != null)
				this.graph = graph;
			else
				return ;
			
			graph.SetRealMode(true);
			chunkSize = graph.chunkSize;
			graphOutput = graph.outputNode as PWNodeGraphOutput;
			graph.UpdateComputeOrder();
			if (!graph.IsRealMode())
				terrainRoot = GameObject.Find("PWPreviewTerrain");
			if (terrainRoot == null)
			{
				terrainRoot = GameObject.Find(PWConstants.realModeRootObjectName);
				if (terrainRoot == null)
				{
					terrainRoot = new GameObject(PWConstants.realModeRootObjectName);
					terrainRoot.transform.position = Vector3.zero;
				}
			}
			graph.ProcessOnce();
		}
		
		//Generate 2D positions
		IEnumerable< Vector3 > GenerateChunkPositions()
		{
			//snap position to the nearest chunk:
			if (chunkSize > 0)
				position = PWUtils.Round(position / chunkSize) * chunkSize;
			else
				position = Vector3.zero;

			switch (loadPatternMode)
			{
				case PWChunkLoadPatternMode.CUBIC:
					Vector3 pos = position;
					for (int x = -renderDistance; x <= renderDistance; x++)
						for (int z = -renderDistance; z <= renderDistance; z++)
						{
							Vector3 chunkPos = pos + new Vector3(x * chunkSize, 0, z * chunkSize);
							yield return chunkPos;
						}
					yield break ;
				default:
					Debug.Log("TODO: " + loadPatternMode + " load mode");
					break ;
			}
			yield return position;
		}

		public abstract ChunkData RequestChunkGeneric(Vector3 pos, int seed);
		public abstract object OnChunkCreateGeneric(ChunkData terrainData, Vector3 pos);
		public abstract void OnChunkRenderGeneric(ChunkData terrainData, object userStoredObject, Vector3 pos);
		public abstract void OnChunkDestroyGeneric(ChunkData terrainData, object userStoredObject, Vector3 pos);
		public abstract void OnChunkHideGeneric(ChunkData terrainData, object userStoredObject, Vector3 pos);
		public abstract object RequestCreateGeneric(ChunkData terrainData, Vector3 pos);
		
		//Instanciate / update ALL chunks (must be called to refresh a whole terrain)
		public void	UpdateChunks()
		{
			Debug.Log("Updating chunks, storage: " + terrainStorage);
			if (terrainStorage == null)
				return ;

			foreach (var pos in GenerateChunkPositions())
			{
				if (!terrainStorage.isLoaded(pos))
				{
					var data = RequestChunkGeneric(pos, graph.seed);
					var userChunkData = OnChunkCreateGeneric(data, pos);
					terrainStorage.AddChunk(pos, data, userChunkData);
					/*PWWorker.EnqueueTask(
						() => RequestChunk(pos, graph.seed),
						(chunkData) => {
							T data = chunkData as T;
							var userChunkData = OnChunkCreate(data, pos);
							terrainStorage.AddChunk(pos, data, userChunkData);
						},
						WorkerGenerationId
					);*/
				}
				else
				{
					var chunk = terrainStorage[pos];
					OnChunkRenderGeneric(chunk.terrainData, chunk.userData, pos);
				}
			}
		}

		public void	DestroyAllChunks()
		{
			Debug.Log("Destroying all chunks");
			PWWorker.StopAllWorkers(WorkerGenerationId);
			if (terrainStorage == null)
				return ;
			terrainStorage.Foreach((pos, terrainData, userData) => {
				OnChunkDestroyGeneric(terrainData, userData, (Vector3)pos);
			});
			terrainStorage.Clear();

			//manually cleanup remaining GOs:
			while (terrainRoot.transform.childCount > 0)
				DestroyImmediate(terrainRoot.transform.GetChild(0).gameObject);
		}

		#region Utils
		/* Utils function to simplify the downstream scripting: */

		string				PositionToChunkName(Vector3i pos)
		{
			return "chunk (" + pos.x + ", " + pos.y + ", " + pos.z + ")";
		}

		GameObject			TryFindExistingGameobjectByName(string name)
		{
			Transform t = terrainRoot.transform.Find(name);
			if (t != null)
				return t.gameObject;
			return null;
		}

		public GameObject	CreateChunkObject(Vector3 pos)
		{
			string		name = PositionToChunkName(pos);
			GameObject	ret;

			ret = TryFindExistingGameobjectByName(name);
			if (ret != null && ret.GetComponent< MeshRenderer >() == null)
				return ret;
			else if (ret != null)
				GameObject.DestroyImmediate(ret);
			
			ret = new GameObject(name);
			ret.transform.parent = terrainRoot.transform;
			ret.transform.position = pos;
			//TODO: implement Sampler* scale (step) in the scale of the object.

			return ret;
		}

		#endregion
	}
}