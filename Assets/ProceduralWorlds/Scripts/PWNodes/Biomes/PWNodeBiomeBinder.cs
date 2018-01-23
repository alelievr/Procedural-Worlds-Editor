using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeBiomeBinder : PWNode {

		[PWInput("Biome datas")]
		public BiomeData			inputBiome;
		
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
			if (prop == "inputBiome" && outputBiome != null)
				outputBiome.biomeDataReference = inputBiome;
		}

		public override void OnNodeProcess()
		{
			if (outputBiome == null)
				outputBiome = new Biome();
			outputBiome.biomeDataReference = inputBiome;
			outputBiome.biomeSurfaces = biomeSurfaces;
		}

		public override void OnNodeProcessOnce()
		{
			//just pass the biomeSurfaces to the blender for processOnce:
			if (outputBiome == null)
				outputBiome = new Biome();
			
			outputBiome.biomeSurfaces = biomeSurfaces;
			outputBiome.biomeDataReference = inputBiome;

			//merge the modified terrain if there is
			if (terrain != null)
			{
				//if there is more than one biome in the chunk
				if (inputBiome.ids.Count > 1)
				{
					Sampler2D terrain2D = terrain as Sampler2D;

					if (terrain.type == SamplerType.Sampler2D)
						inputBiome.terrain.Foreach((x, y, val) => {
							float t = terrain2D[x, y];
							var info = inputBiome.biomeIds.GetBiomeBlendInfo(x, y);
							var p = (info.firstBiomeId == biomeGraphRef.id) ? info.firstBiomeBlendPercent : info.secondBiomeBlendPercent;

							return t * p;
						});
					else
						inputBiome.terrain3D.Foreach((x, y, z, val) => {
							return val;
						});
				}
				else
				{
					inputBiome.terrain = terrain as Sampler2D;
					inputBiome.terrain3D = terrain as Sampler3D;
				}
			}
		}
	}
}
