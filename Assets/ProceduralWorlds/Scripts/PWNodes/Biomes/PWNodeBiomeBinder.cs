using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeBinder : PWNode
	{

		[PWInput("Partial Biome")]
		public PartialBiome			inputPartialBiome;
		
		[PWInput("Terrain"), PWNotRequired]
		public Sampler				terrain;
		
		//inputs for 2D topdown map
		[PWInput("Surfaces"), PWNotRequired]
		public BiomeSurfaceGraph	biomeSurfaceGraph;

		[PWInput("Details"), PWNotRequired]
		public TerrainDetail		biomeDetail;

		[PWOutput("biome")]
		public Biome				outputBiome;
		
		public PWGraphTerrainType	outputMode;

		[SerializeField]
		Rect						colorPreviewRect;

		public override void OnNodeCreation()
		{
			name = "Biome binder";
		}

		void FillBiomeOutput()
		{
			if (inputPartialBiome == null)
				return ;
			
			outputBiome.biomeSurfaceGraph = biomeSurfaceGraph;
			outputBiome.id = inputPartialBiome.id;
			outputBiome.name = inputPartialBiome.name;
			outputBiome.previewColor = inputPartialBiome.previewColor;
			outputBiome.biomeDataReference = inputPartialBiome.biomeDataReference;

			//we set our version of the terrain for future merge
			outputBiome.modifiedTerrain = terrain;
		}

		public override void OnNodeProcess()
		{
			if (outputBiome == null)
				outputBiome = new Biome();
			
			FillBiomeOutput();
		}

		public override void OnNodeProcessOnce()
		{
			//just pass the biomeSurfaces to the blender for processOnce:
			if (outputBiome == null)
				outputBiome = new Biome();
			
			FillBiomeOutput();
		}
	}
}
