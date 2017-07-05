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

		[PWMultiple(1, typeof(Biome))]
		[PWInput]
		public PWValues		inputBiomes = new PWValues();

		//TODO: set the terrain as output.

		//TODO: biome merge range by disponible biomeData fields

		Texture2D			biomeRepartitionPreview;
		int					maxBiomeBlendCount = 2;

		[SerializeField]
		bool				biomePreviewFoldout = true;
		[SerializeField]
		bool				biomeCoverageRecap = false;

		public override void OnNodeCreate()
		{
			externalName = "Biome blender";

			InitOrUpdatePreview();
		}

		void InitOrUpdatePreview()
		{
			if (inputBiomes.GetValues< Biome >().Count != 0)
			{
				var biome = inputBiomes.GetValues< Biome >().First().biomeDataReference;
				var heightSamp = biome.terrainRef;
				biomeRepartitionPreview = new Texture2D(heightSamp.size, heightSamp.size, TextureFormat.RGBA32, false);
				biomeRepartitionPreview.filterMode = FilterMode.Point;
				if (chunkSizeHasChanged)
					biomeRepartitionPreview.Resize(heightSamp.size, heightSamp.size);
			}
		}

		public override void OnNodeGUI()
		{
			var biomes = inputBiomes.GetValues< Biome >();
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
				if ((biomePreviewFoldout = EditorGUILayout.Foldout(biomePreviewFoldout, "Biome id map")))
				{
					if (biomeData.biomeIds != null)
						PWGUI.BiomeMap2DPreview(biomeData, needUpdate || forceReload || biomeReloadRequested);
					//TODO: biome 3D preview
				}
			}
			else
				EditorGUILayout.LabelField("no biome data");

			if (biomeCoverageRecap = EditorGUILayout.Foldout(biomeCoverageRecap, "Biome coverage recap"))
			{
				foreach (var biomeCoverageKP in biomeData.biomeTree.GetBiomeCoverage())
					if (biomeCoverageKP.Value > 0)
						EditorGUILayout.LabelField(biomeCoverageKP.Key.ToString(), (biomeCoverageKP.Value * 100).ToString("F2") + "%");
				//TODO: exloit the biome switch tree datas
			}
		}

		public override void OnNodeProcess()
		{
			if (inputBiomes.Count == 0 || inputBiomes.GetValues< Biome >().First() == null)
				return ;

			var biomes = inputBiomes.GetValues< Biome >();
			var biomeData = biomes[0].biomeDataReference;

			if (biomeData == null)
				return ;

			//run the biome tree precomputing once all the biome tree have been parcoured
			if (!biomeData.biomeTree.isBuilt || forceReload || biomeReloadRequested)
				biomeData.biomeTree.BuildTree(biomeData.biomeTreeStartPoint);

			biomeData.biomeTree.FillBiomeMap(maxBiomeBlendCount, biomeData);
		}
	}
}
