using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;

namespace PW.Editor
{ 
	[CustomEditor(typeof(PWNodeMaterial))]
	public class PWNodeMaterialEditor : PWNodeEditor
	{
		public PWNodeMaterial node;
		
		PWGUIMaterialPreview	matPreview;

		public override void OnNodeEnable()
		{
			node = target as PWNodeMaterial;
			
			matPreview = new PWGUIMaterialPreview();
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);

			node.outputMaterial = EditorGUILayout.ObjectField(node.outputMaterial, typeof(Material), false) as Material;
			
			if ((node.showPreview = EditorGUILayout.Foldout(node.showPreview, "preview")))
				matPreview.Render(node.outputMaterial);

		}

		public override void OnNodeDisable()
		{
			matPreview.Cleanup();
		}
	}
}