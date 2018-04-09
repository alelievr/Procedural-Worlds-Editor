using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using System;

namespace ProceduralWorlds
{
	public class SeamlessTerrain
	{
		public Dictionary< Vector3Int, WorldChunk >	chunks = new Dictionary< Vector3Int, WorldChunk >();

		Vector3Int									position = Vector3Int.zero;
		WorldChunk									currentChunk = null;
		int											chunkSize = -1;
		Vector3Int									oldPosition = Vector3Int.zero;

		public T			Sample< T >(Vector3Int pos, int chunkX, int chunkY, Func< WorldChunk, T > callback)
		{
			if (oldPosition != position)
			{
				currentChunk = chunks[pos];
			}
			
			if (chunkX >= 0 && chunkX < chunkSize && chunkY >= 0 && chunkY < chunkSize)
				return callback(currentChunk);
			
			//TODO: 

			return default(T);
		}
	}
}