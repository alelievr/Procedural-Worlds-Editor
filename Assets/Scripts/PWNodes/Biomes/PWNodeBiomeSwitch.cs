using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

namespace PW
{
	public enum BiomeSwitchMode
	{
		Water,
		Height,
		Wetness,
		Temperature,
		Wind,
		Lighting,
		SoilPH,
		SoilDrainage,
		SoilNutriment,
		SoilMineral,
		SoilClay,
		SoilSilt,
		SoilSand,
		SoilGravel,
		Air,
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
	public class SwitchData
	{
		public float		min;
		public float		max;
		public string		name;

		public SwitchData()
		{
			name = "swampland";
			min = 70;
			max = 90;
		}
	}

	public class PWNodeBiomeSwitch : PWNode {

		[PWInput]
		public BiomeBaseData	inputBiome;

		[PWOutput]
		[PWMultiple(typeof(BiomeBaseData))]
		[PWOffset(20)]
		public PWValues			outputBiomes = new PWValues();

		public BiomeSwitchMode	switchMode;
		public List< SwitchData >	switchDatas = new List< SwitchData >();
		public bool				boolSwitch;

		ReorderableList			switchList;
		string[]				biomeSwitchModes;
		int						selectedBiomeSwitchMode;

		public override void OnNodeCreate()
		{
			name = "Biome switch";
			UpdateBiomeSwitchModes();
			switchList = new ReorderableList(switchDatas, typeof(SwitchData), true, true, true, true);

            switchList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
				SwitchData elem = switchDatas[index];
				
                rect.y += 2;
				int		floatFieldSize = 40;
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
				switchDatas.Add(new SwitchData());
				UpdateSwitchMode();
			};

			switchList.onRemoveCallback += (ReorderableList l) => {
				switchDatas.RemoveAt(l.index);
				UpdateSwitchMode();
			};
		}

		void UpdateBiomeSwitchModes()
		{
			if (inputBiome != null && inputBiome.isWaterless)
				biomeSwitchModes = Enum.GetNames(typeof(BiomeSwitchMode)).Skip(1).ToArray();
			else
				biomeSwitchModes = Enum.GetNames(typeof(BiomeSwitchMode));
		}

		void UpdateSwitchMode()
		{
			if (switchMode == BiomeSwitchMode.Water)
				UpdateMultiProp("outputBiomes", 2, "terrestrial", "aquatic");
			else
				UpdateMultiProp("outputBiomes", switchDatas.Count, null);
		}

		public override void OnNodeAnchorLink(string propName, int index)
		{
			if (propName == "inputBiome")
				UpdateBiomeSwitchModes();
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 80;
			EditorGUI.BeginChangeCheck();
			{
				selectedBiomeSwitchMode = EditorGUILayout.Popup("switch field", selectedBiomeSwitchMode, biomeSwitchModes);
				switchMode = (BiomeSwitchMode)Enum.Parse(typeof(BiomeSwitchMode), biomeSwitchModes[selectedBiomeSwitchMode]);
			}
			if (EditorGUI.EndChangeCheck() || needUpdate)
				UpdateSwitchMode();
			
			//TODO: display a reorderable list of min-max floats
			//TODO: preview for min-max data coverage

			if (switchMode != BiomeSwitchMode.Water)
				switchList.DoLayoutList();
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