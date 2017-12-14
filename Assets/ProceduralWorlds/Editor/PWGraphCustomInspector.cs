using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using System;

static class PWGraphCustomInspectorUtils
{
	public static void Open< T >(PWGraph graph) where T : PWGraphEditor
	{
		PWGraphEditor editor = EditorWindow.GetWindow< T >();
		editor.LoadGraph(graph);
		editor.Show();
	}
}

[CustomEditor(typeof(PWMainGraph))]
public class PWGraphCustomInspector : Editor {

	public override void OnInspectorGUI()
	{
		EditorGUILayout.LabelField("You can't edit graph datas from the inspector");
		if (GUILayout.Button("Open Graph editor"))
			PWGraphCustomInspectorUtils.Open< PWMainGraphEditor >(target as PWGraph);
	}
}

[CustomEditor(typeof(PWBiomeGraph))]
public class PWBiomeCustomInspector : Editor {

	public override void OnInspectorGUI()
	{
		EditorGUILayout.LabelField("You can't edit graph datas from the inspector");
		if (GUILayout.Button("Open Graph editor"))
			PWGraphCustomInspectorUtils.Open< PWBiomeGraphEditor >(target as PWGraph);
	}
}
