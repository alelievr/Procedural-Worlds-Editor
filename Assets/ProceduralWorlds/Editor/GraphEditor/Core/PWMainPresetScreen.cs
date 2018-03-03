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
			Texture2D preset1DDensityFieldTexture= Resources.Load< Texture2D >("preview1DDensityField");
			Texture2D preset2DDensityFieldTexture = Resources.Load< Texture2D >("preview2DDensityField");
			Texture2D preset3DDensityFieldTexture = Resources.Load< Texture2D >("preview3DDensityField");
			// Texture2D presetMeshTetxure = Resources.Load< Texture2D >("previewMesh");
			
			PresetCellList	presets = new PresetCellList()
			{
				{"2D Terrains", preset2DSideViewTexture, "2D sideview procedural terrain", Build2DSideView, false},
				{"2D Terrains", preset2DTopDownViewTexture, "2D top down procedural terrain", Build2DTopDown, true},
				{"3D Terrains", preset3DPlaneTexture , "3D plane procedural terrain", Build3DPlanar, false},
				{"3D Terrains", preset3DSphericalTexture, "3D spherical procedural terrain", Build3DSpherical, false},
				{"3D Terrains", preset3DCubicTexture , "3D cubic procedural terrain", Build3DCubic, false},
				{"Density Fields", preset1DDensityFieldTexture, "1D float density field", Build1DDensity, false},
				{"Density Fields", preset2DDensityFieldTexture, "2D float density field", Build2DDensity, false},
				{"Density Fields", preset3DDensityFieldTexture, "3D float density field", Build3DDensity, false},
			};
		
			LoadPresetList(presets);
		}
	
		void ImportGraphTextAsset(string path)
		{
			var file = Resources.Load< TextAsset >(path);
	
			PWGraphBuilder.FromGraph(mainGraph)
				.ImportCommands(file.text.Split('\n'))
				.Execute();
		}
	
		void Build2DSideView()
		{
			//TODO
		}
	
		void Build2DTopDown()
		{
			ImportGraphTextAsset("GraphPresets/Main/TopDown2D");
		}
	
		void Build3DPlanar()
		{
			//TODO
		}
	
		void Build3DSpherical()
		{
			//TODO
		}
	
		void Build3DCubic()
		{
			//TODO
		}
	
		void Build1DDensity()
		{
			//TODO
		}
	
		void Build2DDensity()
		{
			//TODO
		}
	
		void Build3DDensity()
		{
			//TODO
		}
		
	}
}