using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;
using System;

namespace ProceduralWorlds.Nodes
{
	public class NodeBiomeBlender : BaseNode
	{
		[Input]
		public PWArray< PartialBiome >	inputBiomes = new PWArray< PartialBiome >();

		[Output]
		public BlendedBiomeTerrain		outputBlendedBiomeTerrain = new BlendedBiomeTerrain();

		public float			biomeBlendPercent = .1f;

		public BiomeBlendList	blendList = new BiomeBlendList();

		public bool				biomeCoverageRecap = false;

		public override void OnNodeCreation()
		{
			name = "Biome blender";
		}

		public BiomeData GetBiomeData()
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

			outputBlendedBiomeTerrain.biomePerIds.Clear();

			//if the main graph was processed from a biome graph, we process all the available biomes
			if (worldGraphRef.processedFromBiome)
				ProcessAllBiomes(partialBiomes);
			else
				ProcessChunkBiomes(biomeData, partialBiomes);


			outputBlendedBiomeTerrain.biomeData = biomeData;
		}

		void ProcessChunkBiomes(BiomeData biomeData, List< PartialBiome > partialBiomes)
		{
			//We process only the biomes found into the chunk
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
						partialBiome.biomeGraph.ProcessFrom(worldGraphRef);

						if (!partialBiome.biomeGraph.hasProcessed)
						{
							Debug.LogError("[PWBiomeBlender] Can't process the biome graph '" + partialBiome.biomeGraph + "'");
							continue ;
						}

						Biome b = partialBiome.biomeGraph.GetOutput();

						if (b == null)
							throw new InvalidOperationException("Biome graph " + partialBiome.biomeGraph + " returns null biome");

						if (outputBlendedBiomeTerrain.biomePerIds.ContainsKey(b.id))
						{
							Debug.LogError("[PWBiomeBlender] Duplicate biome in the biome graph: " + b.name + " (" + b.id + ")");
							continue ;
						}

						outputBlendedBiomeTerrain.biomePerIds[b.id] = b;
					}
				}
			}
		}

		void ProcessAllBiomes(List< PartialBiome > partialBiomes)
		{
			foreach (var p in partialBiomes)
			{
				p.biomeGraph.SetInput(p);
				p.biomeGraph.ProcessFrom(worldGraphRef);
			}
		}

		public void FillBiomeMap(BiomeData biomeData)
		{
			if (biomeData.biomeMap == null)
				biomeData.biomeMap = new BiomeMap2D(chunkSize, step);

			biomeData.biomeMap.ResizeIfNeeded(chunkSize, step);

			biomeData.biomeSwitchGraph.FillBiomeMap2D(biomeData, blendList, biomeBlendPercent);
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
