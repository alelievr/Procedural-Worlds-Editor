using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Nodes;
using UnityEditor;
using System.Linq;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiomeSurface))]
	public class NodeBiomeSurfaceEditor : BaseNodeEditor
	{
		public NodeBiomeSurface node;
		
		readonly GUIContent		surfaceGraphError = new GUIContent("Surface graph not built !", "You have a gap in some parameter so the graph can't be correctly built");

		public override void OnNodeEnable()
		{
			node = target as NodeBiomeSurface;
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 80;

			int switchCount = node.GetInputNodes().Count();
			
			EditorGUILayout.LabelField("Texturing switches: " + switchCount);

			if (node.surfaceGraph == null)
			{
				EditorGUILayout.LabelField("Null texturing graph !");
				return ;
			}

			if (node.surfaceGraph.isBuilt)
				EditorGUILayout.LabelField("Graph built without error");
			else
				EditorGUILayout.LabelField(surfaceGraphError, Styles.errorLabel);
		}

	}
}