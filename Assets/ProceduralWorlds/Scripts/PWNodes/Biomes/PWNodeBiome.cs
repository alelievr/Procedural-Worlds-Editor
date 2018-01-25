using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiome : PWNode
	{

		[PWInput]
		public BiomeData	inputBiomeData;

		[PWOutput]
		public Biome		outputBiome;

		[SerializeField]
		PWBiomeGraph		biomeGraph;

		Rect				biomeColorPreviewRect;

		public override void OnNodeCreation()
		{
			name = "Biome";
		}

		public override void OnNodeEnable()
		{
			outputBiome = new Biome();
		}

		public override void OnNodeGUI()
		{
			EditorGUILayout.LabelField("Biome: " + outputBiome.name);
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUIUtility.labelWidth = 40;
				EditorGUILayout.LabelField("id: " + outputBiome.id);
				EditorGUIUtility.labelWidth = 0;
				Rect colorRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));
				if (e.type == EventType.Repaint)
					biomeColorPreviewRect = colorRect;
				EditorGUI.BeginDisabledGroup(false);
				EditorGUIUtility.DrawColorSwatch(biomeColorPreviewRect, outputBiome.previewColor);
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Biome Graph reference");
			biomeGraph = EditorGUILayout.ObjectField(biomeGraph, typeof(PWBiomeGraph), false) as PWBiomeGraph;

			if (biomeGraph != null)
			{
				if (GUILayout.Button("Open " + biomeGraph.name))
					AssetDatabase.OpenAsset(biomeGraph);
			}
		}

		public override void OnNodeProcessOnce()
		{
			if (biomeGraph == null)
			{
				Debug.LogError("NUll biome graph when processing once a biome node");
				return ;
			}

			outputBiome.biomeDataReference = inputBiomeData;
			outputBiome.biomeGraph = biomeGraph;
		}

		public override void OnNodeProcess()
		{
			if (biomeGraph == null)
			{
				Debug.LogError("NUll biome graph when processing a biome node");
				return ;
			}

			//we set the biomeData to the biomeGraph so it can process
			biomeGraph.SetInput("inputBiomeData", inputBiomeData);
			
			outputBiome.biomeDataReference = inputBiomeData;
			outputBiome.biomeGraph = biomeGraph;
		}
		
	}
}