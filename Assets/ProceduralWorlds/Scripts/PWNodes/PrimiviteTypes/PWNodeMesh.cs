using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeMesh : PWNode
	{

		[PWOutput("mesh")]
		public Mesh			outputMesh;

		PWGUIMeshPreview	meshPreview;
		Material			previewMaterial;

		[SerializeField]
		bool				showSceneHiddenObjects = false;
		[SerializeField]
		bool				displayPreview;

		public override void OnNodeCreation()
		{
			renamable = true;
			name = "mesh";

			previewMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
		}

		public override void OnNodeEnable()
		{
			meshPreview = new PWGUIMeshPreview();
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);
			outputMesh = EditorGUILayout.ObjectField(outputMesh, typeof(Mesh), false) as Mesh;

			showSceneHiddenObjects = EditorGUILayout.Toggle("Show scene hidden objects", showSceneHiddenObjects);

			if ((displayPreview = EditorGUILayout.Foldout(displayPreview, "preview")))
			{
				EditorGUIUtility.labelWidth = 75;
				
				previewMaterial = EditorGUILayout.ObjectField("preview Mat", previewMaterial, typeof(Material), false) as Material;
	
	 			meshPreview.Render(outputMesh, previewMaterial);
			}
		}

		public override void OnNodeDisable()
		{
			meshPreview.Cleanup();
		}
	}
}
