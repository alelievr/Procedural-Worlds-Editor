using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Nodes;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeSlider))]
	public class NodeSliderEditor : BaseNodeEditor
	{
		public NodeSlider node;

		readonly string changeKey = "Slider";

		public override void OnNodeEnable()
		{
			node = target as NodeSlider;
			
			delayedChanges.BindCallback(changeKey, (unused) => { NotifyReload(); });
		}

		public override void OnNodeGUI()
		{
			EditorGUI.BeginChangeCheck();
			node.sliderValue = EditorGUILayout.Slider(node.sliderValue, node.min, node.max);
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(changeKey);
		}
	}
}