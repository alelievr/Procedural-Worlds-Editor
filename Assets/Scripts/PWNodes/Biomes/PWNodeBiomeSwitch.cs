using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

namespace PW
{
	public enum PWBiomeSwitchMode
	{
		Water,
		Height,
		Wetness,
		Temperature,
		Wind,
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
		Custom9,
	}

	[System.SerializableAttribute]
	public class BiomeFieldSwitchData
	{
		public float				min;
		public float				max;
		public string				name;
		public SerializableColor	color;

		public BiomeFieldSwitchData()
		{
			name = "swampland";
			min = 70;
			max = 90;
			color = (SerializableColor)new Color(0.196f, 0.804f, 0.196f);
		}
	}

	public class PWNodeBiomeSwitch : PWNode {

		[PWInput]
		public BiomeData	inputBiome;

		[PWOutput]
		[PWMultiple(typeof(BiomeData))]
		[PWOffset(0, 58, 1)]
		public PWValues			outputBiomes = new PWValues();

		public PWBiomeSwitchMode			switchMode;
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
		
		const int				previewTextureWidth = 200;
		const int				previewTextureHeight = 60;

		public override void OnNodeCreate()
		{
			name = "Biome switch";
			biomeSwitchModes = Enum.GetNames(typeof(PWBiomeSwitchMode));
			switchList = new ReorderableList(switchDatas, typeof(BiomeFieldSwitchData), true, true, true, true);

			switchList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 4; //padding

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
				EditorGUIUtility.labelWidth = 25;
				PWGUI.ColorPicker(colorFieldRect, ref elem.color, false, true);
				elem.name = EditorGUI.TextField(nameRect, elem.name);
				elem.min = EditorGUI.FloatField(minRect, "min", elem.min);
				elem.max = EditorGUI.FloatField(maxRect, "max", elem.max);
				EditorGUIUtility.labelWidth = 0;

				switchDatas[index] = elem;
            };

			switchList.drawHeaderCallback = (rect) => {
				EditorGUI.LabelField(rect, "switches");
			};

			switchList.onAddCallback += (ReorderableList l) => {
				switchDatas.Add(new BiomeFieldSwitchData());
				UpdateSwitchMode();
			};

			switchList.onRemoveCallback += (ReorderableList l) => {
				if (switchDatas.Count > 1)
				{
					switchDatas.RemoveAt(l.index);
					UpdateSwitchMode();
				}
			};

			if (switchDatas.Count == 0)
				switchDatas.Add(new BiomeFieldSwitchData());
			
			UpdateSwitchMode();
		}

		void UpdateSwitchMode()
		{
			if (switchMode == PWBiomeSwitchMode.Water)
				UpdateMultiProp("outputBiomes", 2, "terrestrial", "aquatic");
			else
				UpdateMultiProp("outputBiomes", switchDatas.Count, null);
		}

		Dictionary< PWBiomeSwitchMode, string > switchModeToName = new Dictionary< PWBiomeSwitchMode, string >()
		{
			{PWBiomeSwitchMode.Water, "waterHeight"},
			{PWBiomeSwitchMode.Wetness, "wetness"},
			{PWBiomeSwitchMode.Temperature, "temperature"},
			{PWBiomeSwitchMode.Wind, "wind"},
			{PWBiomeSwitchMode.Lighting, "lighting"},
			{PWBiomeSwitchMode.Air, "air"},
			//soil settings apart.
		};

		void CheckForBiomeSwitchErrors()
		{
			error = false;
			if (switchMode.ToString().Contains("Custom"))
			{
				//TODO: 3d samplers management
				var datas = inputBiome.datas;
				int index = (switchMode.ToString().Last() - '0');
				currentSampler = inputBiome.datas[index];
				if (inputBiome.datas[index] == null)
				{
					errorString = "can's switch on custom value\nat index " + index + ",\ndata not provided";
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
				switchMode = (PWBiomeSwitchMode)Enum.Parse(typeof(PWBiomeSwitchMode), biomeSwitchModes[selectedBiomeSwitchMode]);
				if (currentSampler != null)
					EditorGUILayout.LabelField("min: " + currentSampler.min + ", max: " + currentSampler.max);
				else
					EditorGUILayout.LabelField("");
			}
			if (EditorGUI.EndChangeCheck() || needUpdate)
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

				int		i = 0;
				foreach (var switchData in switchDatas)
				{
					float rMin = ((switchData.min + min) / range) * previewTextureWidth;
					float rMax = ((switchData.max + min) / range) * previewTextureWidth;

					Color c = UnityEngine.Random.ColorHSV();
					Debug.Log("pixels: " + rMin + "->" + rMax + " = " + c);
					for (int x = (int)rMin; x < (int)rMax; x++)
						biomeRepartitionPreview.SetPixel(x, 0, switchData.color);
					i++;
				}
				biomeRepartitionPreview.Apply();
				updatePreview = false;
			}
			
			if (switchMode != PWBiomeSwitchMode.Water)
			{
				EditorGUI.BeginChangeCheck();
				switchList.DoLayoutList();
				if (EditorGUI.EndChangeCheck())
					updatePreview = true;

				Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(0));
				previewRect.height = previewTextureHeight;
				GUILayout.Space(previewTextureHeight);
				PWGUI.TexturePreview(previewRect, biomeRepartitionPreview, false);
			}
		}

		//no process needed, this node only exists for user visual organization.
		//switch values are collected form BiomeSwitchTree to create a biome tree.

		public override void OnNodeProcess()
		{
			for (int i = 0; i < outputBiomes.Count; i++)
				outputBiomes.AssignAt(i, inputBiome, "inputBiome");
		}
	}
}