using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Nodes;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeMesh))]
	public class NodeMeshEditor : BaseNodeEditor
	{
		GUIMeshPreview	meshPreview;
		Material			previewMaterial;

		public NodeMesh node;

		public override void OnNodeEnable()
		{
			node = target as NodeMesh;

			meshPreview = new GUIMeshPreview();
			previewMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);
			node.outputMesh = EditorGUILayout.ObjectField(node.outputMesh, typeof(Mesh), false) as Mesh;

			node.displayPreview = EditorGUILayout.Foldout(node.displayPreview, "preview");
			
			if (node.displayPreview)
			{
				EditorGUIUtility.labelWidth = 75;
				
				previewMaterial = EditorGUILayout.ObjectField("preview Mat", previewMaterial, typeof(Material), false) as Material;
	
	 			meshPreview.Render(node.outputMesh, previewMaterial);
			}
		}

		public override void OnNodeDisable()
		{
			meshPreview.Cleanup();
		}
	}
}