using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Biomator;
using UnityEditor;

namespace PW.Editor
{
	public class BiomeDataInputDrawer : PWDrawer
	{
		BiomeDataInputGenerator		inputData;
		bool						showTerrain = true;

		public override void OnEnable()
		{
			inputData = target as BiomeDataInputGenerator;
		}

		public void OnGUI()
		{
			base.OnGUI(new Rect());

			EditorGUIUtility.labelWidth = 90;
			inputData.size = PWGUI.IntSlider("Chunk size: ", inputData.size, 4, 512);
			inputData.step = PWGUI.Slider("Step: ", inputData.step, 0.01f, 16);

			if (PWGUI.BeginFade("Terrain", ref showTerrain, false))
			{
				inputData.maxTerrainHeight = PWGUI.IntSlider("Terrain height: ", inputData.maxTerrainHeight, 0, 1000);
				inputData.isWaterless = EditorGUILayout.Toggle("Is waterless", inputData.isWaterless);
				if (!inputData.isWaterless)
					inputData.waterLevel = PWGUI.IntSlider("WaterLevel: ", (int)inputData.waterLevel, 0, 100);
			}
			PWGUI.EndFade();

			//TODO: dummy temperature/wetness generation
		}
	}
}