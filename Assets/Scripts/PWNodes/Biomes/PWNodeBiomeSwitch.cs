using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public enum BiomeSwitchMode
	{
		Water,
		Height,
		Wetness,
		Temperature,
/*		Wind,
		Lighting,
		Air,
		//TODO: one Soil swicth
		SoilPH,
		SoilDrainage,
		SoilNutriment,
		SoilMineral,
		SoilClay,
		SoilSilt,
		SoilSand,
		SoilGravel,
		Custom1,
		Custom2,
		Custom3,
		Custom4,
		Custom5,
		Custom6,
		Custom7,
		Custom8,
		Custom9,*/
	}

	[System.SerializableAttribute]
	public class BiomeFieldSwitchData
	{
		public float				min;
		public float				max;
		public string				name;
		public float				absoluteMax; //max value in the map
		public float				absoluteMin; //min value in the map
		public SerializableColor	color;

		public BiomeFieldSwitchData(Sampler samp)
		{
			UpdateSampler(samp);
			name = "swampland";
			min = 70;
			max = 90;
			color = (SerializableColor)new Color(0.196f, 0.804f, 0.196f, 1);
		}

		public void UpdateSampler(Sampler samp)
		{
			if (samp != null)
			{
				absoluteMax = samp.max;
				absoluteMin = samp.min;
			}
		}

		public BiomeFieldSwitchData() : this(null) {}
	}

	public class PWNodeBiomeSwitch : PWNode {

		[PWInput]
		public BiomeData	inputBiome;

		[PWOutput]
		[PWMultiple(typeof(BiomeData))]
		[PWOffset(0, 53, 16)]
		public PWValues			outputBiomes = new PWValues();

		public BiomeSwitchMode			switchMode;
		public List< BiomeFieldSwitchData >	switchDatas = new List< BiomeFieldSwitchData >();

		ReorderableList			switchList;
		string[]				biomeSwitchModes;
		[SerializeField]
		int						selectedBiomeSwitchMode;
		[SerializeField]
		bool					error;
		string					errorString;
		Sampler					currentSampler;
		Texture2D				biomeRepartitionPreview;
		bool					updatePreview;
		float					localCoveragePercent;
		
		const int				previewTextureWidth = 200;
		const int				previewTextureHeight = 40;

		const string			delayedUpdateKey = "BiomeSwitchListUpdate";

		public override void OnNodeCreate()
		{
			name = "Biome switch";
			biomeSwitchModes = Enum.GetNames(typeof(BiomeSwitchMode));
			switchList = new ReorderableList(switchDatas, typeof(BiomeFieldSwitchData), true, true, true, true);

			switchList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 4; //padding

			delayedChanges.BindCallback(delayedUpdateKey, (elem) => { notifyBiomeDataChanged = true; });

            switchList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
				BiomeFieldSwitchData elem = switchDatas[index];
				
                rect.y += 2;
				int		floatFieldSize = 70;
				int		colorFieldSize = 20;
				int		nameFieldSize = (int)rect.width - colorFieldSize - 2;
				float	lineHeight = EditorGUIUtility.singleLineHeight;
				Rect	nameRect = new Rect(rect.x, rect.y, nameFieldSize, EditorGUIUtility.singleLineHeight);
				Rect	colorFieldRect = new Rect(rect.x + nameFieldSize + 4, rect.y - 2, colorFieldSize, colorFieldSize);
				Rect	minRect = new Rect(rect.x, rect.y + lineHeight + 2, floatFieldSize, EditorGUIUtility.singleLineHeight);
            	Rect	maxRect = new Rect(rect.x + floatFieldSize, rect.y + lineHeight + 2, floatFieldSize, EditorGUIUtility.singleLineHeight);
				EditorGUI.BeginChangeCheck();
				{
					EditorGUIUtility.labelWidth = 25;
					EditorGUI.BeginChangeCheck();
					{
						float oldMin = elem.min;
						float oldMax = elem.max;
						
						PWGUI.ColorPicker(colorFieldRect, ref elem.color, false, true);
						elem.name = EditorGUI.TextField(nameRect, elem.name);
						elem.min = EditorGUI.FloatField(minRect, "min", elem.min);
						elem.max = EditorGUI.FloatField(maxRect, "max", elem.max);

						//affect up/down cell value
						if (elem.min != oldMin && index > 0)
							switchDatas[index - 1].max = elem.min;
						if (elem.max != oldMax && index + 1 < switchDatas.Count)
							switchDatas[index + 1].min = elem.max;
					}
					if (EditorGUI.EndChangeCheck())
						delayedChanges.UpdateValue(delayedUpdateKey, elem);
					EditorGUIUtility.labelWidth = 0;
				}
				if (EditorGUI.EndChangeCheck())
					updatePreview = true;

				switchDatas[index] = elem;
            };

			switchList.drawHeaderCallback = (rect) => {
				EditorGUI.LabelField(rect, "switches");
			};

			switchList.onReorderCallback += (ReorderableList l) => {
				notifyBiomeDataChanged = true;
			};

			switchList.onAddCallback += (ReorderableList l) => {
				switchDatas.Add(new BiomeFieldSwitchData(currentSampler));
				notifyBiomeDataChanged = true;
				UpdateSwitchMode();
			};

			switchList.onRemoveCallback += (ReorderableList l) => {
				if (switchDatas.Count > 1)
				{
					switchDatas.RemoveAt(l.index);
					notifyBiomeDataChanged = true;
					UpdateSwitchMode();
				}
			};

			if (switchDatas.Count == 0)
				switchDatas.Add(new BiomeFieldSwitchData(currentSampler));
			
			UpdateSwitchMode();
		}

		void UpdateSwitchMode()
		{
			if (switchMode == BiomeSwitchMode.Water)
				UpdateMultiProp("outputBiomes", 2, "terrestrial", "aquatic");
			else
				UpdateMultiProp("outputBiomes", switchDatas.Count, null);
		}

		Dictionary< BiomeSwitchMode, string > switchModeToName = new Dictionary< BiomeSwitchMode, string >()
		{
			{BiomeSwitchMode.Water, "waterHeight"},
			{BiomeSwitchMode.Wetness, "wetness"},
			{BiomeSwitchMode.Temperature, "temperature"},
			// {BiomeSwitchMode.Wind, "wind"},
			// {BiomeSwitchMode.Lighting, "lighting"},
			// {BiomeSwitchMode.Air, "air"},
			{BiomeSwitchMode.Height, "terrain"}
			//soil settings apart.
		};

		void CheckForBiomeSwitchErrors()
		{
			error = false;
			if (switchMode.ToString().Contains("Custom"))
			{
				//TODO: 3d samplers management
				int index = (switchMode.ToString().Last() - '0');
				currentSampler = inputBiome.datas[index];
				foreach (var sd in switchDatas)
					sd.UpdateSampler(currentSampler);
				if (inputBiome.datas[index] == null)
				{
					errorString = "can't switch on custom value\nat index " + index + ",\ndata not provided";
					error = true;
				}
			}
			else if (switchModeToName.ContainsKey(switchMode))
			{
				var field = inputBiome.GetType().GetField(switchModeToName[switchMode], BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField);
				var field3D = inputBiome.GetType().GetField(switchModeToName[switchMode] + "3D", BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField);
				object val = null, val3D = null;

				if (field != null)
					val = field.GetValue(inputBiome);
				if (field3D != null)
					val3D = field3D.GetValue(inputBiome);

				if (val == null && val3D == null)
				{
					errorString = "can't switch on field " + switchModeToName[switchMode] + ",\ndata not provided !";
					error = true;
				}
				else
				{
					currentSampler = ((val == null) ? val3D : val) as Sampler;
					foreach (var sd in switchDatas)
						sd.UpdateSampler(currentSampler);
					updatePreview = true;
				}
			}
		}
		
		public override void OnNodeGUI()
		{
			for (int i = 0; i < outputBiomes.Count; i++)
				UpdatePropVisibility("outputBiomes", error ? PWVisibility.Invisible : PWVisibility.Visible, i);
				
			if (biomeRepartitionPreview == null)
				biomeRepartitionPreview = new Texture2D(previewTextureWidth, 1);

			if (inputBiome == null)
			{
				error = true;
				EditorGUILayout.LabelField("null biome input !");
				return ;
			}
			EditorGUIUtility.labelWidth = 80;
			EditorGUI.BeginChangeCheck();
			{
				selectedBiomeSwitchMode = EditorGUILayout.Popup("switch field", selectedBiomeSwitchMode, biomeSwitchModes);
				switchMode = (BiomeSwitchMode)Enum.Parse(typeof(BiomeSwitchMode), biomeSwitchModes[selectedBiomeSwitchMode]);
				if (currentSampler != null)
					EditorGUILayout.LabelField("min: " + currentSampler.min + ", max: " + currentSampler.max);
				else
					EditorGUILayout.LabelField("");
			}
			if (EditorGUI.EndChangeCheck() || needUpdate || biomeReloadRequested)
			{
				UpdateSwitchMode();
				CheckForBiomeSwitchErrors();
				updatePreview = true;
			}

			if (error)
			{
				Rect errorRect = EditorGUILayout.GetControlRect(false, GUI.skin.label.lineHeight * 3.5f);
				EditorGUI.LabelField(errorRect, errorString);
				return ;
			}

			if (updatePreview && currentSampler != null)
			{
				float min = currentSampler.min;
				float max = currentSampler.max;
				float range = max - min;

				//clear the current texture:
				for (int x = 0; x < previewTextureWidth; x++)
					biomeRepartitionPreview.SetPixel(x, 0, Color.white);

				localCoveragePercent = 0;
				int		i = 0;

				//add water if there is and if switch mode is height:
				if (!inputBiome.isWaterless && switchMode == BiomeSwitchMode.Height)
				{
					float rMax = (inputBiome.waterLevel / range) * previewTextureWidth;
					for (int x = 0; x < rMax; x++)
						biomeRepartitionPreview.SetPixel(x, 0, Color.blue);
				}

				foreach (var switchData in switchDatas)
				{
					float switchMin = Mathf.Max(switchData.min, min);
					float switchMax = Mathf.Min(switchData.max, max);
					float rMin = ((switchMin - min) / range) * previewTextureWidth;
					float rMax = ((switchMax - min) / range) * previewTextureWidth;
					localCoveragePercent += (rMax - rMin) / previewTextureWidth * 100;

					for (int x = (int)rMin; x < (int)rMax; x++)
						biomeRepartitionPreview.SetPixel(x, 0, switchData.color);
					i++;
				}
				biomeRepartitionPreview.Apply();
				updatePreview = false;
			}
			
			if (switchMode != BiomeSwitchMode.Water)
			{
				switchList.DoLayoutList();

				EditorGUILayout.LabelField("repartition map: (" + localCoveragePercent.ToString("F1") + "%)");
				Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(0));
				previewRect.height = previewTextureHeight;
				GUILayout.Space(previewTextureHeight);
				PWGUI.TexturePreview(previewRect, biomeRepartitionPreview, false);
				PWGUI.SetScaleModeForField(-1, ScaleMode.StretchToFill);
			}
		}

		//no process needed else than assignation, this node only exists for user visual organization.
		//switch values are collected form BiomeSwitchTree to create a biome tree.

		public override void OnNodeProcess()
		{
			for (int i = 0; i < outputBiomes.Count; i++)
				outputBiomes.AssignAt(i, inputBiome, "inputBiome");
		}
	}
}
