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

	[SerializeField]
	bool					scaledPreviewFoldout;
	[SerializeField]
	bool					terrainSettingsFoldout;
	[SerializeField]
	bool					geologicalSettingsFoldout;

	const string			graphProcessKey = "PWMainGraphEditor";

#region Initialization and data baking

	[MenuItem("Window/Procedural Worlds/Main Graph", priority = 1)]
	static void Init()
	{
		PWMainGraphEditor window = (PWMainGraphEditor)EditorWindow.GetWindow (typeof (PWMainGraphEditor));

		window.Show();
	}

	public override void OnEnable()
	{
		base.OnEnable();

		OnWindowResize += WindowResizeCallback;
		OnGraphChanged += GraphChangedCallback;
		
		layout.onDrawNodeSelector = (rect) => nodeSelectorBar.DrawNodeSelector(rect);
		layout.onDrawOptionBar = (rect) => optionBar.DrawOptionBar(rect);
		layout.onDrawSettingsBar = (rect) => settingsBar.Draw(rect);

		delayedChanges.BindCallback(graphProcessKey, (unsued) => {
			graph.Process();
		});
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

	void WindowResizeCallback(Vector2 oldSize)
	{
		layout.ResizeWindow(oldSize, position);
	}

	void GraphChangedCallback(PWGraph newGraph)
	{
		if (newGraph == null)
			return ;
		
		terrainManager = new PWGraphTerrainManager(graph);

		settingsBar.onDraw = (rect) =>
		{
			settingsBar.DrawDefault(rect);

			if (PWGUI.BeginFade("Scaled preview", ref scaledPreviewFoldout, false))
			{
				EditorGUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("Active", (mainGraph.scaledPreviewEnabled) ? PWStyles.pressedButton : PWStyles.button))
					{
						mainGraph.scaledPreviewEnabled = !mainGraph.scaledPreviewEnabled;
						mainGraph.Process();
					}
				}
				EditorGUILayout.EndHorizontal();

				mainGraph.scaledPreviewRatio = EditorGUILayout.Slider("", mainGraph.scaledPreviewRatio, 1, 128);
				mainGraph.scaledPreviewChunkSize = EditorGUILayout.IntSlider("", mainGraph.scaledPreviewChunkSize, mainGraph.chunkSize, 1024);
			}
			PWGUI.EndFade();

			if (PWGUI.BeginFade("Terrain settings", ref terrainSettingsFoldout, false))
			{
				terrainManager.DrawTerrainSettings(rect, mainGraph.materializerType);
			}
			PWGUI.EndFade();

			/*if (PWGUI.BeginFade("Geological settings", ref geologicalSettingsFoldout, false))
			{
				mainGraph.geologicTerrainStep = graph.PWGUI.Slider("Geological terrain step: ", mainGraph.geologicTerrainStep, 4, 64);
				mainGraph.geologicDistanceCheck = graph.PWGUI.IntSlider("Geological search distance: ", mainGraph.geologicDistanceCheck, 1, 4);
			}
			PWGUI.EndFade();*/

			EditorGUI.BeginChangeCheck();
			{
				//seed
				GUI.SetNextControlName("seed");
				mainGraph.seed = EditorGUILayout.IntField("Seed", mainGraph.seed);
				
				EditorGUI.BeginDisabledGroup(mainGraph.scaledPreviewEnabled);
				{
					//chunk size:
					GUI.SetNextControlName("chunk size");
					mainGraph.chunkSize = EditorGUILayout.IntField("Chunk size", mainGraph.chunkSize);
					mainGraph.chunkSize = Mathf.Clamp(mainGraph.chunkSize, 1, 1024);
		
					//step:
					float min = 0.1f;
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("step", PWStyles.prefixLabel);
					mainGraph.step = mainGraph.PWGUI.Slider(mainGraph.step, ref min, ref mainGraph.maxStep, 0.01f, false, true);
					EditorGUILayout.EndHorizontal();
				}
				EditorGUI.EndDisabledGroup();
			}
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(graphProcessKey);
		};
	}

	void LoadStyles()
	{
		layout.LoadStyles(position);
	}

}