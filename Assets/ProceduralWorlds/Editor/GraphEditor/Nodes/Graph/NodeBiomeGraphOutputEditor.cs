using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Nodes;
using UnityEditor;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiomeGraphOutput))]
	public class NodeBiomeGraphOutputEditor : BaseNodeEditor
	{
		public NodeBiomeGraphOutput node;

		public override void OnNodeEnable()
		{
			node = target as NodeBiomeGraphOutput;
		}

		public override void OnNodeGUI()
		{
			PWGUI.SpaceSkipAnchors();
			
			EditorGUILayout.LabelField("Biome: " + node.inputBiome);
			
			PWGUI.PWArrayField(node.inputValues);
		}
	}
}