﻿using UnityEditor;
using UnityEngine;
using PW.Core;

namespace PW.Node
{
	public class PWNodeChunkData : PWNode
	{
	
		[PWInput]
		public BlendedBiomeTerrain	inputBlendedBiomes;

		[PWOutput]
		public ChunkData			terrainOutput;

		[SerializeField]
		bool						blendMapsFoldout;

		[System.NonSerialized]
		public Texture2DArray		biomeTextureArray = null;

		[SerializeField]
		MapOutputOption[]			outputMaps = {
			new MapOutputOption("height", true),
			new MapOutputOption("wetness", false),
			new MapOutputOption("temperatue", false),
			new MapOutputOption("air", false),
			new MapOutputOption("lighting", false),
		};

		[System.Serializable]
		class MapOutputOption
		{
			public string			name;
			public bool				active;

			public MapOutputOption(string name, bool active)
			{
				this.name = name;
				this.active = active;
			}
		}

		public override void OnNodeCreation()
		{
			name = "Data To Chunk";
			//TODO: change the output type based on a popup
			terrainOutput = new ChunkData();
		}

		public override void OnNodeGUI()
		{
			int i = 0;
			EditorGUIUtility.labelWidth = 80;
			mainGraphRef.materializerType = (MaterializerType)EditorGUILayout.EnumPopup("Materializer", mainGraphRef.materializerType);
			
			EditorGUILayout.LabelField("Output maps in chunk");
			EditorGUIUtility.labelWidth = 66;
			EditorGUILayout.BeginHorizontal();
			foreach (var outputMap in outputMaps)
			{
				i++;
				outputMap.active = EditorGUILayout.Toggle(outputMap.name, outputMap.active);
				if (i % 2 == 0)
				{
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		public override void OnNodeProcess()
		{
			terrainOutput.biomeMap = inputBlendedBiomes.biomeMap;
			terrainOutput.biomeMap3D = inputBlendedBiomes.biomeMap3D;
			terrainOutput.materializerType = mainGraphRef.materializerType;

			var biomeData = inputBlendedBiomes.biomeData;

			//assign everything needed to the output chunk:
			terrainOutput.size = chunkSize;
			if (outputMaps[0].active)
				terrainOutput.terrain = biomeData.terrain;
			if (outputMaps[1].active)
				terrainOutput.wetnessMap = biomeData.wetnessRef;
			if (outputMaps[2].active)
				terrainOutput.temperatureMap = biomeData.temperatureRef;
			if (outputMaps[3].active)
				terrainOutput.airMap = biomeData.airRef;
			if (outputMaps[4].active)
				terrainOutput.lightingMap = biomeData.lighting;
		}
	}
}