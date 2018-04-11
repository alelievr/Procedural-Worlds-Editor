using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProceduralWorlds.Editor.DebugWindows
{
	using Debug = UnityEngine.Debug;

	public class BiomeTerrainCurveDebugWindow : EditorWindow
	{
		float	inputWetness;
		float	inputTemperature;

		struct Biome
		{
			AnimationCurve	terrainCurve;
			float			minWetness;
			float			maxWetness;
			float			minTemperature;
			float			maxTemperature;
		}

		[MenuItem("Window/Procedural Worlds/Debug/BiomeTerrainCurve")]
		public static void Open()
		{
			var win = EditorWindow.GetWindow< BiomeTerrainCurveDebugWindow >();

			win.Show();
		}

		public void OnEnable()
		{
			
		}

		public void OnGUI()
		{
			inputWetness = EditorGUILayout.FloatField("Wetness", inputWetness);
			inputTemperature = EditorGUILayout.FloatField("Temperature", inputTemperature);
		}

		public void OnDisable()
		{
			
		}
	}
}