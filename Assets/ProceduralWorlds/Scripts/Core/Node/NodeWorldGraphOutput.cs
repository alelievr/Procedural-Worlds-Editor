using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Node
{
	public class NodeWorldGraphOutput : BaseNode
	{
		[Input("Final terrain")]
		public FinalTerrain			finalTerrain;

		[Input]
		public PWArray< object >	inputValues = new PWArray< object >();

		//Called only when the node is created (not when it is enabled/loaded)
		public override void OnNodeCreation()
		{
			name = "World graph output";
		}
		
	}
}