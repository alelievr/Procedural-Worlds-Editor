using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(Naive2DTerrain))]
	public class Naive2DTerrainInspector : BaseTerrainInspector
	{
		Naive2DTerrain	terrain;

		public override void OnEditorEnable()
		{
			terrain = target as Naive2DTerrain;
		}

		public override void OnEditorGUI()
		{
			terrain.yPosition = EditorGUILayout.FloatField("Y position", terrain.yPosition);
			terrain.naive2DSettings.heightDisplacement = EditorGUILayout.Toggle("Height displacement", terrain.naive2DSettings.heightDisplacement);
			if (terrain.naive2DSettings.heightDisplacement)
			{
				terrain.naive2DSettings.heightScale = EditorGUILayout.Slider("Height scale", terrain.naive2DSettings.heightScale, 0.0001f, 1);
			}
		}
	}
}
	