using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(TopDownHex2DTerrain))]
	public class TopDownHex2DTerrainInspector : TerrainBaseInspector
	{
		TopDownHex2DTerrain		terrain;

		public override void OnEditorEnable()
		{
			terrain = target as TopDownHex2DTerrain;
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
	