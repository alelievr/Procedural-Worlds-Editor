using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Biomator;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	public class BiomeDataInputDrawer : Drawer
	{
		BiomeDataInputGenerator		inputData;
		bool						showTerrain;

		public override void OnEnable()
		{
			inputData = target as BiomeDataInputGenerator;
		}

		public void OnGUI()
		{
			base.OnGUI(new Rect());

			EditorGUIUtility.labelWidth = 90;
			inputData.seed = EditorGUILayout.IntField("Seed", inputData.seed);
			inputData.size = PWGUI.IntSlider("Chunk size: ", inputData.size, 4, 512);
			inputData.step = PWGUI.Slider("Step: ", inputData.step, 0.01f, 16);

			if (PWGUI.BeginFade("Terrain", ref showTerrain, false))
			{
				inputData.maxTerrainHeight = PWGUI.IntSlider("Terrain height: ", inputData.maxTerrainHeight, 0, 1000);
				inputData.octaves = PWGUI.IntSlider("Noise octaves: ", inputData.octaves, 1, 8);
				inputData.persistance = PWGUI.Slider("Noise persistance: ", inputData.persistance, 0f, 2f);
				inputData.lacunarity = PWGUI.Slider("Noise lacunarity: ", inputData.lacunarity, 0f, 2f);
				inputData.isWaterless = EditorGUILayout.Toggle("Is waterless", inputData.isWaterless);
				if (!inputData.isWaterless)
					inputData.waterLevel = PWGUI.IntSlider("WaterLevel: ", (int)inputData.waterLevel, 0, 100);
			}
			PWGUI.EndFade();

			//TODO: dummy temperature/wetness generation
		}
	}
}