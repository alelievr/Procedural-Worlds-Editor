using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Editor;
using UnityEditorInternal;
using ProceduralWorlds.Biomator;
using ProceduralWorlds.Biomator.SwitchGraph;

namespace ProceduralWorlds.Editor.DebugWindows
{
	using Debug = UnityEngine.Debug;

	public class BiomeTerrainCurveDebugWindow : ProceduralWorldsEditorWindow
	{
		float			blendPercent = 0.05f;

		[SerializeField]
		float			inputMinWetness = 0;
		[SerializeField]
		float			inputMaxWetness = 0;
		[SerializeField]
		float			inputMinTemperature = 0;
		[SerializeField]
		float			inputMaxTemperature = 0;
		[SerializeField]
		float			step = 0.1f;

		float			minGlobalHeight = 0;
		float			maxGlobalHeight = 100;
		float			minGlobalWetness = 0;
		float			maxGlobalWetness = 100;
		float			minGlobalTemperature = -20;
		float			maxGlobalTemperature = 40;

		int				stepIndex;

		Texture2D			heightTexture;
		List< StepInfo >	stepinfos = new List< StepInfo >();

		ReorderableList	reorderableBiomeList;

		[SerializeField]
		List< Biome >	biomes = new List< Biome >();

		BiomeSwitchGraph	switchGraph = new BiomeSwitchGraph();

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

		class StepInfo
		{
			public float			height;
			public float			wetness;
			public float			temperature;

			public List< string >	biomeNames = new List< string >();
			public List< float >	blendPercents = new List< float >();
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

			BuildGraph();
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

			DrawGlobalRange();

			EditorGUILayout.Space();
			
			reorderableBiomeList.DoLayoutList();

			if (GUILayout.Button("Build biome switch graph"))
				BuildGraph();
			EditorGUILayout.LabelField((switchGraph.isBuilt) ? "[Graph built]" : "[Graph not built]", (switchGraph.isBuilt) ? Styles.greenLabel : Styles.redLabel);

			EditorGUILayout.Space();

			blendPercent = EditorGUILayout.Slider("Blend percent", blendPercent, 0, 0.5f);
			step = EditorGUILayout.Slider("Step", step, 0.01f, 1f);
			
			EditorGUIUtility.labelWidth = 50;
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Wetness");
				inputMinWetness = EditorGUILayout.FloatField("Min", inputMinWetness);
				inputMaxWetness = EditorGUILayout.FloatField("Max", inputMaxWetness);
			}
			
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Temperature");
				inputMinTemperature = EditorGUILayout.FloatField("Min", inputMinTemperature);
				inputMaxTemperature = EditorGUILayout.FloatField("Max", inputMaxTemperature);
			}
			EditorGUIUtility.labelWidth = 0;

			if (GUILayout.Button("Evaluate"))
				Evaluate();

			DrawStepInfos();
			
			EditorGUILayout.Space();
		}

		void Evaluate()
		{
			if (!switchGraph.isBuilt)
				return ;

			stepinfos.Clear();

			for (float i = inputMinWetness; i < inputMaxWetness; i += step)
				for (float j = inputMinTemperature; j < inputMaxTemperature; j += step)
					EvaluateStep(i, j);
		}

		void DrawStepInfos()
		{
			stepIndex = EditorGUILayout.IntSlider("Step", stepIndex, 0, stepinfos.Count - 1);

			if (stepIndex < stepinfos.Count)
			{
				var step = stepinfos[stepIndex];

				EditorGUILayout.LabelField("Temperature: " + step.temperature);
				EditorGUILayout.LabelField("Wetness: " + step.wetness);

				for (int i = 0; i < step.biomeNames.Count; i++)
				{
					EditorGUILayout.LabelField("Biome " + step.biomeNames[i] + ": " + (step.blendPercents[i] * 100) + "%");
				}

				if (step.biomeNames.Count == 0)
				{
					EditorGUILayout.LabelField("No biome found for t: " + step.temperature + " / " + step.wetness);
				}
			}
		}

		void DrawGlobalRange()
		{
			EditorGUIUtility.labelWidth = 50;

			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Height range: ");
				minGlobalHeight = EditorGUILayout.FloatField("Min", minGlobalHeight);
				maxGlobalHeight = EditorGUILayout.FloatField("Max", maxGlobalHeight);
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Wetness range: ");
				minGlobalWetness = EditorGUILayout.FloatField("Min", minGlobalWetness);
				maxGlobalWetness = EditorGUILayout.FloatField("Max", maxGlobalWetness);
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Temperature range: ");
				minGlobalTemperature = EditorGUILayout.FloatField("Min", minGlobalTemperature);
				maxGlobalTemperature = EditorGUILayout.FloatField("Max", maxGlobalTemperature);
			}

			EditorGUIUtility.labelWidth = 0;
		}

		void EvaluateStep(float wetness, float temperature)
		{
			var bsw = new BiomeSwitchValues();
			var bsc = switchGraph.FindBiome(bsw);
			var blendList = new BiomeBlendList();
			var blendParams = new BiomeSwitchCellParams();
			
			bsw[1] = wetness;
			bsw[2] = temperature;

			var step = new StepInfo();
			step.temperature = temperature;
			step.wetness = wetness;

			stepinfos.Add(step);

			if (bsc == null)
				return;

			step.biomeNames.Add(bsc.name);
			step.blendPercents.Add(1);

			blendList.blendEnabled = new bool[3];
			blendList.blendEnabled[0] = false;	//height
			blendList.blendEnabled[1] = true;	//wetness
			blendList.blendEnabled[2] = true;	//temperature
			
			//add biome that can be blended with the primary biome,
			if (blendPercent > 0)
				foreach (var link in bsc.links)
				{
					if (link.Overlaps(blendParams))
					{
						float blend = link.ComputeBlend(blendList, switchGraph.paramRanges, bsw, blendPercent, true);
						
						if (blend > 0.001f)
						{
							step.biomeNames.Add(link.name);
							step.blendPercents.Add(blend);
						}
					}
				}
		}

		#region Utils

		IEnumerable< Vector2 > GetBiomeRanges()
		{
			yield return new Vector2(minGlobalHeight, maxGlobalHeight);
			yield return new Vector2(minGlobalWetness, maxGlobalWetness);
			yield return new Vector2(minGlobalTemperature, maxGlobalTemperature);
		}

		IEnumerable< BiomeSwitchCell > GetBiomeCells()
		{
			short biomeId = 0;

			foreach (var b in biomes)
			{
				var bsc = new BiomeSwitchCell();
				var bsp = new BiomeSwitchCellParams();

				//we ignore height param
				bsp.switchParams[1] = new BiomeSwitchCellParam(true, b.minWetness, b.maxWetness);
				bsp.switchParams[2] = new BiomeSwitchCellParam(true, b.minTemperature, b.maxTemperature);

				bsc.id = biomeId++;
				bsc.name = b.name;
				bsc.switchParams = bsp;

				yield return bsc;
			}
		}

		void BuildGraph()
		{
			switchGraph.BuildGraph(GetBiomeRanges(), GetBiomeCells());
		}

		#endregion
	}
}