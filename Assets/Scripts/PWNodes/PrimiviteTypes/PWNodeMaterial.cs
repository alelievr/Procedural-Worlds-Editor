using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeMaterial : PWNode {

		[PWOutput("Material")]
		public Material			outputMaterial;
		PWGUIObjectPreview		objectPreview = new PWGUIObjectPreview();

		public override void OnNodeCreate()
		{
			objectPreview.Initialize();
			renamable = true;
			externalName = "Material";
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);

			outputMaterial = EditorGUILayout.ObjectField(outputMaterial, typeof(Material), false) as Material;
		}

		//no process needed

	}
}
