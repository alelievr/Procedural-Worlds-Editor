using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Nodes;
using ProceduralWorlds.Core;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeGraphInput))]
	public class NodeGraphInputEditor : BaseNodeEditor
	{
		public NodeGraphInput node;

		public override void OnNodeEnable()
		{
			node = target as NodeGraphInput;
		}

		public override void OnNodeGUI()
		{
			EditorGUILayout.LabelField("inputs:");
			var names = node.outputValues.GetNames();
			var values = node.outputValues.GetValues();

			if (names != null && values != null)
			{
				for (int i = 0; i < values.Count; i++)
					if (i < names.Count)
						EditorGUILayout.LabelField(names[i] + ": " + values[i]);
					else if (values[i] != null)
						EditorGUILayout.LabelField(values[i].ToString());
			}
		}
	}
}