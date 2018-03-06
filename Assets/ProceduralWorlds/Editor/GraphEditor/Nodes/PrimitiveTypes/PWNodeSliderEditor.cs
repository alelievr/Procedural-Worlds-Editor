using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeSlider))]
	public class PWNodeSliderEditor : PWNodeEditor
	{
		public PWNodeSlider node;

		readonly string changeKey = "Slider";

		public override void OnNodeEnable()
		{
			node = target as PWNodeSlider;
			
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