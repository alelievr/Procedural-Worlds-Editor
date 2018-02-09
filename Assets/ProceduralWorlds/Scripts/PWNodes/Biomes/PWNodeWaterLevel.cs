using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeWaterLevel : PWNode
	{

		[PWInput("Terrain input")]
		[PWOffset(5)]
		public Sampler		terrainNoise;

		[PWOutput("Biome datas")]
		[PWOffset(5)]
		public BiomeData	outputBiome;

		[SerializeField]
		float				waterLevel;
		[SerializeField]
		float				mapMin;
		[SerializeField]
		float				mapMax;

		Gradient			waterGradient;

		const string		delayedUpdateKey = "delayedUpdate";

		public override void OnNodeCreation()
		{
			mapMin = 0;
			mapMax = 100;
			waterLevel = 50;
			name = "Water level";
		}

		public override void OnNodeEnable()
		{
			outputBiome = new BiomeData();

			outputBiome.isWaterless = false;
			outputBiome.biomeTreeStartPoint = this;
			outputBiome.biomeSwitchGraphStartPoint = this;

			delayedChanges.BindCallback(delayedUpdateKey, (unused) => {
				NotifyReload();
			});
		}

		public override void OnNodeDisable()
		{
			
		}

		void UpdateGradient()
		{
			waterGradient = PWUtils.CreateGradient(GradientMode.Fixed, 
				new KeyValuePair< float, Color >(waterLevel / (mapMax - mapMin), Color.blue),
				new KeyValuePair< float, Color >(1, Color.white));
			PWGUI.SetGradientForField(0, waterGradient);
		}
		
		public override void OnNodeGUI()
		{
			bool updateWaterPreview = false;

			GUILayout.Space(GUI.skin.label.lineHeight * 2f);
				
			if (outputBiome != null)
			{
				// BiomeUtils.DrawBiomeInfos(outputBiome);
				// EditorGUILayout.Separator();
			}

			EditorGUIUtility.labelWidth = 100;

			EditorGUI.BeginChangeCheck();
				waterLevel = EditorGUILayout.FloatField("WaterLevel", waterLevel);
			if (EditorGUI.EndChangeCheck())
			{
				delayedChanges.UpdateValue(delayedUpdateKey);
				updateWaterPreview = true;
			}
	
			EditorGUI.BeginChangeCheck();
			{
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
	
						PWGUI.Sampler2DPreview(terrainNoise as Sampler2D, false, FilterMode.Point);
					}
					else
					{
						EditorGUILayout.LabelField("TODO: water level 3D");
					}
					if (waterGradient == null || EditorGUI.EndChangeCheck() || updateWaterPreview)
					{
						UpdateGradient();
						delayedChanges.UpdateValue(delayedUpdateKey);
					}
				}
			}
		}

		public override void OnNodeProcess()
		{
			if (terrainNoise == null)
			{
				Debug.LogError("[PWNodeWaterLevel] null terrain input received !");
				return ;
			}

			outputBiome.terrainRef = terrainNoise;
			
			outputBiome.waterLevel = waterLevel;

			if (terrainNoise.type == SamplerType.Sampler2D)
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

		void CreateNewBiome()
		{

		}

		public override void OnNodeProcessOnce()
		{
			CreateNewBiome();
		}
	}
}
