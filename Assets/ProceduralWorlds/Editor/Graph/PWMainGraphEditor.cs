using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;
using PW.Core;
using PW.Node;
using PW.Editor;
using Object = UnityEngine.Object;

[System.Serializable]
public partial class PWMainGraphEditor : PWGraphEditor
{

	[SerializeField]
	PWGraphLayout			layout = new PWGraphLayout();
	
	//terrain manager to configure terrain generation / preview
	PWGraphTerrainManager	terrainManager;
	
	//events fields
	Vector2					lastMousePosition;
	[System.NonSerializedAttribute]
	Vector2					currentMousePosition;

	//multi-node selection
	[System.NonSerializedAttribute]
	Rect					selectionRect;

#region Internal editor styles and textures

	private static Texture2D	preset2DSideViewTexture;
	private static Texture2D	preset2DTopDownViewTexture;
	private static Texture2D	preset3DPlaneTexture;
	private static Texture2D	preset3DSphericalTexture;
	private static Texture2D	preset3DCubicTexture;
	private static Texture2D	preset1DDensityFieldTexture;
	private static Texture2D	preset2DDensityFieldTexture;
	private static Texture2D	preset3DDensityFieldTexture;
	private static Texture2D	presetMeshTetxure;

#endregion

#region Initialization and data baking

	[MenuItem("Window/Procedural Worlds/Main Graph", priority = 1)]
	static void Init()
	{
		PWMainGraphEditor window = (PWMainGraphEditor)EditorWindow.GetWindow (typeof (PWMainGraphEditor));

		window.Show();
	}

/*	void InitializeNewGraph(PWNodeGraph graph)
	{
		//setup splitted panels:
	}*/

	public override void OnEnable()
	{
		base.OnEnable();
		
		LoadStyles();
		
		LoadAssets();

		OnWindowResize += WindowResizeCallback;
		OnGraphChanged += GraphChangedCallback;
		
		layout.onDrawNodeSelector = (rect) => nodeSelectorBar.DrawNodeSelector(rect);
		layout.onDrawOptionBar = (rect) => optionBar.DrawOptionBar(rect);
		layout.onDrawSettingsBar = (rect) => settingsBar.DrawSettingsBar(rect);
	}

	public override void OnDisable()
	{
		base.OnDisable();

		OnWindowResize -= WindowResizeCallback;
		OnGraphChanged -= GraphChangedCallback;
	}

#endregion

#region Global GUI rendering

	//call all rendering methods:
    public override void OnGUI()
    {
		//render the whole graph
		base.OnGUI();

		//quit if the graph have been destroyed / does not exists
		if (graph == null)
			return ;

		if (!mainGraph.presetChoosed)
		{
			DrawPresetPanel();
			return ;
		}

		layout.Render2ResizablePanel(this, position);
    }

#endregion

	void WindowResizeCallback(Vector2 newSize)
	{
		layout.ResizeWindow(newSize, position);
	}

	void GraphChangedCallback(PWGraph newGraph)
	{
		if (newGraph == null)
			return ;
		
		terrainManager = new PWGraphTerrainManager(graph);

		settingsBar.onDrawAdditionalSettings = (rect) =>
		{
			terrainManager.DrawTerrainSettings(rect, mainGraph.materializerType);

			//Main graph sepcific datas:
			mainGraph.geologicTerrainStep = graph.PWGUI.Slider("Geological terrain step: ", mainGraph.geologicTerrainStep, 4, 64);
			mainGraph.geologicDistanceCheck = graph.PWGUI.IntSlider("Geological search distance: ", mainGraph.geologicDistanceCheck, 1, 4);
		};
	}

	void LoadStyles()
	{
	}

	void LoadAssets()
	{
		layout.LoadStyles(position);

		//loading preset panel images
		preset2DSideViewTexture = Resources.Load< Texture2D >("preview2DSideView");
		preset2DTopDownViewTexture = Resources.Load< Texture2D >("preview2DTopDownView");
		preset3DPlaneTexture = Resources.Load< Texture2D >("preview3DPlane");
		preset3DSphericalTexture = Resources.Load< Texture2D >("preview3DSpherical");
		preset3DCubicTexture = Resources.Load< Texture2D >("preview3DCubic");
		presetMeshTetxure = Resources.Load< Texture2D >("previewMesh");
		preset1DDensityFieldTexture= Resources.Load< Texture2D >("preview1DDensityField");
		preset2DDensityFieldTexture = Resources.Load< Texture2D >("preview2DDensityField");
		preset3DDensityFieldTexture = Resources.Load< Texture2D >("preview3DDensityField");
	}

}