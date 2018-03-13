using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Biomator;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Node
{
	public class NodeBiomeWetness : BaseNode
	{

		[Input]
		public BiomeData	inputBiomeData;

		[Output]
		public BiomeData	outputBiomData;

		public Sampler2D	wetnessMap;

		public override void OnNodeCreation()
		{
			name = "Wetness node";
		}

		public override void OnNodeEnable()
		{
			wetnessMap = new Sampler2D(chunkSize, step);
		}

		void UpdateWetnessMap()
		{
			if (inputBiomeData == null || wetnessMap == null)
				return ;
			
			var terrain = inputBiomeData.GetSampler2D(BiomeSamplerName.terrainHeight);

			wetnessMap.ResizeIfNeeded(terrain.size, terrain.step);

			//TODO: Compute wetness map
		}

		public override void OnNodeProcess()
		{
			UpdateWetnessMap();
			
			if (inputBiomeData != null)
				inputBiomeData.UpdateSamplerValue(BiomeSamplerName.wetness, wetnessMap);

			outputBiomData = inputBiomeData;
		}
		
		public override void OnNodeProcessOnce()
		{
			outputBiomData = inputBiomeData;
		}
		
	}
}
