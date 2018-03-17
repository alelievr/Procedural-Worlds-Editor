using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProceduralWorlds.Node;
using UnityEditor;
using ProceduralWorlds.Biomator;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiomeBlender))]
	public class NodeBiomeBlenderEditor : BaseNodeEditor
	{
		public NodeBiomeBlender			node;

		readonly BiomeBlendListDrawer	blendListDrawer = new BiomeBlendListDrawer();

		const string					updateBiomeMapKey = "BiomeBlender";

		[System.NonSerialized]
		bool	updateBiomeMap = false;

		public override void OnNodeEnable()
		{
			node = target as NodeBiomeBlender;
			
			delayedChanges.BindCallback(updateBiomeMapKey, (unused) => {
				BiomeData data = node.GetBiomeData();

				node.FillBiomeMap(data);
				updateBiomeMap = true;

				NotifyReload();
			});
			
			blendListDrawer.OnEnable(node.blendList);
		}

		public override void OnNodeGUI()
		{
			BiomeData biomeData = node.GetBiomeData();

			if (biomeData == null)
			{
				EditorGUILayout.LabelField("biomes not connected !");
				return ;
			}
			else
			{
				EditorGUIUtility.labelWidth = 120;
				EditorGUI.BeginChangeCheck();
				node.biomeBlendPercent = PWGUI.Slider("Biome blend ratio: ", node.biomeBlendPercent, 0f, .5f);
				if (EditorGUI.EndChangeCheck())
					delayedChanges.UpdateValue(updateBiomeMapKey);
				node.blendList.UpdateIfNeeded(biomeData);

				EditorGUI.BeginChangeCheck();
				blendListDrawer.OnGUI(biomeData);
				if (EditorGUI.EndChangeCheck())
					delayedChanges.UpdateValue(updateBiomeMapKey);
			}

			if (biomeData != null)
				PWGUI.BiomeMap2DPreview(biomeData);
			else
				EditorGUILayout.LabelField("no biome data");
			
			if (updateBiomeMap)
			{
				PWGUI.SetUpdateForField(PWGUIFieldType.BiomeMapPreview, 0, true);
				updateBiomeMap = false;
			}

			var biomeCoverage = biomeData.biomeSwitchGraph.GetBiomeCoverage();

			bool biomeCoverageError = biomeCoverage.Any(b => b.Value > 0 && b.Value < 1);

			GUIStyle biomeCoverageFoloutStyle = (biomeCoverageError) ? Styles.errorFoldout : EditorStyles.foldout;

			node.biomeCoverageRecap = EditorGUILayout.Foldout(node.biomeCoverageRecap, "Biome coverage recap", biomeCoverageFoloutStyle);
			
			if (node.biomeCoverageRecap)
			{
				if (biomeData != null && biomeData.biomeSwitchGraph != null)
				{
					foreach (var biomeCoverageKP in biomeCoverage)
						if (biomeCoverageKP.Value > 0)
						{
							string paramName = biomeData.GetBiomeKey(biomeCoverageKP.Key);
							EditorGUILayout.LabelField(paramName, (biomeCoverageKP.Value * 100).ToString("F2") + "%");
						}
				}
				else
					EditorGUILayout.LabelField("Null biome data/biome tree");
			}
		}

		public override void OnNodePreProcess()
		{
			node.BuildBiomeSwitchGraph();
			var biomeData = node.GetBiomeData();

			if (biomeData != null && biomeData.biomeSwitchGraph.isBuilt)
				node.FillBiomeMap(biomeData);
		}
	}
}