using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProceduralWorlds.Core;

namespace ProceduralWorlds
{
	public enum PWChunkLoadPatternMode
	{
		CUBIC,
		FRUSTUM,
		PRIORITY_CUBIC,
		PRIORITY_CIRCLE,
	}

	public abstract class TerrainGenericBase : MonoBehaviour
	{
		public static readonly string	realModeRootObjectName = "PWWorldRoot";

		public Vector3					position;
		public int						renderDistance;
		public PWChunkLoadPatternMode	loadPatternMode;
		public WorldGraph				graph;
		public TerrainStorage			terrainStorage;
		public float					terrainScale = .1f; //10 cm per point
		
		public int						chunkSize { get; private set; }
		
		[HideInInspector]
		public GameObject				terrainRoot;
		public bool						initialized { get { return graph != null && terrainRoot != null && graphOutput != null; } }
		
		[SerializeField]
		[HideInInspector]
		protected NodeGraphOutput		graphOutput = null;

		protected int					oldSeed = 0;
		
		void Start ()
		{
			InitGraph(graph);
		}
		
		public void InitGraph(WorldGraph graph)
		{
			if (graph != null)
				this.graph = graph;
			else
				return ;

			graph.SetRealMode(true);
			chunkSize = graph.chunkSize;
			graphOutput = graph.outputNode as NodeGraphOutput;
			graph.UpdateComputeOrder();
			if (!graph.IsRealMode())
				terrainRoot = GameObject.Find("PWPreviewTerrain");
			else
			{
				terrainRoot = GameObject.Find(realModeRootObjectName);
				if (terrainRoot == null)
				{
					terrainRoot = new GameObject(realModeRootObjectName);
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
				position = Utils.Round(position / chunkSize) * chunkSize;
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
		
		public abstract ChunkData RequestChunkGeneric(Vector3 pos, int seed);
		public abstract object OnChunkCreateGeneric(ChunkData terrainData, Vector3 pos);
		public abstract void OnChunkRenderGeneric(ChunkData terrainData, object userStoredObject, Vector3 pos);
		public abstract void OnChunkDestroyGeneric(ChunkData terrainData, object userStoredObject, Vector3 pos);
		public abstract void OnChunkHideGeneric(ChunkData terrainData, object userStoredObject, Vector3 pos);
		public abstract object RequestCreateGeneric(ChunkData terrainData, Vector3 pos);

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