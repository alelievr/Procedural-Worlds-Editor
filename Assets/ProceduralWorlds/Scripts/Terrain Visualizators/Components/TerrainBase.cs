using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProceduralWorlds.Core;

namespace ProceduralWorlds
{
	[System.Serializable]
	public abstract class TerrainBase< T > : TerrainGenericBase  where T : ChunkData
	{

		//Generic to specif bindings:
		public override ChunkData RequestChunkGeneric(Vector3 pos, int seed) { return RequestChunk(pos, seed); }
		public override object OnChunkCreateGeneric(ChunkData terrainData, Vector3 pos) { return OnChunkCreate(terrainData as T, pos); } 
		public override void OnChunkRenderGeneric(ChunkData terrainData, object userStoredObject, Vector3 pos) { OnChunkRender(terrainData as T, userStoredObject, pos); } 
		public override void OnChunkDestroyGeneric(ChunkData terrainData, object userStoredObject, Vector3 pos) { OnChunkDestroy(terrainData as T, userStoredObject, pos); } 
		public override void OnChunkHideGeneric(ChunkData terrainData, object userStoredObject, Vector3 pos) { OnChunkHide(terrainData as T, userStoredObject, pos); }
		public override object RequestCreateGeneric(ChunkData terrainData, Vector3 pos) { return RequestCreate(terrainData as T, pos); }

		public T RequestChunk(Vector3 pos, int seed)
		{
			if (seed != oldSeed)
				graph.seed = seed;

			graph.chunkPosition = pos;
			
			graph.Process();

			oldSeed = seed;
			FinalTerrain finalTerrain = graph.GetOutputTerrain();

			if (finalTerrain == null)
			{
				Debug.LogWarning("[TerrainBase] Graph output terrain is null !");
				return null;
			}
			
			return CreateChunkData(finalTerrain);
		}

		public abstract T CreateChunkData(FinalTerrain terrain);

		public virtual object OnChunkCreate(T terrainData, Vector3 pos)
		{
			//do nothing here, the inherited function will render it.
			return null;
		}

		public virtual void OnChunkRender(T terrainData, object userStoredObject, Vector3 pos)
		{
			//do nothing here, the inherited function will update render.
		}

		public virtual void OnChunkDestroy(T terrainData, object userStoredObject, Vector3 pos)
		{

		}

		public virtual void OnChunkHide(T terrainData, object userStoredObject, Vector3 pos)
		{

		}

		public object RequestCreate(T terrainData, Vector3 pos)
		{
			var userData = OnChunkCreate(terrainData, pos);
			if (terrainStorage == null)
				return userData;
			if (terrainStorage.isLoaded(pos))
				terrainStorage[pos].userData = userData;
			else
				terrainStorage.AddChunk(pos, terrainData, userData);
			return userData;
		}

		public void FillChunkData(ChunkData chunk, FinalTerrain finalTerrain)
		{
			chunk.size = finalTerrain.mergedTerrain.size;
			chunk.materializerType = finalTerrain.materializerType;
			chunk.terrain = finalTerrain.mergedTerrain;
			chunk.biomeMap = finalTerrain.biomeData.biomeMap;
			chunk.biomeMap3D = null;
		}
	}
}