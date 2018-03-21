using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(TopDown2DTerrainSquare))]
	public class TopDown2DTerrainSquareInspector : TerrainBaseInspector
	{
		TopDown2DTerrainSquare	terrain;

		public override void OnEditorEnable()
		{
			terrain = target as TopDown2DTerrainSquare;
		}

		public override void OnEditorGUI()
		{
			terrain.yPosition = EditorGUILayout.FloatField("Y position", terrain.yPosition);
			terrain.heightDisplacement = EditorGUILayout.Toggle("Height displacement", terrain.heightDisplacement);
			if (terrain.heightDisplacement)
			{
				terrain.heightScale = EditorGUILayout.Slider("Height scale", terrain.heightScale, 0.0001f, 1);
			}
		}
	}
}
	