using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;

public class PWNodeAdd : PWNode {

	[PWInput]
	public float	A;
	[PWInput]
	public float	B;
	[PWOutput("O")]
	public float	output;

	bool			intify = false;

	public override void OnNodeCreate()
	{
		name = "add";

		//override window width
		windowRect.width = 150;
	}

	public override void OnNodeGUI()
	{
		EditorGUIUtility.labelWidth = 100;
		A = EditorGUILayout.FloatField("input A", A);
		EditorGUIUtility.labelWidth = 100;
		B = EditorGUILayout.FloatField("input B", B);

		EditorGUIUtility.labelWidth = 100;
		intify = EditorGUILayout.Toggle("Integer round", intify);

		if (intify)
			output = Mathf.Round(A + B);
		else
			output = A + B;

		EditorGUILayout.LabelField(A + " + " + B + " = " + output);
	}
}
