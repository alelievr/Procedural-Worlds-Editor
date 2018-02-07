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
	[System.NonSerialized]
	Vector2					currentMousePosition;

	//multi-node selection
	[System.NonSerialized]
	Rect					selectionRect;

	[System.NonSerialized]
	PWMainPresetScreen		presetScreen;


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

		OnWindowResize += WindowResizeCallback;
		OnGraphChanged += GraphChangedCallback;
		
		layout.onDrawNodeSelector = (rect) => nodeSelectorBar.DrawNodeSelector(rect);
		layout.onDrawOptionBar = (rect) => optionBar.DrawOptionBar(rect);
		layout.onDrawSettingsBar = (rect) => settingsBar.DrawSettingsBar(rect);
	}

	public override void OnGUIEnable()
	{
		base.OnGUIEnable();
		
		LoadStyles();
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
			if (presetScreen == null)
				presetScreen = new PWMainPresetScreen(mainGraph);
			
			var newGraph = presetScreen.Draw(position, graph);

			if (newGraph != graph)
				LoadGraph(newGraph);
			
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
		layout.LoadStyles(position);
	}

}