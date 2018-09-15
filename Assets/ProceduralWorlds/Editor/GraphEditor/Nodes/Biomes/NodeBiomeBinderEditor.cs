using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Nodes;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiomeBinder))]
	public class NodeBiomeBinderEditor : BaseNodeEditor
	{
		public override void OnNodeGUI()
		{
			//nothing to draw here, maybe this node will be merged with the biome input node
		}
	}
}