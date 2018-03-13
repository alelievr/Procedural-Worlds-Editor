using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Node;
using ProceduralWorlds.Biomator;
using UnityEditor;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiome))]
	public class NodeBiomeEditor : NodeEditor
	{
		public NodeBiome		node;

		Rect				biomeColorPreviewRect;

		public override void OnNodeEnable()
		{
			node = target as NodeBiome;
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
			node.biomeGraph = EditorGUILayout.ObjectField(node.biomeGraph, typeof(BiomeGraph), false) as BiomeGraph;
			if (EditorGUI.EndChangeCheck())
				NotifyReload();

			if (node.biomeGraph != null)
			{
				if (GUILayout.Button("Open " + node.biomeGraph.name))
					AssetDatabase.OpenAsset(node.biomeGraph);
			}
		}
	}
}