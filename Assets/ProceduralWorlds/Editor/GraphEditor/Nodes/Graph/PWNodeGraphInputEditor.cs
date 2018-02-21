using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using PW.Core;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeGraphInput))]
	public class PWNodeGraphInputEditor : PWNodeEditor
	{
		public PWNodeGraphInput node;

		public override void OnNodeEnable()
		{
			node = target as PWNodeGraphInput;
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