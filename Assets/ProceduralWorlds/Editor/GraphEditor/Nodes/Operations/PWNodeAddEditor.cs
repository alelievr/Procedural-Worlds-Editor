using UnityEditor;
using UnityEngine;
using PW.Node;
using PW.Editor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeAdd))]
	public class PWNodeAddEditor : PWNodeEditor
	{
		PWNodeAdd		node;

		public override void OnNodeEnable()
		{
			node = target as PWNodeAdd;
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 100;

			EditorGUILayout.LabelField("result: " + node.fOutput);
		}
	}
}