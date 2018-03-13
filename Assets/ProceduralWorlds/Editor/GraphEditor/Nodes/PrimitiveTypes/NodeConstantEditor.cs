using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Node;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeConstant))]
	public class NodeConstantEditor : NodeEditor
	{
		public NodeConstant node;

		public override void OnNodeEnable()
		{
			node = target as NodeConstant;
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);

			EditorGUI.BeginChangeCheck();
			EditorGUIUtility.labelWidth = 80;
			node.selectedConstantType = (NodeConstant.ConstantType)EditorGUILayout.EnumPopup("output mode", node.selectedConstantType);
			if (EditorGUI.EndChangeCheck())
				node.UpdateConstantType();

			switch (node.selectedConstantType)
			{
				case NodeConstant.ConstantType.Int:
					node.outi = EditorGUILayout.IntField("Int", node.outi);
					break ;
				case NodeConstant.ConstantType.Float:
					node.outf = EditorGUILayout.FloatField("Float", node.outf);
					break ;
				case NodeConstant.ConstantType.Vector2:
					node.outv2 = EditorGUILayout.Vector2Field("Vec2", node.outv2);
					break ;
				case NodeConstant.ConstantType.Vector3:
					node.outv3 = EditorGUILayout.Vector3Field("Vec3", node.outv3);
					break ;
				case NodeConstant.ConstantType.Vector4:
					node.outv4 = EditorGUILayout.Vector4Field("Vec4", node.outv4);
					break ;
				default:
					break ;
			}
		}
	}
}