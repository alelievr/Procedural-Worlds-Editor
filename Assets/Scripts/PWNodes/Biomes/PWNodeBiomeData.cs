using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeBiomeData : PWNode {

		[PWInput("Terrain input")]
		[PWOffset(5)]
		public Sampler			terrain;

		[PWOutput("Biome datas")]
		[PWOffset(5)]
		public BiomeData		outputBiome;
		
		[SerializeField]
		float				mapMin;
		[SerializeField]
		float				mapMax;

		public override void OnNodeCreateOnce()
		{
			mapMin = 0;
			mapMax = 100;
			externalName = "Terrain to BiomeData";
		}
		
		public override void OnNodeGUI()
		{
			GUILayout.Space(GUI.skin.label.lineHeight * 2f);
			EditorGUIUtility.labelWidth = 100;
			if (terrain != null)
			{
				if (terrain.type == SamplerType.Sampler2D)
				{
					EditorGUILayout.LabelField("Map terrain values:");
					EditorGUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth = 30;
					mapMin = EditorGUILayout.FloatField("from", mapMin);
					mapMax = EditorGUILayout.FloatField("to", mapMax);
					EditorGUILayout.EndHorizontal();
				}
			}
			else
				EditorGUILayout.LabelField("Connect a terrain plz.");
		}

		public override void OnNodeProcess()
		{
			if (outputBiome == null || forceReload)
			{
				outputBiome = new BiomeData();

				outputBiome.isWaterless = true;
				outputBiome.biomeTreeStartPoint = this;
			}
			if (needUpdate || reloadRequested)
			{
				if (terrain != null && terrain.type == SamplerType.Sampler2D)
				{
					//terrain mapping
					outputBiome.terrain = PWNoiseFunctions.Map(terrain as Sampler2D, mapMin, mapMax, true);
				}
			}
		}
	}
}
