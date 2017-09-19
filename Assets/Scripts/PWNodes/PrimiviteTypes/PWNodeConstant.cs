using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeConstant : PWNode {

		enum ConstantType
		{
			Int,
			Float,
			Vector2,
			Vector3,
			Vector4,
		}

		[PWOutput("int")]
		public int		outi;

		[PWOutput("float")]
		public float	outf;

		[PWOutput("Vector2")]
		public Vector2	outv2;

		[PWOutput("Vector3")]
		public Vector3	outv3;

		[PWOutput("Vector4")]
		public Vector4	outv4;

		[SerializeField]
		ConstantType		selectedConstantType = ConstantType.Float;

		static Dictionary< ConstantType, string > properties = new Dictionary< ConstantType, string >() {
			{ConstantType.Int, "outi"},
			{ConstantType.Float, "outf"},
			{ConstantType.Vector2, "outv2"},
			{ConstantType.Vector3, "outv3"},
			{ConstantType.Vector4, "outv4"},
		};

		public override void OnNodeCreation()
		{
			renamable = true;
			name = "Constant";

			UpdateConstantType();
		}

		void			UpdateConstantType()
		{
			foreach (var propKp in properties)
				if (propKp.Key == selectedConstantType)
					SetAnchorVisibility(propKp.Value, PWVisibility.Visible);
				else
				{
					SetAnchorVisibility(propKp.Value, PWVisibility.Gone);
					RemoveAllLinksFromAnchor(propKp.Value);
				}
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);

			EditorGUI.BeginChangeCheck();
			EditorGUIUtility.labelWidth = 80;
			selectedConstantType = (ConstantType)EditorGUILayout.EnumPopup("output mode", selectedConstantType);
			if (EditorGUI.EndChangeCheck())
				UpdateConstantType();

			switch (selectedConstantType)
			{
				case ConstantType.Int:
					outi = EditorGUILayout.IntField("Int", outi);
					break ;
				case ConstantType.Float:
					outf = EditorGUILayout.FloatField("Float", outf);
					break ;
				case ConstantType.Vector2:
					outv2 = EditorGUILayout.Vector2Field("Vec2", outv2);
					break ;
				case ConstantType.Vector3:
					outv3 = EditorGUILayout.Vector3Field("Vec3", outv3);
					break ;
				case ConstantType.Vector4:
					outv4 = EditorGUILayout.Vector4Field("Vec4", outv4);
					break ;
			}
		}

		//no process needed
	}
}
