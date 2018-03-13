using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Node;
using UnityEditor;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiomeGraphOutput))]
	public class NodeBiomeGraphOutputEditor : NodeEditor
	{
		public NodeBiomeGraphOutput node;

		public override void OnNodeEnable()
		{
			node = target as NodeBiomeGraphOutput;
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(14);
			
			EditorGUILayout.LabelField("Biome: " + node.inputBiome);
			
			PWGUI.PWArrayField(node.inputValues);
		}
	}
}