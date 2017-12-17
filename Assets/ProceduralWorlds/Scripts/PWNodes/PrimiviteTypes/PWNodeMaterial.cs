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

		public override void OnNodeCreation()
		{
			renamable = true;
			name = "Material";
		}

		public override void OnNodeEnable()
		{
			matPreview = new PWGUIMaterialPreview();
		}
		
		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);

			outputMaterial = EditorGUILayout.ObjectField(outputMaterial, typeof(Material), false) as Material;
			
			if ((showPreview = EditorGUILayout.Foldout(showPreview, "preview")))
				matPreview.Render(outputMaterial);
		}
		
		public override void OnNodeDisable()
		{
			if (matPreview != null)
				matPreview.Cleanup();
		}

		//no process needed

	}
}
