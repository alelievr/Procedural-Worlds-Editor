using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using PW.Biomator;
using UnityEditor;
using PW.Core;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeBiomeTemperature))]
	public class PWNodeBiomeTemperatureEditor : PWNodeEditor
	{
		public PWNodeBiomeTemperature node;

		string		delayedTemperatureKey = "PWNodeBiomeTemperature";
		
		Gradient	temperatureGradient;

		[System.NonSerialized]
		bool		guiInitialized;

		public override void OnNodeEnable()
		{
			node = target as PWNodeBiomeTemperature;
			
			temperatureGradient = PWUtils.CreateGradient(
				new KeyValuePair< float, Color >(0f, Color.blue),
				new KeyValuePair< float, Color >(.25f, Color.cyan),
				new KeyValuePair< float, Color >(.5f, Color.yellow),
				new KeyValuePair< float, Color >(.75f, PWColor.orange),
				new KeyValuePair< float, Color >(1f, Color.red)
			);
			
			delayedChanges.BindCallback(delayedTemperatureKey, (unused) => {
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
					EditorGUIUtility.labelWidth = 30;
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
				}
			}
			if (EditorGUI.EndChangeCheck())
				node.UpdateTemperatureMap();
			
			if (node.localTemperatureMap != null)
				PWGUI.Sampler2DPreview(node.localTemperatureMap as Sampler2D, false, FilterMode.Point);
			
			if (node.updateTemperatureMap)
			{
				PWGUI.SetUpdateForField(PWGUIFieldType.Sampler2DPreview, 0, true);
				node.updateTemperatureMap = false;
			}

			if (!guiInitialized)
			{
				PWGUI.SetGradientForField(PWGUIFieldType.Sampler2DPreview, 0, temperatureGradient);
				PWGUI.SetDebugForField(PWGUIFieldType.Sampler2DPreview, 0, true);
				guiInitialized = true;
			}
		}

	}
}