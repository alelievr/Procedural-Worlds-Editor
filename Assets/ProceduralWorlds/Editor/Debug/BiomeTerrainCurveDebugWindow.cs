using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Editor;
using UnityEditorInternal;
using ProceduralWorlds.Biomator;
using ProceduralWorlds.Biomator.SwitchGraph;
using System.Linq;

namespace ProceduralWorlds.Editor.DebugWindows
{
	using Debug = UnityEngine.Debug;

	public class BiomeTerrainCurveDebugWindow : ProceduralWorldsEditorWindow
	{
		float			blendPercent = 0.05f;

		[SerializeField]
		int				inputMinHeight;
		[SerializeField]
		int				inputMaxHeight;
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
		[SerializeField]
		int				heightStep = 1;

		[SerializeField]
		Vector2			scrollPos;

		float			minGlobalHeight = 0;
		float			maxGlobalHeight = 100;
		float			minGlobalWetness = 0;
		float			maxGlobalWetness = 100;
		float			minGlobalTemperature = -20;
		float			maxGlobalTemperature = 40;

		int				stepIndex;
		int				textureIndex;

		Texture2D			heightTexture;
		List< StepInfo >	stepInfos = new List< StepInfo >();

		ReorderableList		reorderableBiomeList;

		[SerializeField]
		List< Biome >		biomes = new List< Biome >();
		List< HeightTextureInfo > heightTextures = new List< HeightTextureInfo >();

		BiomeSwitchGraph	switchGraph = new BiomeSwitchGraph();

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
			public List< float >	heights = new List< float >();
		}

		class HeightTextureInfo
		{
			public Texture2D		heightTexture;
			public int				height;
			public float			normalizedHeight;
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
				step = EditorGUILayout.Slider("Step", step, 0.01f, 1f);
	
				DrawInputValues();
	
				if (GUILayout.Button("Evaluate"))
				{
					Evaluate();
					FillHeightTextures();
				}
	
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

			if (textureIndex < heightTextures.Count)
			{
				var hi = heightTextures[textureIndex];

				if (hi.heightTexture == null)
					return ;
				
				EditorGUILayout.LabelField("Height: " + hi.height + " (" + hi.normalizedHeight + ")");
				Rect r = EditorGUILayout.GetControlRect(false, 20, GUILayout.ExpandWidth(true));
				EditorGUI.DrawPreviewTexture(r, hi.heightTexture, null, ScaleMode.StretchToFill);
			}
		}

		#endregion

		#region Computing

		void Evaluate()
		{
			if (!switchGraph.isBuilt)
				return ;

			stepInfos.Clear();

			for (float i = inputMinWetness; i < inputMaxWetness; i += step)
				for (float j = inputMinTemperature; j < inputMaxTemperature; j += step)
					EvaluateStep(i, j);
		}
		
		void EvaluateStep(float wetness, float temperature)
		{
			var bsw = new BiomeSwitchValues();
			var blendList = new BiomeBlendList();
			var blendParams = new BiomeSwitchCellParams();
			
			bsw[1] = wetness;
			bsw[2] = temperature;
			
			var bsc = switchGraph.FindBiome(bsw);

			var stepInfo = new StepInfo();
			stepInfo.temperature = temperature;
			stepInfo.wetness = wetness;

			stepInfos.Add(stepInfo);

			if (bsc == null)
			{
				Debug.Log("Biome not found: " + wetness + " | " + temperature);
				return;
			}

			stepInfo.biomeMap = new BiomeMap2D((int)(inputMaxWetness - inputMinWetness * (1f / step)), 1);
			int x = (int)((wetness + inputMinWetness) * (1 / step));
			int y = 0;

			stepInfo.biomeMap.SetPrimaryBiomeId(x, y, bsc.id);

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
						float blend = link.ComputeBlend(blendList, switchGraph.paramRanges, bsw, blendPercent);
						
						if (blend > 0.001f)
							stepInfo.biomeMap.AddBiome(x, y, link.id, blend);
					}
				}
			
			stepInfo.biomeMap.NormalizeBlendValues(x, y);

			var point = stepInfo.biomeMap.GetBiomeBlendInfo(x, y);

			//compute terrain height:
			stepInfo.heights.Clear();
			for (int h = inputMinHeight; h < inputMaxHeight; h += heightStep)
			{
				float height = 0;
				float normalizedH = Mathf.InverseLerp(inputMinHeight, inputMaxHeight, h);

				for (int i = 0; i < point.biomeBlends.Length; i++)
				{
					var biome = biomes.Find(b => b.id == point.biomeIds[i]);
					var blend = point.biomeBlends[i];
					height += blend * biome.terrainCurve.Evaluate(normalizedH);
				}
				
				stepInfo.heights.Add(height);
			}
		}

		void FillHeightTextures()
		{
			heightTextures.Clear();

			for (int h = inputMinHeight; h < inputMaxHeight; h += heightStep)
			{
				var hi = new HeightTextureInfo();

				hi.height = h;
				hi.normalizedHeight = Mathf.InverseLerp(inputMinHeight, inputMaxHeight, h);
				hi.heightTexture = new Texture2D(stepInfos.Count, 1);
				hi.heightTexture.filterMode = FilterMode.Point;

				for (int x = 0; x < stepInfos.Count; x++)
				{
					if (h >= stepInfos[x].heights.Count)
						continue ;
					
					var height = stepInfos[x].heights[h];
					hi.heightTexture.SetPixel(x, 0, new Color(height, height, height, 1));
				}
				hi.heightTexture.Apply();

				heightTextures.Add(hi);
			}
		}

		#endregion

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
				b.id = bsc.id;
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