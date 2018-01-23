using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeSwitch : PWNode
	{
		[PWInput]
		public BiomeData			inputBiome;

		[PWOutput]
		[PWOffset(72, 18)] //hardcoded padding and margin for anchors
		public PWArray< BiomeData >	outputBiomes = new PWArray< BiomeData >();

		[SerializeField]
		public PWBiomeSwitchList	switchList = new PWBiomeSwitchList();

		public BiomeSwitchMode	switchMode;
		string[]					biomeSwitchModes;

		[SerializeField]
		int						selectedBiomeSwitchMode;
		[SerializeField]
		bool					error;
		string					errorString;
		Sampler					currentSampler;
		[System.NonSerialized]
		bool					firstPass = true;

		const string			delayedUpdateKey = "BiomeSwitchListUpdate";

		public override void OnNodeCreation()
		{
			name = "Biome switch";
		}

		public override void OnNodeEnable()
		{
			switchList.OnEnable();

			biomeSwitchModes = Enum.GetNames(typeof(BiomeSwitchMode));

			delayedChanges.BindCallback(delayedUpdateKey, (unused) => { NotifyReload(typeof(PWNodeBiomeBlender)); });

			switchList.OnBiomeDataAdded = (unused) => { UpdateSwitchMode(); delayedChanges.UpdateValue(delayedUpdateKey, null); };
			switchList.OnBiomeDataModified = (unused) => { UpdateSwitchMode(); delayedChanges.UpdateValue(delayedUpdateKey, null); };
			switchList.OnBiomeDataRemoved = () => { UpdateSwitchMode(); delayedChanges.UpdateValue(delayedUpdateKey, null); };
			switchList.OnBiomeDataReordered = () => { UpdateSwitchMode(); delayedChanges.UpdateValue(delayedUpdateKey, null); };
			
			UpdateSwitchMode();
		}

		void UpdateSwitchMode()
		{
			if (switchMode == BiomeSwitchMode.Water)
				SetMultiAnchor("outputBiomes", 2, "terrestrial", "aquatic");
			else
				SetMultiAnchor("outputBiomes", switchList.Count, null);
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
					currentSampler = ((val == null) ? val3D : val) as Sampler;
			}

			//Update switchList values:
			switchList.currentSwitchMode = switchMode;
			switchList.currentBiomeData = inputBiome;
			switchList.currentSampler = currentSampler;
			
			switchList.UpdateBiomeRepartitionPreview();
		}
		
		public override void OnNodeGUI()
		{
			//return if input biome is null
			if (inputBiome == null)
			{
				error = true;
				EditorGUILayout.LabelField("null biome input !");
				return ;
			}

			if (firstPass)
				CheckForBiomeSwitchErrors();

			//display popup field to choose the switch source
			EditorGUI.BeginChangeCheck();
			{
				EditorGUIUtility.labelWidth = 80;
				selectedBiomeSwitchMode = EditorGUILayout.Popup("switch parameter", selectedBiomeSwitchMode, biomeSwitchModes);
				switchMode = (BiomeSwitchMode)Enum.Parse(typeof(BiomeSwitchMode), biomeSwitchModes[selectedBiomeSwitchMode]);
			}
			if (EditorGUI.EndChangeCheck())
			{
				UpdateSwitchMode();
				CheckForBiomeSwitchErrors();
			}

			EditorGUILayout.LabelField((currentSampler != null) ? "min: " + currentSampler.min + ", max: " + currentSampler.max : "");

			if (error)
			{
				Rect errorRect = EditorGUILayout.GetControlRect(false, GUI.skin.label.lineHeight * 3.5f);
				EditorGUI.LabelField(errorRect, errorString);
				return ;
			}

			if (switchMode != BiomeSwitchMode.Water)
				switchList.OnGUI();

			firstPass = false;
		}

		//no process needed else than assignation, this node only exists for user visual organization.
		//switch values are collected form BiomeSwitchTree to create a biome tree.

		void AdjustOutputBiomeArraySize()
		{
			//we adjust the size of the outputBiomes array to the size of the switchList
			while (outputBiomes.Count < switchList.Count)
				outputBiomes.Add(inputBiome, "outputBiome");
			while (outputBiomes.Count > switchList.Count)
				outputBiomes.RemoveAt(outputBiomes.Count - 1);
		}

		public override void OnNodeProcess()
		{
			#if UNITY_EDITOR
				switchList.UpdateBiomeRepartitionPreview();
			#endif

			AdjustOutputBiomeArraySize();

			for (int i = 0; i < outputBiomes.Count; i++)
				outputBiomes.AssignAt(i, inputBiome, "inputBiome");
		}

		public override void OnNodeProcessOnce()
		{
			AdjustOutputBiomeArraySize();

			for (int i = 0; i < outputBiomes.Count; i++)
				outputBiomes.AssignAt(i, inputBiome, "inputBiome");
		}
	}
}
