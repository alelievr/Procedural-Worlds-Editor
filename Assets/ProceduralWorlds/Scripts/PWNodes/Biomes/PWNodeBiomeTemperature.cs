using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeTemperature : PWNode
	{

		[PWInput("biome datas")]
		public BiomeData		inputBiomeData;
		[PWInput("temperature map"), PWNotRequired]
		public Sampler			temperatureMap;

		Sampler					localTemperatureMap;
		
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

		[SerializeField]
		Gradient				temperatureGradient;
		[System.NonSerialized]
		bool					fieldUpdate = false;
		[SerializeField]
		bool					internalTemperatureMap = true;

		[SerializeField]
		float					minTemperatureMapInput;
		[SerializeField]
		float					maxTemperatureMapInput;

		string					delayedTemperatureKey = "PWNodeBiomeTemperature";

		public override void OnNodeCreation()
		{
			name = "Temperature node";
		}

		public override void OnNodeEnable()
		{
			UpdateTemperatureMap();

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
				terrainHeightMultiplier = PWGUI.Slider("height multiplier: ", terrainHeightMultiplier, -1, 1);
				waterMultiplier = PWGUI.Slider(new GUIContent("water multiplier: "), waterMultiplier, -1, 1);
	
				EditorGUILayout.LabelField("temperature limits:");
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUIUtility.labelWidth = 30;
					minTemperature = EditorGUILayout.FloatField("from", minTemperature);
					maxTemperature = EditorGUILayout.FloatField("to", maxTemperature);
				}
				EditorGUILayout.EndHorizontal();
				EditorGUIUtility.labelWidth = 120;
				if (internalTemperatureMap)
					averageTemperature = EditorGUILayout.FloatField("average temperature", averageTemperature);
				else
				{
					EditorGUILayout.LabelField("Input temperature map range");
					using (new DefaultGUISkin())
						EditorGUILayout.MinMaxSlider(ref minTemperatureMapInput, ref maxTemperatureMapInput, minTemperature, maxTemperature);
				}
			}
			if (EditorGUI.EndChangeCheck())
				fieldUpdate = true;
			
			if (localTemperatureMap != null)
			{
				PWGUI.Sampler2DPreview(localTemperatureMap as Sampler2D, false, FilterMode.Point);
				PWGUI.SetGradientForField(2, temperatureGradient);
				PWGUI.SetDebugForField(2, true);
			}
			
			if (fieldUpdate)
			{
				delayedChanges.UpdateValue(delayedTemperatureKey);
				PWGUI.SetUpdateForField(2, true);
				UpdateTemperatureMap();
				fieldUpdate = false;
			}
		}

		public override void OnNodeAnchorLink(string prop, int index)
		{
			if (prop == "temperatureMap")
			{
				internalTemperatureMap = false;
				UpdateTemperatureMap();
			}
		}
		
		public override void OnNodeAnchorUnlink(string prop, int index)
		{
			if (prop == "temperatureMap")
			{
				internalTemperatureMap = true;
				temperatureMap = null;
				UpdateTemperatureMap();
			}
		}

		void UpdateTemperatureMap()
		{
			if (localTemperatureMap == null || inputBiomeData == null)
				return ;
			
			var inputTemperatureMap = temperatureMap as Sampler2D;
				
			var terrain = inputBiomeData.GetSampler2D(BiomeSamplerName.terrainHeight);
			var waterHeight = inputBiomeData.GetSampler2D(BiomeSamplerName.waterHeight);

			(localTemperatureMap as Sampler2D).Foreach((x, y, val) => {
				float	terrainMod = 0;
				float	waterMod = 0;
				float	temperatureRange = (maxTemperature - minTemperature);
				float	mapValue = averageTemperature;

				if (!internalTemperatureMap)
					mapValue = Mathf.Lerp(Mathf.Max(minTemperature, minTemperatureMapInput), Mathf.Min(maxTemperature, maxTemperatureMapInput), inputTemperatureMap[x, y]);

				if (terrainHeightMultiplier != 0 && terrain != null)
					terrainMod = terrain.At(x, y, true) * terrainHeightMultiplier * temperatureRange;
				if (waterMultiplier != 0 && waterHeight != null)
					waterMod = waterHeight.At(x, y, true) * waterMultiplier * temperatureRange;
				return Mathf.Clamp(mapValue + terrainMod + waterMod, minTemperature, maxTemperature);
			});

			localTemperatureMap.min = minTemperature;
			localTemperatureMap.max = maxTemperature;

			if (inputBiomeData != null)
				inputBiomeData.UpdateSamplerValue(BiomeSamplerName.temperature, localTemperatureMap);
		}

		void CreateTemperatureMapIfNotExists()
		{
			if (temperatureMap == null)
			{
				localTemperatureMap = new Sampler2D(chunkSize, step);
				return ;
			}

			if (localTemperatureMap == null || localTemperatureMap.NeedResize(chunkSize, step))
			{
				localTemperatureMap = temperatureMap.Clone(localTemperatureMap);
				return ;
			}
		}

		public override void OnNodeProcess()
		{
			CreateTemperatureMapIfNotExists();

			UpdateTemperatureMap();

			outputBiome = inputBiomeData;
		}

		//to prebuild the biome tree:
		public override void OnNodeProcessOnce()
		{
			outputBiome = inputBiomeData;
		}

	}
}
