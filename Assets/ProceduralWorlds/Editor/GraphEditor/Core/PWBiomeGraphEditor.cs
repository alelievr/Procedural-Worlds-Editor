using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using PW;
using PW.Core;
using PW.Editor;
using PW.Biomator;

public class PWBiomeGraphEditor : PWGraphEditor
{
	
	[SerializeField]
	PWGraphLayout			layout = new PWGraphLayout();

	List< PWBiomeGraph >	biomeGraphs = new List< PWBiomeGraph >();
	ReorderableList			biomeGraphList;

	[System.NonSerialized]
	PWBiomePresetScreen		presetScreen;

	[MenuItem("Window/Procedural Worlds/Biome Graph", priority = 2)]
	static void Init()
	{
		PWBiomeGraphEditor window = (PWBiomeGraphEditor)GetWindow(typeof(PWBiomeGraphEditor));
		window.Show();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		
		OnWindowResize += WindowResizeCallback;
		OnGraphChanged += GraphLoadedCallback;

		layout.LoadStyles(position);

		biomeGraphList = new ReorderableList(biomeGraphs, typeof(PWBiomeGraph), false, true, false, false);

		biomeGraphList.drawElementCallback = (rect, index, active, focus) => {
			EditorGUI.LabelField(rect, biomeGraphs[index].name);
			rect.x += rect.width - 50;
			rect.width = 50;
			rect.height = EditorGUIUtility.singleLineHeight;
			if (GUI.Button(rect, "Open"))
				LoadGraph(biomeGraphs[index]);
		};
		biomeGraphList.drawHeaderCallback = (rect) => {
			EditorGUI.LabelField(rect, "Biome list");
		};

		LoadGraphList();
		
		//TODO: layout
		// layout.onDrawNodeSelector = (rect) => nodeSelectorBar.DrawNodeSelector(rect);
		// layout.onDrawOptionBar = (rect) => optionBar.DrawOptionBar(rect);
		// layout.onDrawSettingsBar = (rect) => settingsBar.Draw(rect);
	}

	void LoadGraphList()
	{
		biomeGraphs.Clear();
		var resGraphs = Resources.FindObjectsOfTypeAll< PWBiomeGraph >();

		foreach (var biomeGraph in resGraphs)
			biomeGraphs.Add(biomeGraph);
	}

	void GraphLoadedCallback(PWGraph graph)
	{
		if (graph == null)
			return ;
		
		// settingsBar.onDraw = DrawBiomeSettingsBar;
	}

	void DrawBiomeSettingsBar(Rect rect)
	{
		// settingsBar.DrawTerrainPreview(rect);

		GUI.SetNextControlName("PWName");
		graph.name = EditorGUILayout.TextField("Biome name: ", graph.name);

		EditorGUILayout.Space();

		using (DefaultGUISkin.Get())
			biomeGraphList.DoLayoutList();
		
		if (GUILayout.Button("Refresh"))
			LoadGraphList();

		EditorGUILayout.Space();

		biomeGraph.surfaceType = (BiomeSurfaceType)EditorGUILayout.EnumPopup("Biome surface type", biomeGraph.surfaceType);
	}

	public override void OnGUI()
	{
		//draw the node editor
		base.OnGUI();
		
		if (graph == null)
			return ;
		
		if (!biomeGraph.presetChoosed)
		{
			if (presetScreen == null)
				presetScreen = new PWBiomePresetScreen(biomeGraph);
			
			var newGraph = presetScreen.Draw(position, graph);

			if (newGraph != graph)
				LoadGraph(newGraph);

			return ;
		}

		layout.Render2ResizablePanel(this, position);
	}
	
	void WindowResizeCallback(Vector2 oldSize)
	{
		layout.ResizeWindow(oldSize, position);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		
		OnWindowResize -= WindowResizeCallback;
		OnGraphChanged -= GraphLoadedCallback;
	}

}
