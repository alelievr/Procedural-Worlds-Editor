using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeData : PWNode {

		[PWInput("Terrain input")]
		[PWOffset(5)]
		[System.NonSerialized]
		public Sampler			terrain;

		[PWOutput("Biome datas")]
		[PWOffset(5)]
		[System.NonSerialized]
		public BiomeData		outputBiome;
		
		[SerializeField]
		float				mapMin;
		[SerializeField]
		float				mapMax;

		public override void OnNodeCreation()
		{
			mapMin = 0;
			mapMax = 100;
			name = "Terrain to BiomeData";
		}

		public override void OnNodeEnable()
		{
			CreateNewBiome();
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

		void				CreateNewBiome()
		{
			outputBiome = new BiomeData();

			outputBiome.isWaterless = true;
			outputBiome.biomeSwitchGraphStartPoint = this;
		}

		public override void OnNodeProcess()
		{
			if (terrain != null && terrain.type == SamplerType.Sampler2D)
			{
				//terrain mapping
				outputBiome.UpdateSamplerValue(BiomeSamplerName.terrainHeight, PWNoiseFunctions.Map(terrain as Sampler2D, mapMin, mapMax, true));
			}
		}
	}
}
