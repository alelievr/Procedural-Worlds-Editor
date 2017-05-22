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

		//TODO: remove this output and set one BiomeData as output.
		[PWOutput("biome datas")]
		[PWOffset(5)]
		public BiomeData	biomeData;

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
			externalName = "Water level";
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
			GUILayout.Space(GUI.skin.label.lineHeight * 2f);
				
			if (biomeData != null)
			{
				PWBiomeUtils.DrawBiomeInfos(biomeData);
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

		public override void OnNodeProcess()
		{
			if (biomeData == null || forceReload)
			{
				biomeData = new BiomeData();

				biomeData.biomeTree.BuildTree(this);
			}
			
			if (needUpdate)
			{
				biomeData.terrain = terrainNoise as Sampler2D;
				biomeData.terrain3D = terrainNoise as Sampler3D;
				
				biomeData.waterLevel = waterLevel;

				if (terrainNoise.type == SamplerType.Sampler2D)
				{
					//terrain mapping
					biomeData.terrain = PWNoiseFunctions.Map(terrainNoise as Sampler2D, mapMin, mapMax, true);

					//waterHeight evaluation
					biomeData.waterHeight = new Sampler2D(terrainNoise.size, terrainNoise.step);
					biomeData.waterHeight.min = mapMin;
					biomeData.waterHeight.max = mapMax;
					(terrainNoise as Sampler2D).Foreach((x, y, val) => {
						biomeData.waterHeight[x, y] = waterLevel - val;
					});
				}
				else
					; //TODO
			}
		}

	}
}