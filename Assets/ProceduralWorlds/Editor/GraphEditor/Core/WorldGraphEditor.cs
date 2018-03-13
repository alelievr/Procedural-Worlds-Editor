using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;
using ProceduralWorlds.Editor;
using Object = UnityEngine.Object;

[System.Serializable]
public partial class WorldGraphEditor : BaseGraphEditor
{
	
	//events fields
	[System.NonSerialized]
	Vector2					currentMousePosition;

	//multi-node selection
	[System.NonSerialized]
	Rect					selectionRect;

	[System.NonSerialized]
	WorldPresetScreen		presetScreen;

	[SerializeField]
	bool					scaledPreviewFoldout = true;
	[SerializeField]
	bool					terrainSettingsFoldout;
	[SerializeField]
	bool					geologicalSettingsFoldout;
	[SerializeField]
	bool					chunkSettingsFoldout = true;
	[SerializeField]
	bool					chunkLoaderFoldout = true;

	const string			graphProcessKey = "WorldGraphEditor";

	ChunkLoaderDrawer		chunkLoaderDrawer = new ChunkLoaderDrawer();

	#region Initialization and data baking

	[MenuItem("Window/Procedural Worlds/World Graph", priority = 1)]
	static void Init()
	{
		WorldGraphEditor window = (WorldGraphEditor)EditorWindow.GetWindow (typeof (WorldGraphEditor));

		window.Show();
	}

	public override void OnEnable()
	{
		base.OnEnable();

		OnGraphChanged += GraphChangedCallback;

		OnResetLayout += ResetLayoutCallback;

		delayedChanges.BindCallback(graphProcessKey, (unsued) => {
			graph.Process();
		});

		LoadGUI();
	}

	public override void OnDisable()
	{
		base.OnDisable();

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

		if (!worldGraph.presetChoosed)
		{
			if (presetScreen == null)
				presetScreen = new WorldPresetScreen(worldGraph);
			
			var newGraph = presetScreen.Draw(position, graph);

			//we initialize the layout once the user choosed the preset to generate
			if (worldGraph.presetChoosed)
				ResetLayout();

			if (newGraph != graph)
				LoadGraph(newGraph);
			
			return ;
		}
		
		layout.DrawLayout();
    }

	#endregion

	void GraphChangedCallback(BaseGraph newGraph)
	{
		if (newGraph == null)
			return ;
		
		string path = AssetDatabase.GetAssetPath(newGraph);
		if (String.IsNullOrEmpty(GraphFactory.GetWorldGraphCreateLocation(path)))
			Debug.LogWarning("Your main graph is not inside a Resources folder/don't have a dedicated folder");
	}

	void ResetLayoutCallback()
	{
		LoadGUI();
	}

	void LoadGUI()
	{
		var settingsPanel = layout.GetPanel< BaseGraphSettingsPanel >();

		settingsPanel.onGUI = (rect) =>
		{
			settingsPanel.DrawDefault(rect);
			
			EditorGUI.BeginChangeCheck();

			if (PWGUI.BeginFade("Scaled preview", ref scaledPreviewFoldout, false))
			{
				EditorGUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("Active", (worldGraph.scaledPreviewEnabled) ? Styles.pressedButton : Styles.button))
					{
						worldGraph.scaledPreviewEnabled = !worldGraph.scaledPreviewEnabled;
						ProceduralWorldsGUI.displaySamplerStepBounds = worldGraph.scaledPreviewEnabled;
						worldGraph.Process();
					}
				}
				EditorGUILayout.EndHorizontal();

				worldGraph.scaledPreviewRatio = EditorGUILayout.Slider("Ratio", worldGraph.scaledPreviewRatio, 1, 128);
				worldGraph.scaledPreviewChunkSize = EditorGUILayout.IntSlider("Chunk size", worldGraph.scaledPreviewChunkSize, 32, 2048);
				float scale = (worldGraph.scaledPreviewRatio * worldGraph.scaledPreviewChunkSize) / (worldGraph.nonModifiedChunkSize * worldGraph.nonModifiedStep);
				EditorGUILayout.LabelField("Scale: " + scale);
			}
			PWGUI.EndFade();

			//Activate this when the geological update will be ready
			#if false
				if (PWGUI.BeginFade("Geological settings", ref geologicalSettingsFoldout, false))
				{
					worldGraph.geologicTerrainStep = graph.PWGUI.Slider("Geological terrain step: ", worldGraph.geologicTerrainStep, 4, 64);
					worldGraph.geologicDistanceCheck = graph.PWGUI.IntSlider("Geological search distance: ", worldGraph.geologicDistanceCheck, 1, 4);
				}
				PWGUI.EndFade();
			#endif

			if (PWGUI.BeginFade("Chunk settings", ref chunkSettingsFoldout, false))
			{
				//seed
				GUI.SetNextControlName("seed");
				worldGraph.seed = EditorGUILayout.IntField("Seed", worldGraph.seed);
				
				EditorGUI.BeginDisabledGroup(worldGraph.scaledPreviewEnabled);
				{
					//chunk size:
					GUI.SetNextControlName("chunk size");
					worldGraph.chunkSize = EditorGUILayout.IntField("Chunk size", worldGraph.chunkSize);
					worldGraph.chunkSize = Mathf.Clamp(worldGraph.chunkSize, 1, 1024);
		
					//step:
					float min = 0.1f;
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("step", Styles.prefixLabel);
					worldGraph.step = PWGUI.Slider(worldGraph.step, ref min, ref worldGraph.maxStep, 0.01f, false, true);
					EditorGUILayout.EndHorizontal();
				}
				EditorGUI.EndDisabledGroup();
			}
			PWGUI.EndFade();

			if (PWGUI.BeginFade("Chunkloader settings", ref chunkLoaderFoldout, false))
			{
				if (!chunkLoaderDrawer.isEnabled)
					chunkLoaderDrawer.OnEnable(worldGraph);
				chunkLoaderDrawer.OnGUI(new Rect());
			}
			PWGUI.EndFade();
			
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(graphProcessKey);
		};
	}

}