using UnityEditor;
using UnityEngine;
using PW.Core;

namespace PW.Node
{
	public class PWNodeChunkData : PWNode
	{
	
		[PWInput]
		public FinalBiomeTerrain	inputFinalTerrain;

		[PWOutput]
		public ChunkData			outputChunk;

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
			if (outputChunk == null)
				outputChunk = new ChunkData();
			
			outputChunk.biomeMap = inputFinalTerrain.biomeData.biomeIds;
			outputChunk.biomeMap3D = inputFinalTerrain.biomeData.biomeIds3D;
			outputChunk.materializerType = mainGraphRef.materializerType;
			outputChunk.biomeTexturing = inputFinalTerrain.biomeSurfacesList;

			var biomeData = inputFinalTerrain.biomeData;

			//assign everything needed to the output chunk:
			outputChunk.size = chunkSize;
			if (outputMaps[0].active)
				outputChunk.terrain = biomeData.terrain;
			if (outputMaps[1].active)
				outputChunk.wetnessMap = biomeData.wetnessRef;
			if (outputMaps[2].active)
				outputChunk.temperatureMap = biomeData.temperatureRef;
			if (outputMaps[3].active)
				outputChunk.airMap = biomeData.airRef;
			if (outputMaps[4].active)
				outputChunk.lightingMap = biomeData.lighting;
		}
	}
}
