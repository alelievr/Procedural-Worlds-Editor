using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Editor
{
	public class PWGraphTerrainManager
	{
		//terrain base game object reference
		public static PWTerrainBase	terrainReference;

		//Graph reference
		PWGraph				graph;

		PWTerrainBase		terrain;



		public PWGraphTerrainManager(PWGraph graph)
		{
			this.graph = graph;
		}

		public void LoadStyles()
		{

		}
	
		public void DrawTerrainSettings(Rect settingsRect)
		{
			if (terrain == null)
				terrain = GameObject.FindObjectOfType< PWTerrainBase >();

			if (terrain == null)
				return ;

			terrainReference = terrain;

			terrain.renderDistance = EditorGUILayout.IntSlider("chunk Render distance", terrain.renderDistance, 0, 24);
					
			terrain.chunkSize = graph.chunkSize;
		}
	}
}