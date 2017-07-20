using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeMaterial : PWNode {

		[PWOutput("Material")]
		public Material			outputMaterial;
		PWGUIMaterialPreview	matPreview;

		[SerializeField]
		bool					showPreview;
		[SerializeField]
		bool					showSceneHiddenObjects = false;

		public override void OnNodeCreate()
		{
			renamable = true;
			externalName = "Material";
			
			matPreview = new PWGUIMaterialPreview(outputMaterial);
		}
		
		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);

			EditorGUI.BeginChangeCheck();
			outputMaterial = EditorGUILayout.ObjectField(outputMaterial, typeof(Material), false) as Material;
			if (EditorGUI.EndChangeCheck())
				matPreview.SetMaterial(outputMaterial);
			
			EditorGUI.BeginChangeCheck();
			showSceneHiddenObjects = EditorGUILayout.Toggle("Show scene hidden objects", showSceneHiddenObjects);
			if (EditorGUI.EndChangeCheck())
				matPreview.UpdateShowSceneHiddenObjects(showSceneHiddenObjects);
			
			if ((showPreview = EditorGUILayout.Foldout(showPreview, "preview")))
				matPreview.Render();
		}
		
		public override void OnNodeDisable()
		{
			if (matPreview != null)
				matPreview.Cleanup();
		}

		//no process needed

	}
}
