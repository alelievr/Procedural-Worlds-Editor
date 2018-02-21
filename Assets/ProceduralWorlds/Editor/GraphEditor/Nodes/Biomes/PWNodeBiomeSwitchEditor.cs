using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Node;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeBiomeSwitch))]
	public class PWNodeBiomeSwitchEditor : PWNodeEditor
	{
		public PWNodeBiomeSwitch node;

		const string			delayedUpdateKey = "BiomeSwitchListUpdate";

		public override void OnNodeEnable()
		{
			node = target as PWNodeBiomeSwitch;

			delayedChanges.BindCallback(delayedUpdateKey, (unused) => { NotifyReload(); });

			node.switchList.OnBiomeDataAdded = (unused) => { delayedChanges.UpdateValue(delayedUpdateKey, null); };
			node.switchList.OnBiomeDataModified = (unused) => { node.alreadyModified = true; node.switchList.UpdateBiomeRepartitionPreview(inputBiome); delayedChanges.UpdateValue(delayedUpdateKey, null); };
			node.switchList.OnBiomeDataRemoved = () => { delayedChanges.UpdateValue(delayedUpdateKey, null); };
			node.switchList.OnBiomeDataReordered = () => { delayedChanges.UpdateValue(delayedUpdateKey, null); };
		}

		public override void OnNodeGUI()
		{
			//return if input biome is null
			if (node.inputBiome == null)
			{
				EditorGUILayout.LabelField("null biome input !");
				return ;
			}

			//display popup field to choose the switch source
			EditorGUI.BeginChangeCheck();
			{
				EditorGUIUtility.labelWidth = 80;
				node.selectedBiomeSamplerName = EditorGUILayout.Popup("switch parameter", selectedBiomeSamplerName, samplerNames);
				node.samplerName = node.samplerNames[node.selectedBiomeSamplerName];
			}
			if (EditorGUI.EndChangeCheck())
			{
				node.CheckForBiomeSwitchErrors();
				node.UpdateSwitchMode();
			}

			EditorGUILayout.LabelField((node.currentSampler != null) ? "min: " + node.relativeMin + ", max: " + node.relativeMax : "");

			if (node.error)
			{
				Rect errorRect = EditorGUILayout.GetControlRect(false, GUI.skin.label.lineHeight * 3.5f);
				EditorGUI.LabelField(errorRect, node.errorString);
				return ;
			}

			//TODO: drawers
			// node.switchList.OnGUI(node.inputBiome);
		}

		public override void OnNodeDisable()
		{
			
		}
	}
}