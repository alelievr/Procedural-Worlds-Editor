using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using PW.Core;
using UnityEditor;
using PW.Biomator;

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
			EditorGUI.BeginChangeCheck();
			node.materializerType = (MaterializerType)EditorGUILayout.EnumPopup("Materializer", node.materializerType);
			if (EditorGUI.EndChangeCheck())
			{
				PWTerrainPreviewManager.instance.UpdateTerrainMaterializer(node.materializerType);
			}

			if (finalTerrain == null)
			{
				EditorGUILayout.LabelField("Null terrain");
				return ;
			}
			
			if (!ValidateBlendedTerrainIntegrity())
			{
				EditorGUILayout.HelpBox("Null data found in the input blended biomes datas", MessageType.Error);
			}

			PWGUI.SamplerPreview("Final merged terrain", finalTerrain);

			node.biomeTerrainsFoldout = EditorGUILayout.Foldout(node.biomeTerrainsFoldout, "Show biome terrains");

			if (node.biomeTerrainsFoldout)
				foreach (var biome in node.inputBlendedTerrain.biomes)
					PWGUI.SamplerPreview(biome.name, biome.modifiedTerrain);

			if (update)
			{
				update = false;
				PWGUI.SetUpdateForField(PWGUIFieldType.Sampler2DPreview, 0, true);
			}
		}

		bool ValidateBlendedTerrainIntegrity()
		{
			BlendedBiomeTerrain	terrain = node.inputBlendedTerrain;

			if (terrain.biomeData == null || terrain.biomeData.biomeMap == null)
				return false;

			foreach (var biome in terrain.biomes)
			{
				if (biome == null)
					return false;
			}
			
			return true;
		}

		public override void OnNodePostProcess()
		{
			update = true;
		}

	}
}