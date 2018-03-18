using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using ProceduralWorlds;
using ProceduralWorlds.Core;
using ProceduralWorlds.Editor;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Editor
{
	public class BiomeGraphEditor : BaseGraphEditor
	{
	
		List< BiomeGraph >		biomeGraphs = new List< BiomeGraph >();
		ReorderableList			biomeGraphList;
	
		[System.NonSerialized]
		BiomePresetScreen		presetScreen;
	
		[MenuItem("Window/Procedural Worlds/Biome Graph", priority = 2)]
		static void Init()
		{
			BiomeGraphEditor window = (BiomeGraphEditor)GetWindow(typeof(BiomeGraphEditor));
			window.Show();
		}
	
		public override void OnEnable()
		{
			base.OnEnable();
			
			OnGraphChanged += GraphLoadedCallback;

			OnResetLayout += ResetLayoutCallback;
	
			biomeGraphList = new ReorderableList(biomeGraphs, typeof(BiomeGraph), false, true, false, false);
	
			biomeGraphList.drawElementCallback = (rect, index, active, focus) => {
				if (index < 0 || index >= biomeGraphs.Count || biomeGraphs[index] == null)
				{
					EditorGUI.LabelField(rect, "PLease, reload the biome list");
					return ;
				}
				
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
	
			LoadGUI();
		}
	
		void LoadGraphList()
		{
			string path = AssetDatabase.GetAssetPath(biomeGraph);
			string resourcesName = GraphFactory.unityResourcesFolderName;
	
			if (String.IsNullOrEmpty(path))
				return ;
	
			path = Path.GetDirectoryName(path);
			path = path.Substring(path.IndexOf(resourcesName) + resourcesName.Length + 1);
			var graphAssets = Resources.LoadAll< BiomeGraph >(path);
	
			if (graphAssets != null && graphAssets.Length != 0)
				biomeGraphs = graphAssets.Where(b => b != null).ToList();
		}
	
		void LoadGUI()
		{
			var settingsPanel = layout.GetPanel< BaseGraphSettingsPanel >();
			
			settingsPanel.onGUI = (rect) => {
				settingsPanel.DrawTerrainPreview(rect);
				DrawBiomeSettingsBar(rect);
				settingsPanel.DrawReloadButtons();
			};
		}
	
		void GraphLoadedCallback(BaseGraph graph)
		{
			if (graph == null)
				return ;
		}
		
		void ResetLayoutCallback()
		{
			LoadGUI();
		}
	
		void DrawBiomeSettingsBar(Rect rect)
		{
			GUI.SetNextControlName("Graph Name");
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
					presetScreen = new BiomePresetScreen(this);
				
				var newGraph = presetScreen.Draw(position, graph);
				
				//we initialize the layout once the user choosed the preset to generate
				if (biomeGraph.presetChoosed)
					ResetLayout();
	
				if (newGraph != graph)
					LoadGraph(newGraph);
	
				return ;
			}
			
			layout.DrawLayout();
		}
	
		public override void OnDisable()
		{
			base.OnDisable();
			
			OnGraphChanged -= GraphLoadedCallback;
			
			OnResetLayout -= ResetLayoutCallback;
		}
	
	}
}