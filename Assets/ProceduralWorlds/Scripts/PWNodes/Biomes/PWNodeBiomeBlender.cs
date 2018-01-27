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
		bool				firstGUI = true;

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

		public BiomeData	GetBiomeData()
		{
			var biomes = inputBiomes.GetValues();
			var biomeRef = biomes.FirstOrDefault(b => b.biomeDataReference != null);
			if (biomeRef == null)
				return null;
			return biomeRef.biomeDataReference;
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
				if (biomeData.biomeIds != null)
					PWGUI.BiomeMap2DPreview(biomeData);
				//TODO: biome 3D preview
			}
			else
				EditorGUILayout.LabelField("no biome data");
			
			if (firstGUI)
				PWGUI.SetUpdateForField(0, true);

			if (biomeCoverageRecap = EditorGUILayout.Foldout(biomeCoverageRecap, "Biome coverage recap"))
			{
				if (biomeData != null && biomeData.biomeTree != null)
				{
					foreach (var biomeCoverageKP in biomeData.biomeTree.GetBiomeCoverage())
						if (biomeCoverageKP.Value > 0)
							EditorGUILayout.LabelField(biomeCoverageKP.Key.ToString(), (biomeCoverageKP.Value * 100).ToString("F2") + "%");
				}
				else
					EditorGUILayout.LabelField("Null biome data/biome tree");
			}

			firstGUI = false;
		}

		public override void OnNodeProcess()
		{
			if (inputBiomes.Count == 0 || inputBiomes.GetValues().First() == null)
				return ;

			var biomes = inputBiomes.GetValues();
			var biomeData = biomes[0].biomeDataReference;

			if (biomeData == null)
				return ;
			
			//run the biome tree precomputing once all the biome tree have been parcoured
			if (!biomeData.biomeTree.isBuilt)
				biomeData.biomeTree.BuildTree(biomeData.biomeTreeStartPoint);

			biomeData.biomeTree.FillBiomeMap(maxBiomeBlendCount, biomeData);

			outputBlendedBiomeTerrain.biomes.Clear();

			//once the biome data is filled, we call the biome graphs corresponding to the biome id
			foreach (var id in biomeData.ids)
			{
				foreach (var biome in biomes)
					if (id == biome.id)
					{
						if (biome.biomeGraph == null)
							continue ;
						
						biome.biomeGraph.SetInput(biomes[id]);
						biome.biomeGraph.Process();
						Biome b = biome.biomeGraph.GetOutput();
						outputBlendedBiomeTerrain.biomes.Add(b);
					}
			}

			outputBlendedBiomeTerrain.biomeTree = biomeData.biomeTree;
			outputBlendedBiomeTerrain.biomeData = biomeData;
		}

		void OnReloadCallback(PWNode from)
		{
			//Reload from the editor:
			if (from == null)
				BuildBiomeTree();
		}
		
		void BuildBiomeTree()
		{
			var biomeData = inputBiomes.GetValues()[0].biomeDataReference;

			if (biomeData == null)
			{
				Debug.LogError("[PWBiomeBlender] Can't access to partial biome data, did you forgot the BiomeGraph in a biome node ?");
				return ;
			}

			biomeData.biomeTree.BuildTree(biomeData.biomeTreeStartPoint);
		}
		
		public override void OnNodeProcessOnce()
		{
			var partialBiomes = inputBiomes.GetValues();
			var biomeData = partialBiomes[0].biomeDataReference;

			foreach (var partialBiome in partialBiomes)
				partialBiome.biomeGraph.ProcessOnce();

			if (biomeData == null)
			{
				Debug.LogWarning("Can't build the biome albedo map, need to access to Biome datas !");
				return ;
			}

			//build the biome tree:
			biomeData.biomeTree.BuildTree(biomeData.biomeTreeStartPoint);
		}

		public override void OnNodeDisable()
		{
			OnReload -= OnReloadCallback;
		}
	}
}
