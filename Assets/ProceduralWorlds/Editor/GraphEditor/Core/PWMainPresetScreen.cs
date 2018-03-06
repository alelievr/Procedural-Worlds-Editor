using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Node;
using System.IO;

namespace PW.Editor
{
	public class PWMainPresetScreen : PWPresetScreen
	{
		PWMainGraph		mainGraph;

		readonly string	graphFilePrefix = "GraphPresets/Main/Parts/";
		readonly string biomeAssetPrefix = "GraphPresets/Biome/Full/";
	
		public PWMainPresetScreen(PWMainGraph mainGraph)
		{
			this.mainGraph = mainGraph;
			
			//loading preset panel images
			Texture2D preset2DSideViewTexture = Resources.Load< Texture2D >("PresetImages/preview2DSideView");
			Texture2D preset2DPlanarTexture = Resources.Load< Texture2D >("PresetImages/preview2DTopDownView");
			Texture2D preset3DPlanarTexture = Resources.Load< Texture2D >("PresetImages/preview3DPlane");
			Texture2D preset3DSphericalTexture = Resources.Load< Texture2D >("PresetImages/preview3DSpherical");
			Texture2D preset3DCubicTexture = Resources.Load< Texture2D >("PresetImages/preview3DCubic");

			//Biomes
			PresetCellList	biomePresets = new PresetCellList
			{
				{"Earth like", null, "Biomes/Earth", true}
			};
			
			//ISO surfaces
			PresetCellList terrain2DIsoSurfaces = new PresetCellList
			{
				{"Square", null, "IsoSurfaces/Square", true, biomePresets},
				{"Hexagon", null, "IsoSurfaces/Hexagon", false, biomePresets},
				{"Marching cubes 2D", null, "IsoSurfaces/MarchingCubes2D", false, biomePresets},
				{"Fake voxels", null, "IsoSurfaces/FakeVolxel", false, biomePresets},
			};

			PresetCellList terrain3DIsoSurfaces = new PresetCellList
			{
				{"Marching cubes 3D", null, "IsuSurfaces/MarchingCubes3D", false, biomePresets},
				{"Dual countering 3D", null, "IsoSurfaces/DualCountering", false, biomePresets},
				{"Greedy voxels", null, "IsoSurfaces/GreedyVoxel", false, biomePresets}
			};
			
			//Output type
			PresetCellList terrain3DPresets = new PresetCellList
			{
				{"3D planar", preset3DPlanarTexture, "TerrainType/Planar3D", false, terrain3DIsoSurfaces},
				{"3D spherical", preset3DSphericalTexture, "TerrainType/Spherical3D", false, terrain3DIsoSurfaces},
				{"3D cubic", preset3DCubicTexture, "TerrainType/Cubic3D", false, terrain3DIsoSurfaces},
			};
			
			PresetCellList	terrain2DPresets = new PresetCellList
			{
				{"2D flat", preset2DPlanarTexture, "TerrainType/Planar2D", true, terrain2DIsoSurfaces},
				{"2D spherical", null, "TerrainType/Spherical2D", false, terrain2DIsoSurfaces},
				{"2D cubic", preset2DPlanarTexture, "TerrainType/Cubic2D", false, terrain2DIsoSurfaces},
				{"Side view like terraria", preset2DSideViewTexture, "Base/SideView", false},
			};
			
			PresetCellList	outputTypePresets = new PresetCellList
			{
				{"2D Terrain like civilization", preset2DPlanarTexture, "Base/2D", true, terrain2DPresets},
				{"3D Terrain like minecraft", preset3DPlanarTexture, "Base/3D", false, terrain3DPresets},
			};

			LoadPresetList(outputTypePresets);
		}
	
		void ImportGraphTextAsset(string path, PWGraphBuilder builder)
		{
			var file = Resources.Load< TextAsset >(path);
	
			builder.ImportCommands(file.text.Split('\n'));
		}

		List< PWBiomeGraph > CopyBiomesFromPreset(string biomeFolder)
		{
			List< PWBiomeGraph > biomes = new List< PWBiomeGraph >();

			string graphPath = mainGraph.assetFilePath;
			string biomeTargetPath = Path.GetDirectoryName(graphPath) + "/" + PWGraphFactory.PWGraphBiomeFolderName + "/";
			
			var biomeGraphs = Resources.LoadAll< PWBiomeGraph >(biomeAssetPrefix + biomeFolder);
			for (int i = 0; i < biomeGraphs.Length; i++)
			{
				string name = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(biomeGraphs[i]));
				var bg = biomeGraphs[i].Clone() as PWBiomeGraph;
				string path = biomeTargetPath + name + ".asset";

				AssetDatabase.CreateAsset(bg, path);
				foreach (var node in bg.nodes)
					AssetDatabase.AddObjectToAsset(node, bg);
				
				//Set our graph into biome graph input
				(bg.inputNode as PWNodeBiomeGraphInput).previewGraph = mainGraph;

				biomes.Add(bg);
			}
			
			return biomes;
		}
	
		public override void OnBuildPressed()
		{
			PWGraphBuilder builder = PWGraphBuilder.FromGraph(mainGraph);
			List< PWBiomeGraph > biomes = null;

			foreach (var graphPartFile in graphPartFiles)
			{
				var file = Resources.Load< TextAsset >(graphFilePrefix + graphPartFile);
				builder.ImportCommands(file.text.Split('\n'));

				if (graphPartFile.StartsWith("Biomes/"))
					biomes = CopyBiomesFromPreset(Path.GetFileName(graphPartFile));
			}
			
			builder.Execute();
			
			var biomeNodes = mainGraph.FindNodesByType< PWNodeBiome >();
			for (int i = 0; i < biomeNodes.Count; i++)
			{
				biomeNodes[i].biomeGraph = biomes[i];
			}

			builder.GetGraph().Process();
			
			currentGraph.presetChoosed = true;
		}
	
	}
}