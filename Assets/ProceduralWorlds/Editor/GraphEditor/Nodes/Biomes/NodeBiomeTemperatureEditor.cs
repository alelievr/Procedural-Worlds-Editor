using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Node;
using ProceduralWorlds.Biomator;
using UnityEditor;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiomeTemperature))]
	public class NodeBiomeTemperatureEditor : BaseNodeEditor
	{
		public NodeBiomeTemperature node;

		readonly string	graphReloadKey = "NodeBiomeTemperature";
		
		Gradient	temperatureGradient;

		[System.NonSerialized]
		bool		guiInitialized;

		public override void OnNodeEnable()
		{
			node = target as NodeBiomeTemperature;
			
			temperatureGradient = Utils.CreateGradient(
				new KeyValuePair< float, Color >(0f, Color.blue),
				new KeyValuePair< float, Color >(.25f, Color.cyan),
				new KeyValuePair< float, Color >(.5f, Color.yellow),
				new KeyValuePair< float, Color >(.75f, ColorUtils.orange),
				new KeyValuePair< float, Color >(1f, Color.red)
			);
			
			delayedChanges.BindCallback(graphReloadKey, (unused) => {
				NotifyReload();
			});
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(GUI.skin.label.lineHeight * 3);

			EditorGUI.BeginChangeCheck();
			{
				node.terrainHeightMultiplier = PWGUI.Slider("height multiplier: ", node.terrainHeightMultiplier, -1, 1);
				node.waterMultiplier = PWGUI.Slider("water multiplier: ", node.waterMultiplier, -1, 1);
	
				EditorGUILayout.LabelField("temperature limits:");
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUIUtility.labelWidth = 25;
					node.minTemperature = EditorGUILayout.FloatField("from", node.minTemperature);
					node.maxTemperature = EditorGUILayout.FloatField("to", node.maxTemperature);
				}
				EditorGUILayout.EndHorizontal();
				EditorGUIUtility.labelWidth = 120;
				if (node.internalTemperatureMap)
					node.averageTemperature = EditorGUILayout.FloatField("average temperature", node.averageTemperature);
				else
				{
					EditorGUILayout.LabelField("Input temperature map range");
					using (DefaultGUISkin.Get())
						EditorGUILayout.MinMaxSlider(ref node.minTemperatureMapInput, ref node.maxTemperatureMapInput, node.minTemperature, node.maxTemperature);
					EditorGUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth = 25;
					node.minTemperatureMapInput = EditorGUILayout.FloatField("Min", node.minTemperatureMapInput);
					node.maxTemperatureMapInput = EditorGUILayout.FloatField("Max", node.maxTemperatureMapInput);
					EditorGUILayout.EndHorizontal();
				}
			}
			if (EditorGUI.EndChangeCheck())
			{
				node.UpdateTemperatureMap();
				PWGUI.SetUpdateForField(PWGUIFieldType.Sampler2DPreview, 0, true);
				delayedChanges.UpdateValue(graphReloadKey);
			}
			
			if (node.localTemperatureMap != null)
				PWGUI.Sampler2DPreview(node.localTemperatureMap as Sampler2D, false, FilterMode.Point);
			
			if (!guiInitialized)
			{
				PWGUI.SetGradientForField(PWGUIFieldType.Sampler2DPreview, 0, temperatureGradient);
				PWGUI.SetDebugForField(PWGUIFieldType.Sampler2DPreview, 0, true);
				guiInitialized = true;
			}
		}

		public override void OnNodePostProcess()
		{
			PWGUI.SetUpdateForField(PWGUIFieldType.Sampler2DPreview, 0, true);
		}

	}
}