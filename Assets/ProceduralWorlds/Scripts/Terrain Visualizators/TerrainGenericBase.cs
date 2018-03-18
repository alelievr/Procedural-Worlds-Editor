using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProceduralWorlds.Core;

namespace ProceduralWorlds
{
	public enum ChunkLoadPatternMode
	{
		CUBIC,
		FRUSTUM,
		PRIORITY_CUBIC,
		PRIORITY_CIRCLE,
	}

	[ExecuteInEditMode]
	public abstract class TerrainGenericBase : MonoBehaviour
	{
		public static readonly string	realModeRootObjectName = "PWWorldRoot";

		public Vector3					oldChunkPosition;
		public int						renderDistance;
		public ChunkLoadPatternMode		loadPatternMode;
		public WorldGraph				graph;

		public TerrainStorage			terrainStorage;
		public float					terrainScale = .1f; //10 cm per point

		protected int					chunkSize;
		
		public GameObject				terrainRoot;
		public bool						initialized { get { return graph != null && terrainRoot != null && graphOutput != null; } }
		
		[SerializeField]
		protected NodeGraphOutput		graphOutput = null;

		protected int					oldSeed = 0;
		
		public virtual void Start ()
		{
			InitGraph(graph);
		}

		void OnEnable()
		{
			if (terrainStorage != null && terrainStorage.storeMode == StorageMode.Memory)
			{
				if (!Application.isPlaying && graph != null)
					DestroyAllChunks();
			}
		}

		public virtual void Update()
		{
			UpdateChunks();
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
			
			UpdateChunks(true);
		}

		Vector3 RoundPositionToChunk(Vector3 position)
		{
			if (chunkSize > 0 && terrainScale > 0)
				position = Utils.Round((position * (1 / terrainScale)) / chunkSize) * chunkSize;
			else
				position = Vector3.zero;
			
			return position;
		}
		
		//Generate 2D positions
		IEnumerable< Vector3 > GenerateChunkPositions(Vector3 position)
		{
			//snap position to the nearest chunk:
			position = RoundPositionToChunk(position);

			switch (loadPatternMode)
			{
				case ChunkLoadPatternMode.CUBIC:
					Vector3 pos = position;
					for (int x = -renderDistance; x <= renderDistance; x++)
						for (int z = -renderDistance; z <= renderDistance; z++)
						{
							Vector3 chunkPos = pos + new Vector3(x * chunkSize, 0, z * chunkSize);
							yield return GetChunkPosition(chunkPos);
						}
					yield break ;
				default:
					Debug.Log("TODO: " + loadPatternMode + " load mode");
					break ;
			}
			yield return position;
		}

		public virtual Vector3 GetChunkPosition(Vector3 pos) { return pos; }
		
		//Instanciate / update ALL chunks (must be called to refresh a whole terrain)
		public void	UpdateChunks(bool ignorePositionCheck = false)
		{
			if (terrainStorage == null)
				return ;
			
			Vector3 currentPos = transform.position;
			Vector3 currentChunkPos = RoundPositionToChunk(currentPos);
			
			if (!ignorePositionCheck && oldChunkPosition == currentChunkPos)
				return ;

			foreach (var pos in GenerateChunkPositions(currentPos))
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

			oldChunkPosition = currentChunkPos;
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(oldChunkPosition, 4);

			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(RoundPositionToChunk(transform.position), 4);
		}

		public void	DestroyAllChunks()
		{
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

		public void ReloadChunks(WorldGraph graph)
		{
			InitGraph(graph.Clone() as WorldGraph);

			DestroyAllChunks();
			
			UpdateChunks(true);

			Debug.Log("Reloaded 1");
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