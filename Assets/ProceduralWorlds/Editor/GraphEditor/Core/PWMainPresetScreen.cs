using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Node;

namespace PW.Editor
{
	public class PWMainPresetScreen : PWPresetScreen
	{
		PWMainGraph	mainGraph;
	
		public PWMainPresetScreen(PWMainGraph mainGraph)
		{
			this.mainGraph = mainGraph;
			
			//loading preset panel images
			Texture2D preset2DSideViewTexture = Resources.Load< Texture2D >("preview2DSideView");
			Texture2D preset2DTopDownViewTexture = Resources.Load< Texture2D >("preview2DTopDownView");
			Texture2D preset3DPlaneTexture = Resources.Load< Texture2D >("preview3DPlane");
			Texture2D preset3DSphericalTexture = Resources.Load< Texture2D >("preview3DSpherical");
			Texture2D preset3DCubicTexture = Resources.Load< Texture2D >("preview3DCubic");

			//Biomes
			PresetCellList	biomePresets = new PresetCellList
			{
				{"Earth like", null, "Plain, mountains, beach and oceans", SelectEarthLikeBiome, true}
			};
			
			//Terrain types
			PresetCellList terrainTypePresets = new PresetCellList
			{
				{"Flat terrain", null, "Flat infinite terrain like minecraft", SelectFlatTerrain, true, biomePresets},
				{"Spherical terrain", null, "Spherical terrain link no man's sky", SelectSphericalTerrain, false, biomePresets},
				{"Cubic terrain", null, "Cubic terrain like stellar overload", SelectCubicTerrain, false, biomePresets},
			};

			//ISO surfaces
			PresetCellList terrain2DIsoSurfaces = new PresetCellList
			{
				{"Square", null, "Square map", SelectSquareIsoSurface, true, terrainTypePresets},
				{"Hexagon", null, "Hexagon map", SelectHexagonIsoSurface, false, terrainTypePresets},
				{"Marching cubes", null, "Marching cubes 2D map", SelectMarchingCubesIsoSurface, false, terrainTypePresets},
			};

			PresetCellList terrain3DIsoSurfaces = new PresetCellList
			{
				{"Marching cubes", null, "Marching cubes 3D map", SelectMarchingCubesIsoSurface, false, terrainTypePresets},
				{"Dual countering", null, "Dual countering 3D map", SelectDualCounteringIsoSurface, false, terrainTypePresets}
			};
			
			//Output type
			PresetCellList terrain3DPresets = new PresetCellList
			{
				{"3D Terrains", preset3DPlaneTexture , "3D plane procedural terrain", Select3DPlanar, false, terrain3DIsoSurfaces},
				{"3D Terrains", preset3DSphericalTexture, "3D spherical procedural terrain", Select3DSpherical, false, terrain3DIsoSurfaces},
				{"3D Terrains", preset3DCubicTexture , "3D cubic procedural terrain", Select3DCubic, false, terrain3DIsoSurfaces},
			};
			
			PresetCellList	terrain2DPresets = new PresetCellList
			{
				{"Top down", preset2DTopDownViewTexture, "Top down terrain like civilization", SelectTopDown2D, true, terrain2DIsoSurfaces},
				{"Side view", preset2DSideViewTexture, "Side view terrain like terraria", SelectSideView2D, false},
			};
			
			PresetCellList	outputTypePresets = new PresetCellList
			{
				{"2D Terrain", preset2DTopDownViewTexture, "2D Terrain like civilization", Select2DTerrain, true, terrain2DPresets},
				{"3D Terrain", preset3DPlaneTexture, "3D Terrain like minecraft", Select3DTerrain, false, terrain3DPresets},
			};

			LoadPresetList(outputTypePresets);
		}
	
		void ImportGraphTextAsset(string path)
		{
			var file = Resources.Load< TextAsset >(path);
	
			PWGraphBuilder.FromGraph(mainGraph)
				.ImportCommands(file.text.Split('\n'))
				.Execute();
		}
	
		#region output types

		void Select2DTerrain() {}

		void Select3DTerrain() {}

		void SelectTopDown2D() {}

		void SelectSideView2D() {}

		#endregion

		#region Terrain types

		void SelectFlatTerrain() {}

		void SelectCubicTerrain() {}

		void SelectSphericalTerrain() {}

		void Select3DPlanar() {}

		void Select3DSpherical() {}

		void Select3DCubic() {}

		#endregion

		#region Iso surfaces

		void SelectDualCounteringIsoSurface() {}

		void SelectHexagonIsoSurface() {}

		void SelectSquareIsoSurface() {}

		void SelectMarchingCubesIsoSurface() {}

		#endregion

		#region Biomes

		void SelectEarthLikeBiome() {}

		#endregion

		public override void OnBuildPressed()
		{
			ImportGraphTextAsset("GraphPresets/Main/TopDown2D");
			var biomeNodes = mainGraph.FindNodesByType< PWNodeBiome >();
			// var biomeGraph1 = CreateAndLinkBiomeGraph("GraphPresets/Biome/Realistic/Plain", 0);
			
			currentGraph.presetChoosed = true;
			currentGraph.UpdateComputeOrder();
		}
	
	}
}