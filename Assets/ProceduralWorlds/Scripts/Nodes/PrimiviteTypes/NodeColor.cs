using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Nodes
{
	public class NodeColor : BaseNode {

		[Output]
		public Color		outputColor;
	
		public override void OnNodeCreation()
		{
			name = "Color";
		}

		//no process needed
	}
}