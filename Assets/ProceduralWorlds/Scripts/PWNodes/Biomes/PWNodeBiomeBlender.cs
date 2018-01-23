using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeBlender : PWNode {

		[PWInput]
		public PWArray< Biome >		inputBiomes = new PWArray< Biome >();

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

			//once the biome data is filled, we call the biome graphs corresponding to the biome id
			foreach (var id in biomeData.ids)
			{
				foreach (var biome in biomes)
					if (id == biome.id)
					{
						biome.biomeGraph.id = id;
						biome.biomeGraph.SetInput(biomeData);
						biome.biomeGraph.Process();
					}
			}

			outputBlendedBiomeTerrain.biomeMap = biomeData.biomeIds;
			outputBlendedBiomeTerrain.biomeMap3D = biomeData.biomeIds3D;
			outputBlendedBiomeTerrain.biomeTree = biomeData.biomeTree;
			outputBlendedBiomeTerrain.terrain = biomeData.terrainRef;
			outputBlendedBiomeTerrain.waterHeight = biomeData.waterHeight;
			outputBlendedBiomeTerrain.wetnessMap = biomeData.wetnessRef;
			outputBlendedBiomeTerrain.temperatureMap = biomeData.temperatureRef;
			outputBlendedBiomeTerrain.windMap = biomeData.windRef;
			outputBlendedBiomeTerrain.lightingMap = biomeData.lighting;
			outputBlendedBiomeTerrain.airMap = biomeData.airRef;
		}

		void OnReloadCallback(PWNode from)
		{
			//Reload from the editor:
			if (from == null)
				BuildBiomeTree();
		}
		
		void BuildBiomeTree()
		{
			inputBiomes.GetValues()[0].biomeDataReference.biomeTree.BuildTree(inputBiomes.GetValues()[0].biomeDataReference.biomeTreeStartPoint);
		}
		
		public override void OnNodeProcessOnce()
		{
			var biomes = inputBiomes.GetValues();
			var biomeData = biomes[0].biomeDataReference;

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
