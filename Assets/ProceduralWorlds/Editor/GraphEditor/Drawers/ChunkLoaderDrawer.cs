using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;
using System;
using ProceduralWorlds;

namespace ProceduralWorlds.Editor
{
	public class ChunkLoaderDrawer : Drawer
	{
		WorldGraph			worldGraph;

		UnityEditor.Editor	terrainEditor;

		GenericBaseTerrain	oldTerrain;

		public override void OnEnable()
		{
			worldGraph =  target as WorldGraph;

			oldTerrain = TerrainPreviewManager.instance.BaseTerrain;
		}

		new public void OnGUI(Rect r)
		{
			base.OnGUI(r);

			var terrain = TerrainPreviewManager.instance.BaseTerrain;

			if (terrain == null)
			{
				if (TerrainPreviewManager.instance.previewRoot == null)
					EditorGUILayout.HelpBox("You must load the preview to activate chunk generation", MessageType.Warning);
				else
					EditorGUILayout.HelpBox("Terrain materializer type not supported (" + worldGraph.terrainPreviewType + ")", MessageType.Warning);
				return ;
			}

			if (terrainEditor == null || oldTerrain != terrain)
				terrainEditor = UnityEditor.Editor.CreateEditor(terrain);

			terrainEditor.OnInspectorGUI();
			
			if (GUILayout.Button("Focus"))
				Selection.activeObject = terrain;

			oldTerrain = terrain;
		}
	}
}