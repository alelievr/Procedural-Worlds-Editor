using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Nodes;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiomeDataDecomposer))]
	public class NodeBiomeDataDecomposerEditor : BaseNodeEditor
	{
		public override void OnNodeGUI()
		{
			//Nothing to render here
		}
	}
}