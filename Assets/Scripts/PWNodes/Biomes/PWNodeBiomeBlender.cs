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
		PWTerrainOutputMode	terrainMode;

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
			if (inputBiomes.Count == 0 || inputBiomes.GetValues< Biome >().First() == null)
				EditorGUILayout.LabelField("biomes not connected !");
			else
			{
				EditorGUIUtility.labelWidth = 120;
			 	maxBiomeBlendCount = EditorGUILayout.IntField("max blended biomes", maxBiomeBlendCount);
			}
			
			terrainMode = GetTerrainOutputMode();
		}

		public override void OnNodeProcess()
		{
			if (inputBiomes.Count == 0 || inputBiomes.GetValues< Biome >().First() == null)
				return ;

			var biomes = inputBiomes.GetValues< Biome >();
			var biomeData = biomes[0].biomeDataReference;

			//run the biome tree precomputing once all the biome tree have been parcoured
			if (!biomeData.biomeTree.isBuilt)
				biomeData.biomeTree.BuildTree(biomeData.biomeTreeStartPoint);

			switch (terrainMode)
			{
				case PWTerrainOutputMode.TopDown2D:
					BiomeBlending.BlendTopDownd2D(biomes);
					break ;
				case PWTerrainOutputMode.Planar3D:
					BiomeBlending.BlendPlanar3D(biomes);
					break ;
				//TODO: other terrain modes
			}
		}
	}
}
