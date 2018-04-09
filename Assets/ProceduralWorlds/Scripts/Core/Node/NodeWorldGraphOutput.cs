using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Nodes
{
	public class NodeWorldGraphOutput : BaseNode
	{
		[Input("Final terrain")]
		public WorldChunk			finalTerrain;

		[Input]
		public PWArray< object >	inputValues = new PWArray< object >();

		//Called only when the node is created (not when it is enabled/loaded)
		public override void OnNodeCreation()
		{
			name = "World graph output";
		}
		
	}
}