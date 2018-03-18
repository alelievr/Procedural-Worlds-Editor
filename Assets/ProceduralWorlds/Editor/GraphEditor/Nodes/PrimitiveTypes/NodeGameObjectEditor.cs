using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Node;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeGameObject))]
	public class NodeGameObjectEditor : BaseNodeEditor
	{
		public NodeGameObject		node;

		public override void OnNodeEnable()
		{
			node = target as NodeGameObject;
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);

			node.outputGameObject = EditorGUILayout.ObjectField(node.outputGameObject, typeof(GameObject), false) as GameObject;

			Texture2D preview = AssetPreview.GetAssetPreview(node.outputGameObject);
			
			node.showPreview = EditorGUILayout.Foldout(node.showPreview, "preview");

			if (node.showPreview)
				EditorGUILayout.LabelField(new GUIContent(preview));
		}
	}
}