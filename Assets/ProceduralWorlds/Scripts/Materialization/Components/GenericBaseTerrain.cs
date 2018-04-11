using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProceduralWorlds.Core;
using ProceduralWorlds.IsoSurfaces;
using System.Diagnostics;

using Debug = UnityEngine.Debug;

namespace ProceduralWorlds
{
	public enum ChunkLoadPatternMode
	{
		CUBIC,
		FRUSTUM,
		PRIORITY_CUBIC,
		PRIORITY_CIRCLE,
	}

	public enum NeighbourMessageMode
	{
		None,
		Mode2DXZ,
		Mode2DXZCorner,
		Mode2DXY,
		Mode2DXYCorner,
		Mode3D,
		Mode3DCorner,
	}

	[ExecuteInEditMode]
	public abstract class GenericBaseTerrain : MonoBehaviour
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
		public bool						debug = false;
		public bool						generateChunksOnLoad = true;

		protected bool					generateBorders = true;
		protected NeighbourMessageMode	neighbourMessageMode = NeighbourMessageMode.None;
		protected SeamlessTerrain		seamlessTerrain = new SeamlessTerrain();
		
		protected int					oldSeed = 0;

		[System.NonSerialized]
		bool							generatedOnLoad = false;
		
		readonly Dictionary< NeighbourMessageMode, Vector3[] > neighbourChunkPositions = new Dictionary< NeighbourMessageMode, Vector3[] >
		{
			{
				NeighbourMessageMode.Mode2DXZ,
				new[] {
					Vector3.right,
					Vector3.left,
					Vector3.forward,
					Vector3.back,
				}
			},
			{
				NeighbourMessageMode.Mode2DXZCorner,
				new[] {
					Vector3.right,
					Vector3.left,
					Vector3.forward,
					Vector3.back,
					new Vector3(1, 0, 1),
					new Vector3(1, 0, -1),
					new Vector3(-1, 0, 1),
					new Vector3(-1, 0, -1),
				}
			},
			{
				NeighbourMessageMode.Mode2DXY,
				new[] {
					Vector3.up,
					Vector3.down,
					Vector3.right,
					Vector3.left,
				}
			},
			{
				NeighbourMessageMode.Mode2DXYCorner,
				new[] {
					Vector3.up,
					Vector3.down,
					Vector3.right,
					Vector3.left,
					new Vector3(1, -1, 0),
					new Vector3(-1, 1, 0),
				}
			},
			{
				NeighbourMessageMode.Mode3D,
				new[] {
					Vector3.up,
					Vector3.down,
					Vector3.right,
					Vector3.left,
					Vector3.forward,
					Vector3.back,
				}
			},
			{
				NeighbourMessageMode.Mode3DCorner,
				new[] {
					Vector3.up,
					Vector3.down,
					Vector3.right,
					Vector3.left,
					Vector3.forward,
					Vector3.back,
					Vector3.one,
					-Vector3.one,
					new Vector3(-1, 1, 1),
					new Vector3(1, 1, -1),
					new Vector3(-1, 1, -1),
					new Vector3(-1, -1, 1),
					new Vector3(1, -1, -1),
					new Vector3(1, -1, 1),
				}
			},
		};
		
		public virtual void Start()
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
			if (generateChunksOnLoad && !generatedOnLoad)
			{
				if (graphAsset != null)
					InitGraph(graphAsset);
				
				UpdateChunks(true);

				generatedOnLoad = true;
			}

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
					Debug.Log("[ChunkLoader] TODO: " + loadPatternMode + " load mode");
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

		protected virtual Vector3 GetChunkPosition(Vector3 pos) { return pos; }
		
		//Instanciate / update ALL chunks (must be called to refresh a whole terrain)
		public void	UpdateChunks(bool ignorePositionCheck = false)
		{
			if (terrainStorage == null || graph == null)
				return ;
			
			Vector3 currentPos = ApplyWorldPositionModifier(transform.position);
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

					if (neighbourMessageMode != NeighbourMessageMode.None)
						SendNeighbourMessages(pos);
				}
				else
				{
					var chunk = terrainStorage[pos];
					OnChunkRenderGeneric(chunk.terrainData, chunk.userData, pos);
				}
			}

			oldChunkPosition = currentChunkPos;
		}

		void SendNeighbourMessages(Vector3 currentPos)
		{
			Vector3[] positions = neighbourChunkPositions[neighbourMessageMode];

			float chunkUnit = chunkSize;

			foreach (var pos in positions)
			{
				Vector3 neighbourPos = pos * chunkUnit + currentPos;
				if (terrainStorage.isLoaded(neighbourPos))
					OnNeighbourUpdate(currentPos, neighbourPos);
			}
		}

		public void	DestroyAllChunks()
		{
			if (terrainStorage == null)
				return ;

			terrainStorage.Foreach((pos, terrainData, userData) => {
				OnChunkDestroyGeneric(terrainData, userData, (Vector3)pos);
			});
			terrainStorage.Clear();
			
			seamlessTerrain.Clear();

			//manually cleanup remaining GOs:
			while (terrainRoot.transform.childCount > 0)
				DestroyImmediate(terrainRoot.transform.GetChild(0).gameObject);
		}

		public void ReloadChunks(WorldGraph newGraphAsset = null)
		{
			if (newGraphAsset != null)
				InitGraph(newGraphAsset);

			if (graphAsset != null && graph == null)
				InitGraph(graphAsset);
			
			if (graph == null)
			{
				Debug.LogError("[ChunkLoader] Can't reload chunks without a graph reference");
				return ;
			}

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

		protected virtual void OnNeighbourUpdate(Vector3 chunkPosition, Vector3 neighbourPosition) {}
		protected virtual Vector3 ApplyWorldPositionModifier(Vector3 worldPosition) { return worldPosition; }

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

			if (debug)
				ret.AddComponent< ChunkDebug >();

			ret.transform.parent = terrainRoot.transform;
			ret.transform.position = pos;
			ret.transform.localScale = Vector3.one * chunkSize * terrainScale;

			return ret;
		}

		protected void ProvideDebugInfo(GameObject gameObject, VisualDebug visualDebug, ChunkData chunkData)
		{
			if (!debug)
				return ;
			
			ChunkDebug chunkDebug = gameObject.GetComponent< ChunkDebug >();

			if (chunkDebug == null)
				return ;
			
			chunkDebug.visualDebug = visualDebug.Clone(null);
			chunkDebug.chunkData = chunkData;
		}

		#endregion
	}
}