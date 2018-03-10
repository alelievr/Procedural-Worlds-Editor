using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeConstant))]
	public class PWNodeConstantEditor : PWNodeEditor
	{
		public PWNodeConstant node;

		public override void OnNodeEnable()
		{
			node = target as PWNodeConstant;
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);

			EditorGUI.BeginChangeCheck();
			EditorGUIUtility.labelWidth = 80;
			node.selectedConstantType = (PWNodeConstant.ConstantType)EditorGUILayout.EnumPopup("output mode", node.selectedConstantType);
			if (EditorGUI.EndChangeCheck())
				node.UpdateConstantType();

			switch (node.selectedConstantType)
			{
				case PWNodeConstant.ConstantType.Int:
					node.outi = EditorGUILayout.IntField("Int", node.outi);
					break ;
				case PWNodeConstant.ConstantType.Float:
					node.outf = EditorGUILayout.FloatField("Float", node.outf);
					break ;
				case PWNodeConstant.ConstantType.Vector2:
					node.outv2 = EditorGUILayout.Vector2Field("Vec2", node.outv2);
					break ;
				case PWNodeConstant.ConstantType.Vector3:
					node.outv3 = EditorGUILayout.Vector3Field("Vec3", node.outv3);
					break ;
				case PWNodeConstant.ConstantType.Vector4:
					node.outv4 = EditorGUILayout.Vector4Field("Vec4", node.outv4);
					break ;
				default:
					break ;
			}
		}
	}
}