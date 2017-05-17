using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeWaterLevel : PWNode {

		[PWInput("Terrain Input")]
		public Sampler		terrainNoise;

		[PWOutput("terrestrial")]
		[PWOffset(5)]
		public BiomeData	terrestrialBiomeData;
		[PWOutput("aquatic")]
		[PWOffset(10)]
		public BiomeData	aquaticBiomeData;

		[SerializeField]
		float				waterLevel;
		[SerializeField]
		float				mapMin;
		[SerializeField]
		float				mapMax;

		Gradient			waterGradient;

		public override void OnNodeCreateOnce()
		{
			mapMin = 0;
			mapMax = 100;
			waterLevel = 50;
		}

		void UpdateGradient()
		{
			waterGradient = PWUtils.CreateGradient(GradientMode.Fixed, 
				new KeyValuePair< float, Color >(waterLevel / (mapMax - mapMin), Color.white),
				new KeyValuePair< float, Color >(1, Color.blue));
			PWGUI.SetGradientForField(0, waterGradient);
		}
		
		public override void OnNodeGUI()
		{
			GUILayout.Space(GUI.skin.label.lineHeight * 2.5f);
				
			if (terrestrialBiomeData != null)
			{
				PWBiomeUtils.DrawBiomeInfos(terrestrialBiomeData);
				EditorGUILayout.Separator();
			}

			EditorGUI.BeginChangeCheck();
			{
				EditorGUIUtility.labelWidth = 100;
				waterLevel = EditorGUILayout.FloatField("WaterLevel", waterLevel);
	
				if (terrainNoise != null)
				{
					if (terrainNoise.type == SamplerType.Sampler2D)
					{
						EditorGUILayout.LabelField("Map terrain values:");
						EditorGUILayout.BeginHorizontal();
						EditorGUIUtility.labelWidth = 30;
						mapMin = EditorGUILayout.FloatField("from", mapMin);
						mapMax = EditorGUILayout.FloatField("to", mapMax);
						EditorGUILayout.EndHorizontal();
	
						PWGUI.Sampler2DPreview(terrainNoise as Sampler2D, needUpdate, false, FilterMode.Point);
					}
					if (waterGradient == null || EditorGUI.EndChangeCheck())
						UpdateGradient();
				}
			}
		}

		public override void OnNodeProcess()
		{
			if (terrestrialBiomeData == null)
			{
				terrestrialBiomeData = new BiomeData();
				terrestrialBiomeData.waterLevel = waterLevel;
				aquaticBiomeData = terrestrialBiomeData;
			}
			if (needUpdate)
			{
				terrestrialBiomeData.terrain = terrainNoise as Sampler2D;
				terrestrialBiomeData.terrain3D = terrainNoise as Sampler3D;

				//TODO: map the noise values
				//TODO; compute the waterHeight with the mapped noise:
				if (terrainNoise.type == SamplerType.Sampler2D)
				{
					terrestrialBiomeData.waterHeight = new Sampler2D(terrainNoise.size, terrainNoise.step);
					PWNoiseFunctions.Map(terrainNoise as Sampler2D, mapMin, mapMax, true);
					(terrainNoise as Sampler2D).Foreach((x, y, val) => {
						terrestrialBiomeData.waterHeight[x, y] = waterLevel - val;
					});
				}
			}
		}

	}
}