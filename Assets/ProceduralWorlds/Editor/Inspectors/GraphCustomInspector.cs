using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;
using System;

namespace ProceduralWorlds.Editor
{
	static class BaseGraphCustomInspectorUtils
	{
		public static void Open< T >(BaseGraph graph) where T : BaseGraphEditor
		{
			BaseGraphEditor editor = EditorWindow.GetWindow< T >();
			editor.LoadGraph(graph);
			editor.Show();
		}
	}
	
	[CustomEditor(typeof(WorldGraph))]
	public class WorldGraphCustomInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("You can't edit graph datas from the inspector");
			if (GUILayout.Button("Open Graph editor"))
				BaseGraphCustomInspectorUtils.Open< WorldGraphEditor >(target as BaseGraph);
		}
	}
	
	[CustomEditor(typeof(BiomeGraph))]
	public class BiomeGraphCustomInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("You can't edit graph datas from the inspector");
			if (GUILayout.Button("Open Graph editor"))
				BaseGraphCustomInspectorUtils.Open< BiomeGraphEditor >(target as BaseGraph);
		}
	}
}