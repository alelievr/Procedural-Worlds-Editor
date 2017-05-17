using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace PW
{
	public class PWNodeWaterLevel : PWNode {

		[PWInput]
		public Sampler		terrainNoise;

		[PWOutput]
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
			mapMax = 1;
		}

		void UpdateGradient()
		{
			waterGradient = PWUtils.CreateGradient(GradientMode.Fixed, 
				new KeyValuePair< float, Color >(0, Color.white),
				new KeyValuePair< float, Color >(0.5f, Color.blue));
		}
		
		public override void OnNodeGUI()
		{
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
					if (terrainNoise.GetType() == typeof(Sampler2D))
					{
						EditorGUILayout.LabelField("Map terrain values:");
						EditorGUILayout.BeginHorizontal();
						EditorGUIUtility.labelWidth = 30;
						mapMin = EditorGUILayout.FloatField("from", mapMin);
						mapMax = EditorGUILayout.FloatField("to", mapMax);
						EditorGUILayout.EndHorizontal();
	
						PWGUI.Sampler2DPreview(terrainNoise as Sampler2D, needUpdate);
					}
			}
			if (EditorGUI.EndChangeCheck())
				UpdateGradient();
		}

		public override void OnNodeProcess()
		{
			if (biomeData == null)
			{
				biomeData = new BiomeData();
				biomeData.waterLevel = waterLevel;
			}
			if (needUpdate)
			{
				biomeData.terrain = terrainNoise as Sampler2D;
				biomeData.terrain3D = terrainNoise as Sampler3D;
			}
		}

	}
}