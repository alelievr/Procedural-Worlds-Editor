using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeBiomeBinder : PWNode {

		[PWInput("Partial Biome")]
		public PartialBiome			inputPartialBiome;
		
		[PWInput("Terrain"), PWNotRequired]
		public Sampler				terrain;
		
		//inputs for 2D topdown map
		[PWInput("Surfaces")]
		[PWNotRequired]
		public BiomeSurfaces		biomeSurfaces;

		[PWInput("Details")]
		[PWNotRequired]
		public TerrainDetail		biomeDetail;

		//TODO: dispositon algos

		//inputs for 3D planar terrain
		//TODO

		[PWOutput("biome")]
		public Biome				outputBiome;
		
		public PWGraphTerrainType	outputMode;

		[SerializeField]
		Rect						colorPreviewRect;

		public override void OnNodeCreation()
		{
			name = "Biome binder";
		}

		public override void OnNodeGUI()
		{
			//TODO: preview the modified terrain
		}

		public override void OnNodeAnchorLink(string prop, int index)
		{
			if (prop == "inputPartialBiome" && outputBiome != null)
				outputBiome.biomeDataReference = inputPartialBiome.biomeDataReference;
		}

		public override void OnNodeProcess()
		{
			if (outputBiome == null)
				outputBiome = new Biome();
			outputBiome.biomeDataReference = inputPartialBiome.biomeDataReference;
			outputBiome.biomeSurfaces = biomeSurfaces;
		}

		public override void OnNodeProcessOnce()
		{
			//just pass the biomeSurfaces to the blender for processOnce:
			if (outputBiome == null)
				outputBiome = new Biome();
			
			outputBiome.biomeSurfaces = biomeSurfaces;
			outputBiome.id = inputPartialBiome.id;
			outputBiome.name = inputPartialBiome.name;
			outputBiome.previewColor = inputPartialBiome.previewColor;
			outputBiome.biomeDataReference = inputPartialBiome.biomeDataReference;

			//we set our version of the terrain for future merge
			outputBiome.modifiedTerrain = terrain;
		}
	}
}
