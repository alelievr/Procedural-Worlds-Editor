using UnityEditor;
using UnityEngine;
using ProceduralWorlds.Node;
using ProceduralWorlds.Editor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeAdd))]
	public class NodeAddEditor : BaseNodeEditor
	{
		NodeAdd		node;

		public override void OnNodeEnable()
		{
			node = target as NodeAdd;
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 100;

			node.roundToInt = EditorGUILayout.Toggle("Round to int", node.roundToInt);

			EditorGUILayout.LabelField("result: " + node.fOutput);
		}
	}
}