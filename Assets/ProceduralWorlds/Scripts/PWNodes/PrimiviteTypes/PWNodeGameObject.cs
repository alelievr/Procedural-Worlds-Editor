using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeGameObject : PWNode
	{

		[PWOutput("object")]
		public GameObject		outputGameObject;

		[SerializeField]
		bool					showPreview;
		[SerializeField]
		bool					showSceneHiddenObjects = false;

		public override void OnNodeCreation()
		{
			name = "GameObject";
			renamable = true;

			UpdateGameObject();
		}

		void UpdateGameObject()
		{
			if (outputGameObject == null) 
				return ;
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

			Texture2D preview = AssetPreview.GetAssetPreview(outputGameObject);
			if ((showPreview = EditorGUILayout.Foldout(showPreview, "preview")))
				EditorGUILayout.LabelField(new GUIContent(preview));
		}

		//no process needed
	}
}
