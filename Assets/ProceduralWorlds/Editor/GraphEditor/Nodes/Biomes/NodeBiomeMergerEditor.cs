using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Nodes;
using ProceduralWorlds.Core;
using UnityEditor;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiomeMerger))]
	public class NodeBiomeMergerEditor : BaseNodeEditor
	{
		public NodeBiomeMerger node;

		public override void OnNodeEnable()
		{
			node = target as NodeBiomeMerger;
		}

		public override void OnNodeGUI()
		{
			Sampler finalTerrain = null;

			PWGUI.SpaceSkipAnchors();
			
			if (node.mergedBiomeTerrain != null && node.mergedBiomeTerrain.mergedTerrain != null)
				finalTerrain = node.mergedBiomeTerrain.mergedTerrain;
			
			EditorGUIUtility.labelWidth = 80;
			EditorGUI.BeginChangeCheck();
			node.materializerType = (MaterializerType)EditorGUILayout.EnumPopup("Materializer", node.materializerType);
			if (EditorGUI.EndChangeCheck())
			{
				TerrainPreviewManager.instance.UpdateTerrainMaterializer(node.materializerType);
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
				foreach (var biome in node.inputBlendedTerrain.biomePerIds)
					PWGUI.SamplerPreview(biome.Value.name, biome.Value.modifiedTerrain);
		}

		bool ValidateBlendedTerrainIntegrity()
		{
			BlendedBiomeTerrain	terrain = node.inputBlendedTerrain;

			if (terrain.biomeData == null || terrain.biomeData.biomeMap == null)
				return false;

			foreach (var biome in terrain.biomePerIds)
			{
				if (biome.Value == null)
					return false;
			}
			
			return true;
		}

	}
}