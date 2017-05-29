using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeMesh : PWNode {

		[PWOutput("mesh")]
		public Mesh			outputMesh;

		GameObject			meshRenderObject;
		PWGUIObjectPreview	objectPreview = new PWGUIObjectPreview();
		Material			previewMaterial;

		[SerializeField]
		bool				showSceneHiddenObjects = false;
		[SerializeField]
		bool				displayPreview;

		public override void OnNodeCreate()
		{
			renamable = true;
			externalName = "mesh";

			meshRenderObject = new GameObject("MeshNodeRenderer", typeof(MeshRenderer), typeof(MeshFilter));
			previewMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
			meshRenderObject.GetComponent< MeshRenderer >().material = previewMaterial;
			objectPreview.Initialize();
			UpdateMeshRenderer();
			objectPreview.UpdateObjects(meshRenderObject);
			UpdateHideFlags();
		}

		void UpdateHideFlags()
		{
			if (showSceneHiddenObjects)
				meshRenderObject.hideFlags = HideFlags.DontSave;
			else
				meshRenderObject.hideFlags = HideFlags.HideAndDontSave;
		}

		void UpdateMeshRenderer()
		{
			meshRenderObject.GetComponent< MeshFilter >().sharedMesh = outputMesh;
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);
			EditorGUI.BeginChangeCheck();
			outputMesh = EditorGUILayout.ObjectField(outputMesh, typeof(Mesh), false) as Mesh;
			if (EditorGUI.EndChangeCheck())
			{
				UpdateMeshRenderer();
				objectPreview.UpdateObjects(meshRenderObject);
			}

			EditorGUI.BeginChangeCheck();
			showSceneHiddenObjects = EditorGUILayout.Toggle("Show scene hidden objects", showSceneHiddenObjects);
			if (EditorGUI.EndChangeCheck())
				UpdateHideFlags();

			if ((displayPreview = EditorGUILayout.Foldout(displayPreview, "mesh preview")))
			{
				EditorGUIUtility.labelWidth = 75;
				
				EditorGUI.BeginChangeCheck();
				previewMaterial = EditorGUILayout.ObjectField("preview Mat", previewMaterial, typeof(Material), false) as Material;
				if (EditorGUI.EndChangeCheck())
					meshRenderObject.GetComponent< MeshRenderer >().material = previewMaterial;
	
				objectPreview.Render();
			}
		}

		public override void OnNodeDisable()
		{
			//this will destroy all loaded objects too
			objectPreview.Cleanup();
		}

		//no process needed
	}
}
