using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProceduralWorlds.Core;
using ProceduralWorlds.IsoSurfaces;

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
		public WorldGraph				graphAsset;
		[System.NonSerialized]
		public WorldGraph				graph;

		public TerrainStorage			terrainStorage;
		public float					terrainScale = .1f; //10 cm per point

		protected int					chunkSize;
		protected float					step;
		
		public GameObject				terrainRoot;
		public bool						initialized { get { return graph != null && terrainRoot != null; } }

		protected bool					generateBorders = true;
		
		protected int					oldSeed = 0;
		
		public virtual void Start ()
		{
			if (graphAsset != null)
				InitGraph(graphAsset);
		}

		void OnEnable()
		{
			UpdateTerrainRoot();

			if (terrainStorage != null && terrainStorage.storeMode == StorageMode.Memory)
			{
				if (!Application.isPlaying && graphAsset != null)
					DestroyAllChunks();
			}

			OnTerrainEnable();
		}

		void OnDisable()
		{
			OnTerrainDisable();
		}

		public virtual void Update()
		{
			UpdateChunks();
		}
		
		void InitGraph(WorldGraph graphAsset)
		{
			this.graphAsset = graphAsset;
			this.graph = graphAsset.Clone() as WorldGraph;

			graph.SetRealMode(true);

			//we add this to generate borders
			if (generateBorders)
				graph.chunkSize += 1;
			
			chunkSize = graph.chunkSize;

			if (terrainRoot == null)
				UpdateTerrainRoot();

			graph.UpdateComputeOrder();
			graph.ProcessOnce();
		}

		void UpdateTerrainRoot()
		{
			terrainRoot = GameObject.Find(realModeRootObjectName);
			if (terrainRoot == null)
			{
				terrainRoot = new GameObject(realModeRootObjectName);
				terrainRoot.transform.position = Vector3.zero;
			}
		}

		Vector3 RoundPositionToChunk(Vector3 position, bool borders = true)
		{
			int cs = chunkSize;
			
			if (generateBorders)
				cs -= 1;
			
			if (cs > 0 && terrainScale > 0)
				position = Utils.Round((position * (1 / terrainScale)) / cs) * cs;
			else
				position = Vector3.zero;
			
			return position;
		}

		//Generate 2D positions
		IEnumerable< Vector3 > GenerateChunkPositions(Vector3 position)
		{
			//snap position to the nearest chunk:
			position = RoundPositionToChunk(position);

			int cs = (generateBorders) ? chunkSize - 1 : chunkSize;

			switch (loadPatternMode)
			{
				case ChunkLoadPatternMode.CUBIC:
					for (int x = -renderDistance; x <= renderDistance; x++)
						for (int z = -renderDistance; z <= renderDistance; z++)
						{
							Vector3 chunkPos = position + new Vector3(x * cs, 0, z * cs);

							yield return GetChunkPosition(chunkPos);
						}
					yield break ;
				default:
					Debug.Log("TODO: " + loadPatternMode + " load mode");
					break ;
			}
		}
		
		protected Vector3 GetChunkWorldPosition(Vector3 pos)
		{
			if (generateBorders)
				return pos * terrainScale + (pos * terrainScale / (chunkSize - 1));
			else
				return pos * terrainScale;
		}

		public virtual Vector3 GetChunkPosition(Vector3 pos) { return pos; }
		
		//Instanciate / update ALL chunks (must be called to refresh a whole terrain)
		public void	UpdateChunks(bool ignorePositionCheck = false)
		{
			if (terrainStorage == null || graph == null)
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

		public void ReloadChunks(WorldGraph graphAsset)
		{
			InitGraph(graphAsset);

			DestroyAllChunks();
			
			UpdateChunks(true);
		}
		
		//Chunk abstract methods
		protected abstract ChunkData RequestChunkGeneric(Vector3 pos, int seed);
		protected abstract object OnChunkCreateGeneric(ChunkData terrainData, Vector3 pos);
		protected abstract void OnChunkRenderGeneric(ChunkData terrainData, object userStoredObject, Vector3 pos);
		protected abstract void OnChunkDestroyGeneric(ChunkData terrainData, object userStoredObject, Vector3 pos);
		protected abstract void OnChunkHideGeneric(ChunkData terrainData, object userStoredObject, Vector3 pos);
		protected abstract object RequestCreateGeneric(ChunkData terrainData, Vector3 pos);

		//Terrain overridable methods
		protected virtual void OnTerrainEnable() {}
		protected virtual void OnTerrainDisable() {}
		
		//Others
		public virtual IsoSurfaceDebug GetIsoSurfaceDebug() { return null; }

		#region Utils
		/* Utils function to simplify the downstream scripting: */

		string				PositionToChunkName(Vector3 pos)
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
			ret.transform.localScale = Vector3.one * chunkSize * terrainScale;

			return ret;
		}

		#endregion
	}
}