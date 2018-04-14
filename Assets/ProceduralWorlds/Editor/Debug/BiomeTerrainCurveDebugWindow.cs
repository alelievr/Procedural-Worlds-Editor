using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Editor;
using UnityEditorInternal;
using ProceduralWorlds.Biomator;
using ProceduralWorlds.Biomator.SwitchGraph;
using ProceduralWorlds.Core;
using System.Linq;

namespace ProceduralWorlds.Editor.DebugWindows
{
	using Debug = UnityEngine.Debug;

	public class BiomeTerrainCurveDebugWindow : ProceduralWorldsEditorWindow
	{
		float				blendPercent = 0.05f;
	
		public int			inputMinHeight;
		public int			inputMaxHeight;
		public float		inputMinWetness = 0;
		public float		inputMaxWetness = 0;
		public float		inputMinTemperature = 0;
		public float		inputMaxTemperature = 0;
		public float		wetnessStep = 0.1f;
		public float		temperatureStep = 0.1f;
		public int			heightStep = 1;
	
		public Vector2		scrollPos;
	
		public float		minGlobalHeight = 0;
		public float		maxGlobalHeight = 100;
		public float		minGlobalWetness = 0;
		public float		maxGlobalWetness = 100;
		public float		minGlobalTemperature = -20;
		public float		maxGlobalTemperature = 40;
	
		int					stepIndex;
		int					textureIndex;

		List< StepInfo >	stepInfos = new List< StepInfo >();

		ReorderableList		reorderableBiomeList;

		[SerializeField]
		List< Biome >		biomes = new List< Biome >();

		BiomeSwitchGraph	switchGraph = new BiomeSwitchGraph();

		Sampler2D			heightSampler;
		Sampler2D			wetnessSampler;
		Sampler2D			temperatureSampler;

		[System.Serializable]
		public class Biome
		{
			public short			id;
			public string			name;
			public AnimationCurve	terrainCurve = AnimationCurve.Linear(0, 0, 1, 1);
			public float			minWetness;
			public float			maxWetness;
			public float			minTemperature;
			public float			maxTemperature;
		}

		class StepInfo
		{
			public float			wetness;
			public float			temperature;

			public BiomeMap2D		biomeMap;
			public Sampler2D		heightSampler;
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

		#region Editor draw

		public override void OnGUI()
		{
			using (var scrollScope = new EditorGUILayout.ScrollViewScope(scrollPos))
			{
				scrollPos = scrollScope.scrollPosition;

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
				wetnessStep = EditorGUILayout.Slider("Wetness step", wetnessStep, 0.01f, 1f);
				temperatureStep = EditorGUILayout.Slider("Temperature step", temperatureStep, 0.01f, 1f);
	
				DrawInputValues();
	
				if (GUILayout.Button("Evaluate"))
					Evaluate();
	
				DrawStepInfos();
	
				DrawHeightTextures();
				
				EditorGUILayout.Space();
			}
		}

		void DrawInputValues()
		{
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
			
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Height");
				inputMinHeight = EditorGUILayout.IntField("Min", inputMinHeight);
				inputMaxHeight = EditorGUILayout.IntField("Max", inputMaxHeight);
			}

			heightStep = EditorGUILayout.IntSlider("Height step", heightStep, 1, inputMaxHeight);
			EditorGUIUtility.labelWidth = 0;
		}

		void DrawStepInfos()
		{
			stepIndex = EditorGUILayout.IntSlider("Step", stepIndex, 0, stepInfos.Count - 1);

			if (stepIndex < stepInfos.Count)
			{
				var step = stepInfos[stepIndex];
				var point = step.biomeMap.GetBiomeBlendInfo(0, 0);

				EditorGUILayout.LabelField("Temperature: " + step.temperature);
				EditorGUILayout.LabelField("Wetness: " + step.wetness);

				for (int i = 0; i < point.biomeIds.Length; i++)
				{
					EditorGUILayout.LabelField("Biome " + point.biomeIds[i] + ": " + (point.biomeBlends[i] * 100) + "%");
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

		void DrawHeightTextures()
		{
			textureIndex = EditorGUILayout.IntSlider(textureIndex, inputMinHeight, inputMaxHeight - 1);

			//TODO: draw biome map in debug mode
		}

		#endregion

		#region Computing

		void Evaluate()
		{
			if (!switchGraph.isBuilt)
				return ;

			stepInfos.Clear();

			for (float i = inputMinHeight; i < inputMaxHeight; i += heightStep)
					EvaluateStep(i);
		}
		
		void EvaluateStep(float h)
		{
			var blendList = new BiomeBlendList();
			var stepInfo = new StepInfo();

			int size = (int)Mathf.Max((inputMaxWetness - inputMinWetness) * (1 / wetnessStep), (inputMaxTemperature - inputMinTemperature) * (1 / temperatureStep));

			//initialize samplers
			stepInfo.heightSampler = new Sampler2D(size, 1);
			wetnessSampler = new Sampler2D(size, 1);
			temperatureSampler = new Sampler2D(size, 1);
			stepInfo.biomeMap = new BiomeMap2D(size, 1);

			//setup blend list
			blendList.blendEnabled = new bool[3];
			blendList.blendEnabled[0] = false;	//height
			blendList.blendEnabled[1] = true;	//wetness
			blendList.blendEnabled[2] = true;	//temperature

			for (float w = inputMinWetness, x = 0; w < inputMaxWetness; w += wetnessStep, x++)
				for (float t = inputMinTemperature, y = 0; t < inputMaxTemperature; t += temperatureStep, y++)
				{
					wetnessSampler[(int)x, (int)y] = w;
					temperatureSampler[(int)x, (int)y] = t;
					
					switchGraph.FillBiomeMap2D(stepInfo.biomeMap, blendList, blendPercent);

					stepInfo.heightSampler.Foreach((hx, hy) => {
						return 0;
					});
				}
			
			stepInfos.Add(stepInfo);
		}

		#endregion

		#region Utils

		IEnumerable< Sampler > GetBiomeSamplers()
		{
			yield return heightSampler;
			yield return wetnessSampler;
			yield return temperatureSampler;
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
				b.id = bsc.id;
				bsc.name = b.name;
				bsc.switchParams = bsp;

				yield return bsc;
			}
		}

		void BuildGraph()
		{
			switchGraph.BuildGraph(GetBiomeSamplers(), GetBiomeCells());
		}

		#endregion
	}
}