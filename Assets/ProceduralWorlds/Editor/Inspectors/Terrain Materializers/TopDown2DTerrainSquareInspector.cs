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
			terrain.heightDisplacement = EditorGUILayout.Toggle("Height displacement", terrain.heightDisplacement);
			if (terrain.heightDisplacement)
			{
				terrain.heightScale = EditorGUILayout.Slider("Height scale", terrain.heightScale, 0.0001f, 1);
			}
		}
	}
}
	