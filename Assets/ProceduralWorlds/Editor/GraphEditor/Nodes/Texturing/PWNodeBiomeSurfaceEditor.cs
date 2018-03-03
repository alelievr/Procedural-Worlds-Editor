using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;
using System.Linq;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeBiomeSurface))]
	public class PWNodeBiomeSurfaceEditor : PWNodeEditor
	{
		public PWNodeBiomeSurface node;
		
		readonly GUIContent		surfaceGraphError = new GUIContent("Surface graph not built !", "You have a gap in some parameter so the graph can't be correctly built");

		public override void OnNodeEnable()
		{
			node = target as PWNodeBiomeSurface;
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 80;

			int switchCount = node.GetInputNodes().Count();
			
			EditorGUILayout.LabelField("Texturing switches: " + switchCount);

			if (node.surfaceGraph.isBuilt)
				EditorGUILayout.LabelField("Graph built without error");
			else
				EditorGUILayout.LabelField(surfaceGraphError, PWStyles.errorLabel);
		}

	}
}