using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeMesh))]
	public class PWNodeMeshEditor : PWNodeEditor
	{
		PWGUIMeshPreview	meshPreview;
		Material			previewMaterial;

		public PWNodeMesh node;

		public override void OnNodeEnable()
		{
			node = target as PWNodeMesh;

			meshPreview = new PWGUIMeshPreview();
			previewMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);
			node.outputMesh = EditorGUILayout.ObjectField(node.outputMesh, typeof(Mesh), false) as Mesh;

			if ((node.displayPreview = EditorGUILayout.Foldout(node.displayPreview, "preview")))
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