using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW
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

		public override void OnNodeCreate()
		{
			externalName = "Temperature node";

			Gradient temperatureGradient = PWUtils.CreateGradient(
				new KeyValuePair< float, Color >(0f, Color.blue),
				new KeyValuePair< float, Color >(.25f, Color.cyan),
				new KeyValuePair< float, Color >(.5f, Color.yellow),
				new KeyValuePair< float, Color >(.75f, PWColorPalette.orange),
				new KeyValuePair< float, Color >(1f, Color.red)
			);
			PWGUI.SetGradientForField(3, temperatureGradient);
		}

		public override void OnNodeGUI()
		{
			EditorGUILayout.Space();

			PWGUI.Slider("height multiplier", ref terrainHeightMultiplier, 0, 1);
			PWGUI.Slider(new GUIContent("water multiplier"), ref waterMultiplier, 0, 1);

			PWGUI.Sampler2DPreview(temperatureMap as Sampler2D, needUpdate, false, FilterMode.Point);

			//TODO: map temperature noise values

			//TODO: temperature map creation options
		}

		void UpdateTemperatureMap()
		{
			//create a new flat temperature map if not exists
			if (temperatureMap == null || temperatureMap.size != chunkSize || temperatureMap.step != step)
				temperatureMap = new Sampler2D(chunkSize, step);
			
			(temperatureMap as Sampler2D).Foreach((x, y, val) => {
				float	terrainMod = 0;
				float	waterMod = 0;

				if (terrainHeightMultiplier != 0)
					terrainMod = inputBiome.terrain.At(x, y, true) * terrainHeightMultiplier;
				if (waterMultiplier != 0)
					waterMod = inputBiome.waterHeight.At(x, y, true) * waterMultiplier;
				return minTemperature + terrainMod;
			});
			temperatureMap.min = minTemperature;
			temperatureMap.max = maxTemperature;
		}

		public override void OnNodeProcess()
		{
			if (temperatureMap != null || needUpdate)
				UpdateTemperatureMap();
			
			if (temperatureMap.type == SamplerType.Sampler2D)
				inputBiome.temperature = temperatureMap as Sampler2D;
			else if (temperatureMap.type == SamplerType.Sampler3D)
				inputBiome.temperature3D = temperatureMap as Sampler3D;
			else
				Debug.LogWarning("bad sampler type, only Sampler2D and Sampler3D is allowed");

			outputBiome = inputBiome;
		}

	}
}