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
		public float		min;
		public float		max;
		public string		name;

		public BiomeFieldSwitchData()
		{
			name = "swampland";
			min = 70;
			max = 90;
		}
	}

	public class PWNodeBiomeSwitch : PWNode {

		[PWInput]
		public BiomeData	inputBiome;

		[PWOutput]
		[PWMultiple(typeof(BiomeData))]
		[PWOffset(0, 41, 1)]
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

		public override void OnNodeCreate()
		{
			name = "Biome switch";
			biomeSwitchModes = Enum.GetNames(typeof(PWBiomeSwitchMode));
			switchList = new ReorderableList(switchDatas, typeof(BiomeFieldSwitchData), true, true, true, true);

            switchList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
				BiomeFieldSwitchData elem = switchDatas[index];
				
                rect.y += 2;
				int		floatFieldSize = 30;
				int		labelFieldSize = 80;
				Rect labelRect = new Rect(rect.x, rect.y, labelFieldSize, EditorGUIUtility.singleLineHeight);
				Rect minRect = new Rect(rect.x + labelFieldSize, rect.y, floatFieldSize, EditorGUIUtility.singleLineHeight);
            	Rect maxRect = new Rect(rect.x + labelFieldSize + floatFieldSize, rect.y, floatFieldSize, EditorGUIUtility.singleLineHeight);
				elem.name = EditorGUI.TextField(labelRect, elem.name);
				elem.min = EditorGUI.FloatField(minRect, elem.min);
				elem.max = EditorGUI.FloatField(maxRect, elem.max);

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
			//TODO: work with new BiomeData storage class
			error = false;
			if (switchMode.ToString().Contains("Custom"))
			{
				var fieldInfo = inputBiome.GetType().GetField("datas", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.Public);
				var datas = fieldInfo.GetValue(inputBiome) as object[];
				int index = (int)(switchMode.ToString().Last());
				if (index > datas.Length)
				{
					errorString = "can's switch on custom value\nat index " + index + ",\n data not provided";
					error = true;
				}
			}
			else if (switchModeToName.ContainsKey(switchMode)
				&& inputBiome.GetType().GetField(switchModeToName[switchMode], BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField).GetValue(inputBiome) == null)
			{
				errorString = "can't switch on field " + switchModeToName[switchMode] + ",\ndata not provided !";
				error = true;
			}
		}

		public override void OnNodeGUI()
		{
			for (int i = 0; i < outputBiomes.Count; i++)
				UpdatePropVisibility("outputBiomes", error ? PWVisibility.Invisible : PWVisibility.Visible, i);

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
			}
			if (EditorGUI.EndChangeCheck() || needUpdate)
			{
				UpdateSwitchMode();
				CheckForBiomeSwitchErrors();
			}

			if (error)
			{
				Rect errorRect = EditorGUILayout.GetControlRect(false, GUI.skin.label.lineHeight * 3);
				EditorGUI.LabelField(errorRect, errorString);
				return ;
			}
			
			if (switchMode != PWBiomeSwitchMode.Water)
				switchList.DoLayoutList();

			//TODO: preview for min-max data coverage
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