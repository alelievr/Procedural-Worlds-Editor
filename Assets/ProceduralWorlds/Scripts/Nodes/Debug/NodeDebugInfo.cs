using UnityEngine;
using System;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Nodes
{
	public class NodeDebugInfo : BaseNode
	{
		[Input]
		public object		obj;

		public override void OnNodeCreation()
		{
			name = "Debug Info";
			renamable = true;
		}
	}
}
