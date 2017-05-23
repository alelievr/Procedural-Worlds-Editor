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
		public BiomeData	outputBiome;

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
				
			if (outputBiome != null)
			{
				PWBiomeUtils.DrawBiomeInfos(outputBiome);
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
			if (outputBiome == null || forceReload)
			{
				outputBiome = new BiomeData();

				outputBiome.biomeTree.BuildTree(this);
			}
			
			if (needUpdate)
			{
				outputBiome.terrain = terrainNoise as Sampler2D;
				outputBiome.terrain3D = terrainNoise as Sampler3D;
				
				outputBiome.waterLevel = waterLevel;

				if (terrainNoise != null && terrainNoise.type == SamplerType.Sampler2D)
				{
					//terrain mapping
					outputBiome.terrain = PWNoiseFunctions.Map(terrainNoise as Sampler2D, mapMin, mapMax, true);

					//waterHeight evaluation
					outputBiome.waterHeight = new Sampler2D(terrainNoise.size, terrainNoise.step);
					outputBiome.waterHeight.min = mapMin;
					outputBiome.waterHeight.max = mapMax;
					outputBiome.terrain.Foreach((x, y, val) => {
						outputBiome.waterHeight[x, y] = waterLevel - val;
					});
				}
				else
					; //TODO
			}
		}

	}
}