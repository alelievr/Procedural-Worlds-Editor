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
	PWLayout				layout;
	
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

		OnGraphChanged += GraphChangedCallback;
		
		layout = PWLayoutFactory.Create2ResizablePanelLayout(this);

		delayedChanges.BindCallback(graphProcessKey, (unsued) => {
			graph.Process();
		});
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

		if (!mainGraph.presetChoosed)
		{
			if (presetScreen == null)
				presetScreen = new PWMainPresetScreen(mainGraph);
			
			var newGraph = presetScreen.Draw(position, graph);

			if (newGraph != graph)
				LoadGraph(newGraph);
			
			return ;
		}
		
		//reset current layout:
		if (e.type == EventType.KeyDown && e.keyCode == KeyCode.R && e.shift)
		{
			if (graph != null)
			{
				graph.layoutSettings.settings.Clear();
				layout = PWLayoutFactory.Create2ResizablePanelLayout(this);
				e.Use();
			}
		}

		layout.DrawLayout();
    }

	#endregion

	void GraphChangedCallback(PWGraph newGraph)
	{
		if (newGraph == null)
			return ;

		/*settingsBar.onDraw = (rect) =>
		{
			settingsBar.DrawDefault(rect);
			
			EditorGUI.BeginChangeCheck();

			if (PWGUI.BeginFade("Scaled preview", ref scaledPreviewFoldout, false))
			{
				EditorGUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("Active", (mainGraph.scaledPreviewEnabled) ? PWStyles.pressedButton : PWStyles.button))
					{
						mainGraph.scaledPreviewEnabled = !mainGraph.scaledPreviewEnabled;
						PWGUIManager.displaySamplerStepBounds = mainGraph.scaledPreviewEnabled;
						mainGraph.Process();
					}
				}
				EditorGUILayout.EndHorizontal();

				mainGraph.scaledPreviewRatio = EditorGUILayout.Slider("Ratio", mainGraph.scaledPreviewRatio, 1, 128);
				mainGraph.scaledPreviewChunkSize = EditorGUILayout.IntSlider("Chunk size", mainGraph.scaledPreviewChunkSize, 32, 2048);
				float scale = (mainGraph.scaledPreviewRatio * mainGraph.scaledPreviewChunkSize) / (mainGraph.nonModifiedChunkSize * mainGraph.nonModifiedStep);
				EditorGUILayout.LabelField("Scale: " + scale);
			}
			PWGUI.EndFade();

			if (PWGUI.BeginFade("Renderer settings", ref terrainSettingsFoldout, false))
			{
				terrainManager.DrawTerrainSettings(rect, mainGraph.materializerType);
			}
			PWGUI.EndFade();

			//Activate this when the geological update will be ready
			#if false
				if (PWGUI.BeginFade("Geological settings", ref geologicalSettingsFoldout, false))
				{
					mainGraph.geologicTerrainStep = graph.PWGUI.Slider("Geological terrain step: ", mainGraph.geologicTerrainStep, 4, 64);
					mainGraph.geologicDistanceCheck = graph.PWGUI.IntSlider("Geological search distance: ", mainGraph.geologicDistanceCheck, 1, 4);
				}
				PWGUI.EndFade();
			#endif

			if (PWGUI.BeginFade("Chunk settings"))
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
					mainGraph.step = PWGUI.Slider(mainGraph.step, ref min, ref mainGraph.maxStep, 0.01f, false, true);
					EditorGUILayout.EndHorizontal();
				}
				EditorGUI.EndDisabledGroup();
			}
			PWGUI.EndFade();
			
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(graphProcessKey);
		};*/
	}

}