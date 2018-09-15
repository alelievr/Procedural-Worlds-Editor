using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Nodes;
using UnityEditor;

namespace ProceduralWorlds.Editor
{ 
	[CustomEditor(typeof(NodeMaterial))]
	public class NodeMaterialEditor : BaseNodeEditor
	{
		public NodeMaterial node;
		
		GUIMaterialPreview	matPreview;

		public override void OnNodeEnable()
		{
			node = target as NodeMaterial;
			
			matPreview = new GUIMaterialPreview();
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);

			node.outputMaterial = EditorGUILayout.ObjectField(node.outputMaterial, typeof(Material), false) as Material;
			
			node.showPreview = EditorGUILayout.Foldout(node.showPreview, "preview");
			
			if (node.showPreview)
				matPreview.Render(node.outputMaterial);

		}

		public override void OnNodeDisable()
		{
			matPreview.Cleanup();
		}
	}
}