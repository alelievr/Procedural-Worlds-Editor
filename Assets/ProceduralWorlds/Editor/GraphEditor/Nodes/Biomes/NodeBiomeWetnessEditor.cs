using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Node;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiomeWetness))]
	public class NodeBiomeWetnessEditor : BaseNodeEditor
	{
		public NodeBiomeWetness node;

		public override void OnNodeEnable()
		{
			node = target as NodeBiomeWetness;
		}

		public override void OnNodeGUI()
		{
			//TODO
		}
	}
}