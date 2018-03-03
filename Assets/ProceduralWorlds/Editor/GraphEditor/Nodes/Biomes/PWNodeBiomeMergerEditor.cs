using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using PW.Core;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeBiomeMerger))]
	public class PWNodeBiomeMergerEditor : PWNodeEditor
	{
		public PWNodeBiomeMerger node;

		[System.NonSerialized]
		bool					update;

		public override void OnNodeEnable()
		{
			node = target as PWNodeBiomeMerger;
		}

		public override void OnNodeGUI()
		{
			Sampler finalTerrain = null;
			
			if (node.mergedBiomeTerrain != null && node.mergedBiomeTerrain.mergedTerrain != null)
				finalTerrain = node.mergedBiomeTerrain.mergedTerrain;
			
			EditorGUIUtility.labelWidth = 80;
			mainGraphRef.materializerType = (MaterializerType)EditorGUILayout.EnumPopup("Materializer", mainGraphRef.materializerType);

			if (finalTerrain == null)
			{
				EditorGUILayout.LabelField("Null terrain");
				return ;
			}

			PWGUI.SamplerPreview("Final merged terrain", finalTerrain);

			node.biomeTerrainsFoldout = EditorGUILayout.Foldout(node.biomeTerrainsFoldout, "Show biome terrains");

			if (node.biomeTerrainsFoldout)
				foreach (var biome in node.inputBlendedTerrain.biomes)
					PWGUI.SamplerPreview(biome.name, biome.modifiedTerrain);

			if (update)
			{
				update = false;
				PWGUI.SetUpdateForField(0, true);
			}
		}

		public override void OnNodePostProcess()
		{
			update = true;
		}

	}
}