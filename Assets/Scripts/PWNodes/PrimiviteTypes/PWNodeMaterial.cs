using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeMaterial : PWNode {

		[PWOutput("Material")]
		public Material			outputMaterial;
		PWGUIObjectPreview		objectPreview = new PWGUIObjectPreview();

		GameObject				previewGO;

		[SerializeField]
		bool					showPreview;
		[SerializeField]
		bool					showSceneHiddenObjects = false;

		public override void OnNodeCreate()
		{
			renamable = true;
			externalName = "Material";
			
			previewGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			objectPreview.Initialize(30, CameraClearFlags.Color);
			objectPreview.UpdateObjects(previewGO);
			if (outputMaterial != null)
				UpdatePreviewMaterial();
		}

		void		UpdatePreviewMaterial()
		{
			previewGO.GetComponent< MeshRenderer >().material = outputMaterial;
		}
		
		void UpdateHideFlags()
		{
			if (previewGO == null)
				return ;
			
			if (showSceneHiddenObjects)
				previewGO.hideFlags = HideFlags.DontSave;
			else
				previewGO.hideFlags = HideFlags.HideAndDontSave;
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);

			EditorGUI.BeginChangeCheck();
			outputMaterial = EditorGUILayout.ObjectField(outputMaterial, typeof(Material), false) as Material;
			if (EditorGUI.EndChangeCheck())
				UpdatePreviewMaterial();
				
			EditorGUI.BeginChangeCheck();
			showSceneHiddenObjects = EditorGUILayout.Toggle("Show scene hidden objects", showSceneHiddenObjects);
			if (EditorGUI.EndChangeCheck())
				UpdateHideFlags();
			
			if ((showPreview = EditorGUILayout.Foldout(showPreview, "show preview")))
				objectPreview.Render();
		}
		
		public override void OnNodeDisable()
		{
			objectPreview.Cleanup();
		}

		//no process needed

	}
}
