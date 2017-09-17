using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeBiomeTemperature : PWNode {

		[PWInput("biome datas")]
		public BiomeData		inputBiome;
		[PWInput("temperature map")]
		[PWNotRequired]
		public Sampler			temperatureMap;
		
		[PWOutput]
		public BiomeData		outputBiome;

		[SerializeField]
		float					minTemperature = -30;
		[SerializeField]
		float					maxTemperature = 40;
		[SerializeField]
		float					terrainHeightMultiplier = 0;
		[SerializeField]
		float					waterMultiplier = .2f;
		[SerializeField]
		float					averageTemperature = 17;

		Gradient				temperatureGradient;
		bool					fieldUpdate = false;
		[SerializeField]
		bool					internalTemperatureMap = true;

		public override void OnNodeCreation()
		{
			name = "Temperature node";

			temperatureGradient = PWUtils.CreateGradient(
				new KeyValuePair< float, Color >(0f, Color.blue),
				new KeyValuePair< float, Color >(.25f, Color.cyan),
				new KeyValuePair< float, Color >(.5f, Color.yellow),
				new KeyValuePair< float, Color >(.75f, PWColorPalette.orange),
				new KeyValuePair< float, Color >(1f, Color.red)
			);
			
			UpdateTemperatureMap();
		}

		public override void OnNodeGUI()
		{
			fieldUpdate = false;

			GUILayout.Space(GUI.skin.label.lineHeight * 3);

			EditorGUI.BeginChangeCheck();
			PWGUI.Slider("height multiplier: ", ref terrainHeightMultiplier, -1, 1);
			PWGUI.Slider(new GUIContent("water multiplier: "), ref waterMultiplier, -1, 1);

			EditorGUILayout.LabelField("temperature limits:");
			EditorGUILayout.BeginHorizontal();
			EditorGUIUtility.labelWidth = 30;
			minTemperature = EditorGUILayout.FloatField("from", minTemperature);
			maxTemperature = EditorGUILayout.FloatField("to", maxTemperature);
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 120;
			averageTemperature = EditorGUILayout.FloatField("average temperature", averageTemperature);
			if (EditorGUI.EndChangeCheck())
				fieldUpdate = true;
				
			if (fieldUpdate)
				UpdateTemperatureMap();
			
			PWGUI.Sampler2DPreview(temperatureMap as Sampler2D, needUpdate, false, FilterMode.Point);

			if (fieldUpdate)
			{
				PWGUI.SetGradientForField(2, temperatureGradient);
				PWGUI.SetDebugForField(2, true);
			}

			//TODO: temperature map creation options
		}

		public override bool OnNodeAnchorLink(string prop, int index)
		{
			if (prop == "temperatureMap")
				internalTemperatureMap = false;
			
			return true;
		}
		
		public override void OnNodeAnchorUnlink(string prop, int index)
		{
			if (prop == "temperatureMap")
				internalTemperatureMap = true;
		}

		void UpdateTemperatureMap()
		{
			//create a new flat temperature map if not exists
			if (temperatureMap == null || temperatureMap.size != chunkSize || temperatureMap.step != step)
				temperatureMap = new Sampler2D(chunkSize, step);
			
			(temperatureMap as Sampler2D).Foreach((x, y, val) => {
				float	terrainMod = 0;
				float	waterMod = 0;
				float	temperatureRange = (maxTemperature - minTemperature);
				float	mapValue = (internalTemperatureMap) ? averageTemperature : val;

				if (inputBiome != null)
				{
					if (terrainHeightMultiplier != 0)
						terrainMod = inputBiome.terrain.At(x, y, true) * terrainHeightMultiplier * temperatureRange;
					if (waterMultiplier != 0)
						waterMod = inputBiome.waterHeight.At(x, y, true) * waterMultiplier * temperatureRange;
				}
				return mapValue + terrainMod + waterMod;
			});

			temperatureMap.min = minTemperature;
			temperatureMap.max = maxTemperature;
		}

		public override void OnNodeProcess()
		{
			if (temperatureMap != null || needUpdate)
				UpdateTemperatureMap();
			
			if (inputBiome != null)
			{
				inputBiome.temperature = temperatureMap as Sampler2D;
				inputBiome.temperature3D = temperatureMap as Sampler3D;
			}

			outputBiome = inputBiome;
		}

		//to prebuild the biome tree:
		public override void OnNodeProcessOnce()
		{
			outputBiome = inputBiome;
		}

	}
}
