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
		[PWOffset(20)]
		public BiomeData			inputBiome;
		
		public PWGraphTerrainType	outputMode;
		
		//inputs for 2D topdown map
		[PWInput("Surfaces")]
		[PWNotRequired]
		public BiomeSurfaces		biomeSurfaces;

		[PWInput("Details")]
		[PWNotRequired]
		public TerrainDetail		biomeDetail;

		[PWInput("Terrain modifier")]
		[PWNotRequired]
		public BiomeTerrain			biomeTerrainModifier;

		//TODO: dispositon algos

		//inputs for 3D planar terrain
		//TODO

		[PWOutput("biome")]
		[PWOffset(20)]
		public Biome				outputBiome;

		[SerializeField]
		Rect						colorPreviewRect;

		public override void OnNodeCreation()
		{
			name = "Biome binder";
		}

		public override void OnNodeGUI()
		{
			EditorGUILayout.BeginHorizontal();
			if (outputBiome != null)
			{
				EditorGUIUtility.labelWidth = 72;
				EditorGUILayout.LabelField("id: " + outputBiome.id +  " (" +  outputBiome.name + ")");
				if (Event.current.type == EventType.Repaint)
				{
					colorPreviewRect = EditorGUILayout.GetControlRect();
					colorPreviewRect.width -= 88;
				}
				else
					EditorGUILayout.GetControlRect();
				EditorGUIUtility.DrawColorSwatch(colorPreviewRect, outputBiome.previewColor);
			}
			EditorGUILayout.EndHorizontal();

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
			outputBiome.biomeTerrain = biomeTerrainModifier;
			outputBiome.mode = outputMode;
			outputBiome.biomeSurfaces = biomeSurfaces;
		}

		public override void OnNodeProcessOnce()
		{
			//just pass the biomeSurfaces to the blender for processOnce:
			if (outputBiome == null)
				outputBiome = new Biome();
			
			outputBiome.biomeSurfaces = biomeSurfaces;
			outputBiome.biomeDataReference = inputBiome;
		}
	}
}
