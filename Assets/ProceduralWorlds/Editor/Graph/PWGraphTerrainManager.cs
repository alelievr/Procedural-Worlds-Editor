using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Editor
{
	public class PWGraphTerrainManager
	{
		PWTerrainBase		terrain;

		PWGraph				graph;

		public PWGraphTerrainManager(PWGraph graph)
		{
			this.graph = graph;
		}

		public void LoadStyles()
		{
			
		}
	
		public void DrawTerrainSettings(Rect settingsRect)
		{
			terrain.renderDistance = EditorGUILayout.IntSlider("chunk Render distance", terrain.renderDistance, 0, 24);
					
			terrain.chunkSize = graph.chunkSize;
		}
	}
}