using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Node;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeSlider))]
	public class NodeSliderEditor : NodeEditor
	{
		public NodeSlider node;

		readonly string changeKey = "Slider";

		public override void OnNodeEnable()
		{
			node = target as NodeSlider;
			
			delayedChanges.BindCallback(changeKey, (value) => { NotifyReload(); });
		}

		public override void OnNodeGUI()
		{
			EditorGUI.BeginChangeCheck();
			node.outValue = EditorGUILayout.Slider(node.outValue, node.min, node.max);
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(changeKey, node.outValue);
		}
	}
}