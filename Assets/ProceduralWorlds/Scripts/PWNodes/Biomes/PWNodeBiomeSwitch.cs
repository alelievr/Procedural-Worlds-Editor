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
		[PWOffset(53, 16)]
		public PWArray< BiomeData >	outputBiomes = new PWArray< BiomeData >();

		[SerializeField]
		public PWBiomeSwitchList	switchList = new PWBiomeSwitchList();

		public PWBiomeSwitchMode	switchMode;
		string[]					biomeSwitchModes;

		[SerializeField]
		int						selectedPWBiomeSwitchMode;
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

			biomeSwitchModes = Enum.GetNames(typeof(PWBiomeSwitchMode));

			delayedChanges.BindCallback(delayedUpdateKey, (unused) => { UpdateSwitchMode(); NotifyReload(typeof(PWNodeBiomeBlender)); });

			Action UpdateBiomeBlender = () => { delayedChanges.UpdateValue(delayedUpdateKey, null); };

			switchList.OnBiomeDataAdded = (unused) => { UpdateBiomeBlender(); };
			switchList.OnBiomeDataModified = (unused) => { UpdateBiomeBlender(); };
			switchList.OnBiomeDataRemoved = UpdateBiomeBlender;
			switchList.OnBiomeDataReordered = UpdateBiomeBlender;
			
			UpdateSwitchMode();
		}

		void UpdateSwitchMode()
		{
			if (switchMode == PWBiomeSwitchMode.Water)
				SetMultiAnchor("outputBiomes", 2, "terrestrial", "aquatic");
			else
				SetMultiAnchor("outputBiomes", switchList.Count, null);
		}

		Dictionary< PWBiomeSwitchMode, string > switchModeToName = new Dictionary< PWBiomeSwitchMode, string >()
		{
			{PWBiomeSwitchMode.Water, "waterHeight"},
			{PWBiomeSwitchMode.Wetness, "wetness"},
			{PWBiomeSwitchMode.Temperature, "temperature"},
			// {PWBiomeSwitchMode.Wind, "wind"},
			// {PWBiomeSwitchMode.Lighting, "lighting"},
			// {PWBiomeSwitchMode.Air, "air"},
			{PWBiomeSwitchMode.Height, "terrain"}
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

			//FIXME do i still need this ?
			for (int i = 0; i < outputBiomes.Count; i++)
				SetAnchorVisibility("outputBiomes", error ? PWVisibility.Invisible : PWVisibility.Visible, i);
			
			//display popup field to choose the switch source
			EditorGUI.BeginChangeCheck();
			{
				EditorGUIUtility.labelWidth = 80;
				selectedPWBiomeSwitchMode = EditorGUILayout.Popup("switch field", selectedPWBiomeSwitchMode, biomeSwitchModes);
				switchMode = (PWBiomeSwitchMode)Enum.Parse(typeof(PWBiomeSwitchMode), biomeSwitchModes[selectedPWBiomeSwitchMode]);
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

			if (switchMode != PWBiomeSwitchMode.Water)
				switchList.OnGUI();

			firstPass = false;
		}

		//no process needed else than assignation, this node only exists for user visual organization.
		//switch values are collected form BiomeSwitchTree to create a biome tree.

		public override void OnNodeProcess()
		{
			#if UNITY_EDITOR
				switchList.UpdateBiomeRepartitionPreview();
			#endif

			Debug.Log("Processing biome switch, count: " + outputBiomes.Count);
			for (int i = 0; i < outputBiomes.Count; i++)
				outputBiomes.AssignAt(i, inputBiome, "inputBiome");
		}

		public override void OnNodeProcessOnce()
		{
			for (int i = 0; i < outputBiomes.Count; i++)
				outputBiomes.AssignAt(i, inputBiome, "inputBiome");
		}
	}
}
