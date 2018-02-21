using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;
using PW.Core;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeBiomeGraphOutput))]
	public class PWNodeBiomeGraphOutputEditor : PWNodeEditor
	{
		public PWNodeBiomeGraphOutput node;

		public override void OnNodeEnable()
		{
			node = target as PWNodeBiomeGraphOutput;
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(14);
			
			EditorGUILayout.LabelField("Biome: " + node.inputBiome);
			
			PWGUI.PWArrayField(node.inputValues);
		}
	}
}