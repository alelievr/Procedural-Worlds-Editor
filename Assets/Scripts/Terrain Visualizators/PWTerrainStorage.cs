using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;

//Class used to store / load / update generated chunks by PWNodeGraph.

namespace PW
{
	public enum		PWStorageMode
	{
		MEMORY,
		FILE,
		// ENCRYPTED_FILE,
	}

	public enum	ChunkVisibility
	{
		None,
		VISIBLE,
		HIDDEN,
		UNLOADED,
	}

	[System.SerializableAttribute]
	[CreateAssetMenuAttribute()]
	public class PWTerrainStorage : ScriptableObject {
		
		[System.SerializableAttribute]
		public class Chunk
		{
			public ChunkData		terrainData;
			public object			userData;
			public ChunkVisibility	visibility;

			public Chunk(ChunkData terrain, object user, ChunkVisibility visi)
			{
				terrainData = terrain;
				userData = user;
				visibility = visi;
			}

			public Chunk(ChunkData terrain, object user) : this(terrain, user, ChunkVisibility.VISIBLE) {}
		}

		public PWStorageMode	storeMode = PWStorageMode.FILE;
		public string			storageFolder = null;
		public bool				editorMode;

		[NonSerializedAttribute]
		Dictionary< Vector3i, Chunk > chunks = new Dictionary< Vector3i, Chunk >();

		public void OnEnable()
		{
			storageFolder = Application.dataPath + "/Levels/";
		}

		public int Count {get {return chunks.Count;} }

		public bool isLoaded(Vector3i pos)
		{
			return chunks.ContainsKey(pos);
		}
		
		public ChunkData	AddChunk(Vector3i pos, ChunkData chunk, object userChunkDatas)
		{
			chunks[pos] = new Chunk(chunk, userChunkDatas);
			if (storeMode == PWStorageMode.FILE)
			{
				//TODO: asyn save chunkData ans pos to a file.
			}
			return chunk;
		}

		public ChunkData	GetChunkDatas(Vector3i pos)
		{
			if (chunks.ContainsKey(pos))
				return chunks[pos].terrainData;
			else if (storeMode == PWStorageMode.FILE)
			{
				//TODO: check if file at pos exists and load it if exists.
			}
			return null;
		}

		public object		GetChunkUserDatas(Vector3i pos)
		{
			if (chunks.ContainsKey(pos))
				return chunks[pos].userData;
			return null;
		}

		public Chunk this[Vector3i pos]
		{
			get {
				return chunks[pos];
			}
		}

		public void Foreach(ChunkVisibility filter, Action<Vector3i, ChunkData, object> callback)
		{
			foreach (var kp in chunks)
			{
				if (filter == ChunkVisibility.None || kp.Value.visibility == filter)
					callback(kp.Key, kp.Value.terrainData, kp.Value.userData);
			}
		}

		public void Foreach(Action<Vector3i, ChunkData, object> callback)
		{
			Foreach(ChunkVisibility.None, callback);
		}

		public void Clear()
		{
			chunks.Clear();
		}

		public bool	RemoveAt(Vector3i pos)
		{
			if (chunks.ContainsKey(pos))
			{
				chunks.Remove(pos);
				return true;
			}
			return false;
		}

		public List< Vector3i > GetLoadedChunks()
		{
			return chunks.Keys.ToList();
		}

	}
}