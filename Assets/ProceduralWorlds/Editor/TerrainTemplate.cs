using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds
{
	//Put your custom ChunkData type in the TerrainBase template
	public class TerrainTemplate : TerrainBase< ChunkData >
	{
		public override ChunkData CreateChunkData(FinalTerrain terrain)
		{
			throw new System.NotImplementedException();
		}
	}
}