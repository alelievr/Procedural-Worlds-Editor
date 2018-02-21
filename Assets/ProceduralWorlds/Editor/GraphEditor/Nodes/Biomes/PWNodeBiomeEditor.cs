using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using PW.Biomator;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeBiome))]
	public class PWNodeBiomeEditor : PWNodeEditor
	{
		public PWNodeBiome		node;

		Rect				biomeColorPreviewRect;

		public override void OnNodeEnable()
		{
			node = target as PWNodeBiome;
		}

		public override void OnNodeGUI()
		{
			EditorGUILayout.LabelField("Biome: " + node.outputBiome.name);
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUIUtility.labelWidth = 40;
				EditorGUILayout.LabelField("id: " + node.outputBiome.id);
				EditorGUIUtility.labelWidth = 0;
				Rect colorRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));
				if (e.type == EventType.Repaint)
					biomeColorPreviewRect = colorRect;
				EditorGUI.BeginDisabledGroup(false);
				EditorGUIUtility.DrawColorSwatch(biomeColorPreviewRect, node.outputBiome.previewColor);
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Biome Graph reference");
			EditorGUI.BeginChangeCheck();
			node.biomeGraph = EditorGUILayout.ObjectField(node.biomeGraph, typeof(PWBiomeGraph), false) as PWBiomeGraph;
			if (EditorGUI.EndChangeCheck())
				NotifyReload();

			if (node.biomeGraph != null)
			{
				if (GUILayout.Button("Open " + node.biomeGraph.name))
					AssetDatabase.OpenAsset(node.biomeGraph);
			}
		}

		public override void OnNodeDisable()
		{
			
		}
	}
}