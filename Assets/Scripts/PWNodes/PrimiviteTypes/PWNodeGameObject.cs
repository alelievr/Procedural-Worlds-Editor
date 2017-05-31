using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeGameObject : PWNode {

		[PWOutput("object")]
		public GameObject		outputGameObject;
		PWGUIObjectPreview		objectPreview = new PWGUIObjectPreview();

		[SerializeField]
		GameObject				previewGO;
		[SerializeField]
		bool					showPreview;
		[SerializeField]
		bool					showSceneHiddenObjects = false;

		public override void OnNodeCreate()
		{
			externalName = "GameObject";
			renamable = true;
			objectPreview.UpdateObjects(previewGO);

			UpdateGameObject();
		}

		void UpdateGameObject()
		{
			if (outputGameObject == null || objectPreview == null) 
				return ;
			
			DestroyImmediate(previewGO);

			previewGO = GameObject.Instantiate(outputGameObject, Vector3.zero, Quaternion.identity);
			Debug.Log("instanciated new GO: " + previewGO);
			
			if (showSceneHiddenObjects)
				previewGO.hideFlags = HideFlags.DontSave;
			else
				previewGO.hideFlags = HideFlags.HideAndDontSave;
			
			objectPreview.UpdateObjects(previewGO);
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);

			EditorGUI.BeginChangeCheck();
			{
				outputGameObject = EditorGUILayout.ObjectField(outputGameObject, typeof(GameObject), false) as GameObject;
				showSceneHiddenObjects = EditorGUILayout.Toggle("Show scene hidden objects", showSceneHiddenObjects);
			}
			if (EditorGUI.EndChangeCheck())
				UpdateGameObject();

			if ((showPreview = EditorGUILayout.Foldout(showPreview, "preview")))
				objectPreview.Render();
		}

		public override void OnNodeDisable()
		{
			objectPreview.Cleanup();
		}

		//no process needed
	}
}
