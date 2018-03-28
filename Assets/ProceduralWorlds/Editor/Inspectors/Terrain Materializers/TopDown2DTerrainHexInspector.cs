using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(TopDown2DTerrainHex))]
	public class TopDown2DTerrainHexInspector : TerrainBaseInspector
	{
		TopDown2DTerrainHex		terrain;

		public override void OnEditorEnable()
		{
			terrain = target as TopDown2DTerrainHex;
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
	