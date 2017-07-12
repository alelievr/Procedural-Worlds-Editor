using UnityEditor;
using UnityEngine;
using PW.Core;

namespace PW.Node
{
	public class PWNodeTopDown2DTerrain : PWNode {
	
		[PWInput]
		public BlendedBiomeTerrain	inputBlendedBiomes;

		[PWOutput]
		public TopDown2DData		terrainOutput;

		[SerializeField]
		MaterializerType			materializer;

		[System.NonSerialized]
		public Texture2DArray		biomeTextureArray = null;

		[SerializeField]
		MapOutputOption[]			outputMaps = {
			new MapOutputOption("height", false),
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

		public override void OnNodeCreate()
		{
			externalName = "2D TopDown terrain";
			terrainOutput = new TopDown2DData();
		}

		public override void OnNodeGUI()
		{
			int i = 0;
			EditorGUIUtility.labelWidth = 80;
			materializer = (MaterializerType)EditorGUILayout.EnumPopup("Materializer", materializer);
			
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
			//TODO: 3D biome map management
			BiomeUtils.ApplyBiomeTerrainModifiers(inputBlendedBiomes);

			if (biomeTextureArray == null)
				terrainOutput.blendMaps = BiomeUtils.GenerateBiomeBlendMaps(inputBlendedBiomes, GetGraphName());

			//TODO: apply geologic layer (rivers / oth)

			//TODO: place vegetation / small details

			//assign everything needed to the output chunk:
			terrainOutput.size = chunkSize;
			if (outputMaps[0].active)
				terrainOutput.terrain = inputBlendedBiomes.terrain;
			if (outputMaps[1].active)
				terrainOutput.wetnessMap = inputBlendedBiomes.wetnessMap;
			if (outputMaps[2].active)
				terrainOutput.temperatureMap = inputBlendedBiomes.temperatureMap;
			if (outputMaps[3].active)
				terrainOutput.airMap = inputBlendedBiomes.airMap;
			if (outputMaps[4].active)
				terrainOutput.lightingMap = inputBlendedBiomes.lightingMap;
		}
	}
}
