using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using PW.Core;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeWaterLevel))]
	public class PWNodeWaterLevelEditor : PWNodeEditor
	{
		public PWNodeWaterLevel node;
		
		Gradient			waterGradient;
		
		bool				updateWaterPreview = false;
		
		const string		delayedUpdateKey = "delayedUpdate";

		public override void OnNodeEnable()
		{
			node = target as PWNodeWaterLevel;
			
			delayedChanges.BindCallback(delayedUpdateKey, (unused) => {
				node.UpdateWaterMap();
				NotifyReload();
			});
		}
		
		void UpdateGradient()
		{
			waterGradient = PWUtils.CreateGradient(GradientMode.Fixed, 
				new KeyValuePair< float, Color >(node.waterLevel / (node.mapMax - node.mapMin), Color.blue),
				new KeyValuePair< float, Color >(1, Color.white));
			PWGUI.SetGradientForField(0, waterGradient);

			updateWaterPreview = true;
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(GUI.skin.label.lineHeight * 2f);
				
			if (node.outputBiome != null)
			{
				// BiomeUtils.DrawBiomeInfos(outputBiome);
				// EditorGUILayout.Separator();
			}

			EditorGUIUtility.labelWidth = 100;

			EditorGUI.BeginChangeCheck();
			{
				node.waterLevel = EditorGUILayout.FloatField("WaterLevel", node.waterLevel);
			}
			if (EditorGUI.EndChangeCheck())
			{
				delayedChanges.UpdateValue(delayedUpdateKey);
				updateWaterPreview = true;
			}
	
			EditorGUI.BeginChangeCheck();
			{
				if (node.terrainNoise != null)
				{
					if (node.terrainNoise.type == SamplerType.Sampler2D)
					{
						EditorGUILayout.LabelField("Map terrain values:");
						EditorGUILayout.BeginHorizontal();
						EditorGUIUtility.labelWidth = 30;
						node.mapMin = EditorGUILayout.FloatField("from", node.mapMin);
						node.mapMax = EditorGUILayout.FloatField("to", node.mapMax);
						EditorGUILayout.EndHorizontal();
	
						PWGUI.Sampler2DPreview(node.terrainNoise as Sampler2D, false, FilterMode.Point);
					}
					else
					{
						EditorGUILayout.LabelField("TODO: water level 3D");
					}
					if (waterGradient == null || EditorGUI.EndChangeCheck() || updateWaterPreview)
					{
						UpdateGradient();
						delayedChanges.UpdateValue(delayedUpdateKey);
						updateWaterPreview = false;
					}
				}
			}
		}

	}
}