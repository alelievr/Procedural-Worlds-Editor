using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Node
{
	public class NodeBiomeSurfaceColor : BaseNode
	{

		[Output]
		public BiomeSurfaceColor	surfaceColor = new BiomeSurfaceColor();

		//Called only when the node is created (not when it is enabled/loaded)
		public override void OnNodeCreation()
		{
			name = "Surface color";
		}
		
	}
}