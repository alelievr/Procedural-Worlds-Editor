using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using System;

namespace ProceduralWorlds
{
	public class SeamlessTerrain
	{
		public delegate T ChunkSampleDelegate< T >(WorldChunk chunk, int chunkX, int chunkY); 

		public Dictionary< Vector3Int, WorldChunk >	chunks = new Dictionary< Vector3Int, WorldChunk >();

		WorldChunk									currentChunk = null;
		int											chunkSize = -1;
		Vector3Int									oldPosition = Vector3Int.zero;

		public void			AddChunk(Vector3Int pos, WorldChunk chunk)
		{
			chunks.Add(pos, chunk);
		}
		
		public void			AddChunk(Vector3 pos, WorldChunk chunk)
		{
			chunks.Add(new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z), chunk);
		}

		public T			SampleChunkAt< T >(Vector3Int pos, int chunkX, int chunkY, ChunkSampleDelegate< T > callback)
		{
			if (oldPosition != pos)
			{
				currentChunk = chunks[pos];
			}
			
			oldPosition = pos;
			
			if (chunkX >= 0 && chunkX < chunkSize && chunkY >= 0 && chunkY < chunkSize)
				return callback(currentChunk, chunkX, chunkY);
				
			pos.x -= Mathf.FloorToInt((float)-chunkX / (float)chunkSize);
			pos.y -= Mathf.FloorToInt((float)-chunkY / (float)chunkSize);

			WorldChunk	chunk;
			chunks.TryGetValue(pos, out chunk);
			if (chunk != null)
				return callback(chunk, Utils.PositiveMod(chunkX, chunkSize), Utils.PositiveMod(chunkY, chunkSize));
			
			return default(T);
		}

		public float		SampleHeight2D(Vector3Int pos, int chunkX, int chunkY)
		{
			return SampleChunkAt(pos, chunkX, chunkY, (chunk, safeChunkX, safeChunkY) => {
				return (chunk.mergedTerrain as Sampler2D)[safeChunkX, safeChunkY];
			});
		}
	}
}