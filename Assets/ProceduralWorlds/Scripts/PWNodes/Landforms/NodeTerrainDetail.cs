using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using System;

namespace ProceduralWorlds.Nodes
{
	public class NodeTerrainDetail : BaseNode
	{

		[Output]
		public TerrainDetail	outputDetail = new TerrainDetail();
	
		public override void OnNodeCreation()
		{
			name = "Terrain Detail";
		}
		
	}
}