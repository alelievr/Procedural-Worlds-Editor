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

		public override void OnNodeCreate()
		{
			name = "2D TopDown terrain";
			terrainOutput = new TopDown2DData();
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 80;
			materializer = (MaterializerType)EditorGUILayout.EnumPopup("Materializer", materializer);
		}

		public override void OnNodeProcess()
		{
			terrainOutput.size = chunkSize;
			terrainOutput.wetnessMap = inputBlendedBiomes.wetnessMap;
			
			//TODO: apply biome terrain modifiers to terrain

			//TODO: apply biome terrain detail (caves / oth)

			//TODO: apply geologic layer (rivers / oth)

			//TODO: place vegetation / small details
		}
	}
}
