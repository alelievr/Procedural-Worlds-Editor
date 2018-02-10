using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeBlender : PWNode
	{

		[PWInput]
		public PWArray< PartialBiome >		inputBiomes = new PWArray< PartialBiome >();

		[PWOutput]
		public BlendedBiomeTerrain	outputBlendedBiomeTerrain = new BlendedBiomeTerrain();

		int					maxBiomeBlendCount = 2;

		[SerializeField]
		bool				biomeCoverageRecap = false;

		[System.NonSerialized]
		bool				updateBiomeMap = true;

		public override void OnNodeCreation()
		{
			name = "Biome blender";
		}

		public override void OnNodeEnable()
		{
			OnReload += OnReloadCallback;
			
			if (inputBiomes.GetValues().Count == 0)
				return ;
		}

		public override void OnNodeDisable()
		{
			OnReload -= OnReloadCallback;
		}

		BiomeData	GetBiomeData()
		{
			var partialbiomes = inputBiomes.GetValues();
			
			if (partialbiomes.Count == 0)
				return null;
			
			var biomeDataRef = partialbiomes.FirstOrDefault(pb => pb != null && pb.biomeDataReference != null);

			if (biomeDataRef == null)
				return null;

			return biomeDataRef.biomeDataReference;
		}

		public override void OnNodeGUI()
		{
			var biomes = inputBiomes.GetValues();
			BiomeData biomeData = null;
			if (biomes.Count == 0 || biomes.First() == null)
				EditorGUILayout.LabelField("biomes not connected !");
			else
			{
				biomeData = biomes.First().biomeDataReference;
				EditorGUIUtility.labelWidth = 120;
			 	maxBiomeBlendCount = EditorGUILayout.IntField("max blended biomes", maxBiomeBlendCount);
			}

			if (biomeData != null)
			{
				if (biomeData.biomeMap != null)
					PWGUI.BiomeMap2DPreview(biomeData);
				//TODO: biome 3D preview
			}
			else
				EditorGUILayout.LabelField("no biome data");
			
			if (updateBiomeMap)
				PWGUI.SetUpdateForField(0, true);

			if (biomeCoverageRecap = EditorGUILayout.Foldout(biomeCoverageRecap, "Biome coverage recap"))
			{
				if (biomeData != null && biomeData.biomeSwitchGraph != null)
				{
					foreach (var biomeCoverageKP in biomeData.biomeSwitchGraph.GetBiomeCoverage())
						if (biomeCoverageKP.Value > 0)
							EditorGUILayout.LabelField(biomeCoverageKP.Key.ToString(), (biomeCoverageKP.Value * 100).ToString("F2") + "%");
				}
				else
					EditorGUILayout.LabelField("Null biome data/biome tree");
			}

			updateBiomeMap = false;
		}
		
		public override void OnNodeProcessOnce()
		{
			var partialBiomes = inputBiomes.GetValues();
			var biomeData = partialBiomes[0].biomeDataReference;

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
				biomeData.biomeSwitchGraph.BuildGraph(biomeData.biomeSwitchGraphStartPoint);

			biomeData.biomeSwitchGraph.FillBiomeMap(biomeData);

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
						
						partialBiome.biomeGraph.SetInput(partialBiomes[id]);
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

		void OnReloadCallback(PWNode from)
		{
			BuildBiomeSwitchGraph();
			
			var biomeData = GetBiomeData();
			
			//if the reload does not comes from the editor
			if (from != null)
			{
				biomeData.biomeSwitchGraph.FillBiomeMap(biomeData);
				updateBiomeMap = true;
			}
		}
		
		void BuildBiomeSwitchGraph()
		{
			BiomeData biomeData = GetBiomeData();

			if (biomeData == null)
			{
				Debug.LogError("[PWBiomeBlender] Can't access to partial biome data, did you forgot the BiomeGraph in a biome node ?");
				return ;
			}

			biomeData.biomeSwitchGraph.BuildGraph(biomeData.biomeSwitchGraphStartPoint);
		}
	}
}
