using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeGameObject))]
	public class PWNodeGameObjectEditor : PWNodeEditor
	{
		public PWNodeGameObject		node;

		public override void OnNodeEnable()
		{
			node = target as PWNodeGameObject;
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);

			node.outputGameObject = EditorGUILayout.ObjectField(node.outputGameObject, typeof(GameObject), false) as GameObject;

			Texture2D preview = AssetPreview.GetAssetPreview(node.outputGameObject);
			if ((node.showPreview = EditorGUILayout.Foldout(node.showPreview, "preview")))
				EditorGUILayout.LabelField(new GUIContent(preview));
		}
	}
}