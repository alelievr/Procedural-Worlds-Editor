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
		
		public PWTerrainOutputMode	outputMode;
		
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

		static Dictionary< PWTerrainOutputMode, List< string > > propertiesPerOutputType = new Dictionary< PWTerrainOutputMode, List< string > >()
		{
			{ PWTerrainOutputMode.TopDown2D, new List< string >() {"terrainSurface"} },
			{ PWTerrainOutputMode.Planar3D, new List< string >() {""} },
		};

		void UpdateOutputType()
		{
			foreach (var kp in propertiesPerOutputType)
				if (outputMode == kp.Key)
					foreach (var propName in kp.Value)
						UpdatePropVisibility(propName, PWVisibility.Visible);
				else
					foreach (var propName in kp.Value)
						UpdatePropVisibility(propName, PWVisibility.Gone);
		}

		public override void OnNodeCreate()
		{
			externalName = "Biome binder";
		}

		public override void OnNodeGUI()
		{
			PWTerrainOutputMode	oldTerrainMode = outputMode;
				outputMode = GetTerrainOutputMode();
			if (outputMode != oldTerrainMode)
				UpdateOutputType();

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

		public override bool OnNodeAnchorLink(string prop, int index)
		{
			if (prop == "inputBiome" && outputBiome != null)
				outputBiome.biomeDataReference = inputBiome;
			return true;
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
