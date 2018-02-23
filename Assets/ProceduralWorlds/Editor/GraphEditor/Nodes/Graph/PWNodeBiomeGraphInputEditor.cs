using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Node;
using PW.Core;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeBiomeGraphInput))]
	public class PWNodeBiomeGraphInputEditor : PWNodeEditor
	{
		public PWNodeBiomeGraphInput	node;

		BiomeDataDrawer					biomeDataDrawer = new BiomeDataDrawer();

		public override void OnNodeEnable()
		{
			node = target as PWNodeBiomeGraphInput;
			
			if (node.previewGraph == null)
			{
				var graphs = Resources.FindObjectsOfTypeAll< PWMainGraph >();

				//TODO: if there is multiple graphs, find the one who have our biomeGraph in it.
				if (graphs.Length > 0)
					node.previewGraph = graphs[0];
			}
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(14);
			
			PWGUI.PWArrayField(node.outputValues);

			EditorGUILayout.LabelField("Preview graph");
			node.previewGraph = EditorGUILayout.ObjectField(node.previewGraph, typeof(PWMainGraph), false) as PWMainGraph;

			if (node.previewGraph == null)
				EditorGUILayout.HelpBox("Can't process the graph without a preview graph ", MessageType.Error);
			
			if (node.outputPartialBiome != null)
			{
				if (!biomeDataDrawer.isEnabled)
					biomeDataDrawer.OnEnable(node.outputPartialBiome);
				biomeDataDrawer.OnGUI(rect);
			}
			
			node.calls = 0;
		}
	}
}