using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeWaterLevel : PWNode {

		[PWInput("Terrain Input")]
		[PWOffset(5)]
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
			GUILayout.Space(GUI.skin.label.lineHeight * 4f);
				
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
					else
					{
						EditorGUILayout.LabelField("TODO: water level 3D");
					}
					if (waterGradient == null || EditorGUI.EndChangeCheck())
						UpdateGradient();
				}
			}
		}

		public override void OnNodePreProcess()
		{
			if (terrestrialBiomeData == null)
			{
				terrestrialBiomeData = new BiomeData();
				aquaticBiomeData = terrestrialBiomeData;

				terrestrialBiomeData.biomeTree.BuildTree(this);
			}
		}

		public override void OnNodeProcess()
		{
			if (needUpdate)
			{
				terrestrialBiomeData.terrain = terrainNoise as Sampler2D;
				terrestrialBiomeData.terrain3D = terrainNoise as Sampler3D;
				
				terrestrialBiomeData.waterLevel = waterLevel;

				if (terrainNoise.type == SamplerType.Sampler2D)
				{
					//terrain mapping
					terrestrialBiomeData.terrain = PWNoiseFunctions.Map(terrainNoise as Sampler2D, mapMin, mapMax, true);

					//waterHeight evaluation
					terrestrialBiomeData.waterHeight = new Sampler2D(terrainNoise.size, terrainNoise.step);
					(terrainNoise as Sampler2D).Foreach((x, y, val) => {
						terrestrialBiomeData.waterHeight[x, y] = waterLevel - val;
					});
				}
				else
					; //TODO
			}
		}

	}
}