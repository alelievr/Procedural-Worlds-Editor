using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using System;

[CustomEditor(typeof(PWMainGraph))]
public class PWGraphCustomInspector : Editor {

	public override void OnInspectorGUI()
	{
		EditorGUILayout.LabelField("You can't edit graph datas from the inspector");
		if (GUILayout.Button("Open Graph editor"))
		{
			EditorWindow.GetWindow(typeof(PWMainGraphEditor)).Show();
		}
	}
}
[CustomEditor(typeof(PWBiomeGraph))]
public class PWBiomeCustomInspector : Editor {

	public override void OnInspectorGUI()
	{
		EditorGUILayout.LabelField("You can't edit graph datas from the inspector");
		if (GUILayout.Button("Open Graph editor"))
		{
			EditorWindow.GetWindow(typeof(PWBiomeGraphEditor)).Show();
		}
	}
}
