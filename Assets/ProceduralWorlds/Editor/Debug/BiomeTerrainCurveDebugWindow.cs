using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Editor;
using UnityEditorInternal;

namespace ProceduralWorlds.Editor.DebugWindows
{
	using Debug = UnityEngine.Debug;

	public class BiomeTerrainCurveDebugWindow : ProceduralWorldsEditorWindow
	{
		float			blendPercent = 0.05f;

		float			inputWetness;
		float			inputTemperature;

		ReorderableList	reorderableBiomeList;

		[SerializeField]
		List< Biome >	biomes = new List< Biome >();

		[System.Serializable]
		public class Biome
		{
			public string			name;
			public AnimationCurve	terrainCurve = AnimationCurve.Linear(0, 0, 1, 1);
			public float			minWetness;
			public float			maxWetness;
			public float			minTemperature;
			public float			maxTemperature;
		}

		[MenuItem("Window/Procedural Worlds/Debug/BiomeTerrainCurve")]
		public static void Open()
		{
			var win = EditorWindow.GetWindow< BiomeTerrainCurveDebugWindow >();

			win.name = "Terrain Curve debug";
			win.Show();
		}

		public override void OnEnable()
		{
			reorderableBiomeList = new ReorderableList(biomes, typeof(Biome));

			reorderableBiomeList.drawElementCallback = DrawBiomeElement;
			reorderableBiomeList.onAddCallback = AddBiome;
			reorderableBiomeList.elementHeight = (EditorGUIUtility.singleLineHeight + 2) * 4;
			reorderableBiomeList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Biomes");
		}

		void DrawBiomeElement(Rect rect, int index, bool active, bool focused)
		{
			float	lineHeight = EditorGUIUtility.singleLineHeight + 2;
			float	third = rect.width / 3;
			Biome	b = biomes[index];
			
			rect.height = EditorGUIUtility.singleLineHeight;
			
			EditorGUIUtility.labelWidth = rect.width / 3;

			//biome name field
			b.name = EditorGUI.TextField(rect, "Name", b.name);
			rect.y += lineHeight;

			//terrain curve field
			b.terrainCurve = EditorGUI.CurveField(rect, "Terrain curve", b.terrainCurve);
			rect.y += lineHeight;

			EditorGUIUtility.labelWidth = rect.width / 6;

			//wetness field
			Rect wetnessRect = rect;
			wetnessRect.width = third;
			EditorGUI.LabelField(wetnessRect, "Wetness");
			wetnessRect.x += third;
			b.minWetness = EditorGUI.FloatField(wetnessRect, "min", b.minWetness);
			wetnessRect.x += third;
			b.maxWetness = EditorGUI.FloatField(wetnessRect, "max", b.maxWetness);
			rect.y += lineHeight;

			//temperature field
			Rect temperatureRect = rect;
			temperatureRect.width = third;
			EditorGUI.LabelField(temperatureRect, "Temperature");
			temperatureRect.x += third;
			b.minTemperature = EditorGUI.FloatField(temperatureRect, "min", b.minTemperature);
			temperatureRect.x += third;
			b.maxTemperature = EditorGUI.FloatField(temperatureRect, "max", b.maxTemperature);
		}

		void AddBiome(ReorderableList list)
		{
			biomes.Add(new Biome());
		}

		public override void OnGUI()
		{
			EditorGUILayout.LabelField("Terrain curve debug window");
			EditorGUILayout.Space();
			blendPercent = EditorGUILayout.Slider("Blend percent", blendPercent, 0, 0.5f);
			inputWetness = EditorGUILayout.FloatField("Wetness", inputWetness);
			inputTemperature = EditorGUILayout.FloatField("Temperature", inputTemperature);

			EditorGUILayout.Space();
			
			reorderableBiomeList.DoLayoutList();

			EditorGUILayout.Space();
		}

		public override void OnDisable()
		{
			
		}
	}
}