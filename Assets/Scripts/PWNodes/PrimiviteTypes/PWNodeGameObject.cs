using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeGameObject : PWNode {

		[PWOutput("object")]
		public GameObject		outputGameObject;
		PWGUIObjectPreview		objectPreview = new PWGUIObjectPreview();

		[SerializeField]
		bool					previewGO;
		[SerializeField]
		bool					showSceneHiddenObjects = false;

		public override void OnNodeCreate()
		{
			externalName = "GameObject";
			renamable = true;
			objectPreview.Initialize();
			objectPreview.UpdateObjects(outputGameObject);
		}

		void UpdateHideFlags()
		{
			if (outputGameObject == null)
				return ;
			
			if (showSceneHiddenObjects)
				outputGameObject.hideFlags = HideFlags.DontSave;
			else
				outputGameObject.hideFlags = HideFlags.HideAndDontSave;
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);

			outputGameObject = EditorGUILayout.ObjectField(outputGameObject, typeof(GameObject), false) as GameObject;

			EditorGUI.BeginChangeCheck();
			showSceneHiddenObjects = EditorGUILayout.Toggle("Show scene hidden objects", showSceneHiddenObjects);
			if (EditorGUI.EndChangeCheck())
				UpdateHideFlags();

			if ((previewGO = EditorGUILayout.Foldout(previewGO, "show preview")))
			{
				objectPreview.Render();
			}
		}

		public override void OnNodeDisable()
		{
			objectPreview.Cleanup();
		}

		//no process needed
	}
}
