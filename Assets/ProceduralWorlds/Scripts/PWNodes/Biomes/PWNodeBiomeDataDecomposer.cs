using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeDataDecomposer : PWNode
	{

		[PWInput("Biome Data")]
		public BiomeData		inputBiomeData;

		[PWOutput("Biome Data")]
		public BiomeData		outputBiomeData;

		[PWOutput("Terrain")]
		public Sampler			outputTerrain;

		//Called only when the node is created (not when it is enabled/loaded)
		public override void OnNodeCreation()
		{
			name = "";
		}

		public override void OnNodeEnable()
		{
		}

		public override void OnNodeGUI()
		{
		}

		public override void OnNodeProcess()
		{
			if (inputBiomeData == null)
			{
				Debug.LogError("[PWNodeBiomeDataDecomposer]: Null input biome data");
				return ;
			}

			outputTerrain = inputBiomeData.terrainRef;
		}
		
	}
}