using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Nodes
{
	public class NodeBiomeBinder : BaseNode
	{

		[Input("Partial Biome")]
		public PartialBiome			inputPartialBiome;
		
		[Input("Terrain"), NotRequired]
		public Sampler				terrain;
		
		//inputs for 2D topdown map
		[Input("Surfaces"), NotRequired]
		public BiomeSurfaceGraph	biomeSurfaceGraph;

		[Input("Details"), NotRequired]
		public TerrainDetail		biomeDetail;

		[Output("biome")]
		public Biome				outputBiome;
		
		public BaseGraphTerrainType	outputMode;

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
