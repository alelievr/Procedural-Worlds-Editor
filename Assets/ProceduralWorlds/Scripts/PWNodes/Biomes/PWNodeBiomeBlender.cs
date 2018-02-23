using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeBlender : PWNode
	{

		[PWInput]
		public PWArray< PartialBiome >	inputBiomes = new PWArray< PartialBiome >();

		[PWOutput]
		public BlendedBiomeTerrain		outputBlendedBiomeTerrain = new BlendedBiomeTerrain();

		public float			biomeBlendPercent = .1f;

		public BiomeBlendList	blendList = new BiomeBlendList();

		public bool				biomeCoverageRecap = false;

		public override void OnNodeCreation()
		{
			name = "Biome blender";
		}

		public override void OnNodeEnable()
		{
			if (inputBiomes.GetValues().Count == 0)
				return ;
		}

		public BiomeData	GetBiomeData()
		{
			var partialbiomes = inputBiomes.GetValues();
			
			if (partialbiomes.Count == 0)
				return null;
			
			var biomeDataRef = partialbiomes.FirstOrDefault(pb => pb != null && pb.biomeDataReference != null);

			if (biomeDataRef == null)
				return null;

			return biomeDataRef.biomeDataReference;
		}

		public override void OnNodeProcessOnce()
		{
			var partialBiomes = inputBiomes.GetValues();

			foreach (var partialBiome in partialBiomes)
				partialBiome.biomeGraph.ProcessOnce();

			BuildBiomeSwitchGraph();
		}

		public override void OnNodeProcess()
		{
			if (inputBiomes.Count == 0 || inputBiomes.GetValues().All(b => b == null))
				return ;

			var partialBiomes = inputBiomes.GetValues();
			var biomeData = GetBiomeData();

			if (biomeData == null)
				return ;
			
			//run the biome tree precomputing once all the biome tree have been parcoured
			if (!biomeData.biomeSwitchGraph.isBuilt)
				BuildBiomeSwitchGraph();
			
			FillBiomeMap(biomeData);

			outputBlendedBiomeTerrain.biomes.Clear();

			//once the biome data is filled, we call the biome graphs corresponding to the biome id
			foreach (var id in biomeData.ids)
			{
				foreach (var partialBiome in partialBiomes)
				{
					if (partialBiome == null)
						continue ;
					
					if (id == partialBiome.id)
					{
						if (partialBiome.biomeGraph == null)
							continue ;
						
						partialBiome.biomeGraph.SetInput(partialBiome);
						partialBiome.biomeGraph.Process();

						if (!partialBiome.biomeGraph.hasProcessed)
						{
							Debug.LogError("[PWBiomeBlender] Can't process properly the biome graph '" + partialBiome.biomeGraph + "'");
							continue ;
						}

						Biome b = partialBiome.biomeGraph.GetOutput();

						if (outputBlendedBiomeTerrain.biomes.Contains(b))
						{
							Debug.LogError("[PWBiomeBlender] Duplicate biome in the biome graph: " + b.name + " (" + b.id + ")");
							continue ;
						}

						outputBlendedBiomeTerrain.biomes.Add(b);
					}
				}
			}

			outputBlendedBiomeTerrain.biomeData = biomeData;
		}

		public void FillBiomeMap(BiomeData biomeData)
		{
			biomeData.biomeSwitchGraph.FillBiomeMap(biomeData, blendList, biomeBlendPercent);
		}
		
		public void BuildBiomeSwitchGraph()
		{
			BiomeData biomeData = GetBiomeData();

			if (biomeData == null)
			{
				Debug.LogError("[PWBiomeBlender] Can't access to partial biome data, did you forgot the BiomeGraph in a biome node ?");
				return ;
			}

			biomeData.biomeSwitchGraph.BuildGraph(biomeData);
		}
	}
}
