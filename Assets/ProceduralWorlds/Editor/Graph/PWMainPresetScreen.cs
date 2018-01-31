using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Node;

public class PWMainPresetScreen : PWPresetScreen
{
	Texture2D	preset2DSideViewTexture;
	Texture2D	preset2DTopDownViewTexture;
	Texture2D	preset3DPlaneTexture;
	Texture2D	preset3DSphericalTexture;
	Texture2D	preset3DCubicTexture;
	Texture2D	preset1DDensityFieldTexture;
	Texture2D	preset2DDensityFieldTexture;
	Texture2D	preset3DDensityFieldTexture;
	// Texture2D	presetMeshTetxure;

	PWMainGraph	mainGraph;

	public PWMainPresetScreen(PWMainGraph mainGraph)
	{
		this.mainGraph = mainGraph;
		
		//loading preset panel images
		preset2DSideViewTexture = Resources.Load< Texture2D >("preview2DSideView");
		preset2DTopDownViewTexture = Resources.Load< Texture2D >("preview2DTopDownView");
		preset3DPlaneTexture = Resources.Load< Texture2D >("preview3DPlane");
		preset3DSphericalTexture = Resources.Load< Texture2D >("preview3DSpherical");
		preset3DCubicTexture = Resources.Load< Texture2D >("preview3DCubic");
		preset1DDensityFieldTexture= Resources.Load< Texture2D >("preview1DDensityField");
		preset2DDensityFieldTexture = Resources.Load< Texture2D >("preview2DDensityField");
		preset3DDensityFieldTexture = Resources.Load< Texture2D >("preview3DDensityField");
		// presetMeshTetxure = Resources.Load< Texture2D >("previewMesh");
		
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

	void Build2DSideView()
	{
	}

	void Build2DTopDown()
	{
		PWGraphBuilder.FromGraph(mainGraph)
			.Execute();
	}

	void Build3DPlanar()
	{

	}

	void Build3DSpherical()
	{

	}

	void Build3DCubic()
	{

	}

	void Build1DDensity()
	{

	}

	void Build2DDensity()
	{

	}

	void Build3DDensity()
	{
		
	}
	
}
