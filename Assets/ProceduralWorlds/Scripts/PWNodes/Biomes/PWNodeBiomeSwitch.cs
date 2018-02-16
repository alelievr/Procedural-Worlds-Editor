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
		public BiomeSwitchList		switchList = new BiomeSwitchList();

		public string				samplerName;
		string[]					samplerNames;

		[SerializeField]
		int						selectedBiomeSamplerName;
		[SerializeField]
		bool					error;
		string					errorString;
		Sampler					currentSampler;
		[System.NonSerialized]
		bool					firstPass = true;

		float					relativeMin;
		float					relativeMax;

		const string			delayedUpdateKey = "BiomeSwitchListUpdate";

		public override void OnNodeCreation()
		{
			name = "Biome switch";
		}

		public override void OnNodeEnable()
		{
			switchList.OnEnable();

			samplerNames = BiomeSamplerName.GetNames().ToArray();
			samplerName = samplerNames[selectedBiomeSamplerName];

			delayedChanges.BindCallback(delayedUpdateKey, (unused) => { NotifyReload(typeof(PWNodeBiomeBlender)); });

			switchList.OnBiomeDataAdded = (unused) => { UpdateSwitchMode(); delayedChanges.UpdateValue(delayedUpdateKey, null); };
			switchList.OnBiomeDataModified = (unused) => { UpdateSwitchMode(); delayedChanges.UpdateValue(delayedUpdateKey, null); };
			switchList.OnBiomeDataRemoved = () => { UpdateSwitchMode(); delayedChanges.UpdateValue(delayedUpdateKey, null); };
			switchList.OnBiomeDataReordered = () => { UpdateSwitchMode(); delayedChanges.UpdateValue(delayedUpdateKey, null); };
			
			UpdateSwitchMode();
		}

		void UpdateSwitchMode()
		{
			UpdateRelativeBounds();

			//TODO: set default values for switches
			switch (samplerName)
			{
				case BiomeSamplerName.terrainHeight:
					if (switchList.Count < 2)
					{
						while (switchList.Count < 2)
							switchList.switchDatas.Add(new BiomeSwitchData(currentSampler, samplerName));

						var d1 = switchList.switchDatas[0];
						var d2 = switchList.switchDatas[1];
						
						d1.min = relativeMin;
						d1.max = relativeMax / 2;
						d2.min = relativeMax / 2;
						d2.max = relativeMax;
					}
					break ;
				case BiomeSamplerName.waterHeight:
					break ;
				case BiomeSamplerName.temperature:
					break ;
				case BiomeSamplerName.wetness:
					break ;
			}

			SetMultiAnchor("outputBiomes", switchList.Count, null);
		}

		void UpdateRelativeBounds()
		{
			var inputNodes = GetInputNodes();

			if (inputNodes.Count() == 0)
				return ;
			
			var prevNode = inputNodes.First();

			while (prevNode.GetType() == typeof(PWNodeBiomeSwitch))
			{
				PWNodeBiomeSwitch prevSwitch = (PWNodeBiomeSwitch)prevNode;

				if (prevSwitch.samplerName != samplerName)
				{
					prevNode = prevNode.GetInputNodes().First();
					continue ;
				}

				var prevNodeOutputAnchors = prevSwitch.outputAnchors.ToList();
	
				for (int i = 0; i < prevNodeOutputAnchors.Count; i++)
				{
					var anchor = prevNodeOutputAnchors[i];
	
					if (anchor.links.Any(l => l.toNode == this))
					{
						relativeMin = prevSwitch.switchList.switchDatas[i].min;
						relativeMax = prevSwitch.switchList.switchDatas[i].max;
					}
				}

				break ;
			}

			if (prevNode.GetType() != typeof(PWNodeBiomeSwitch))
			{
				if (currentSampler == null)
					return ;
				relativeMin = currentSampler.min;
				relativeMax = currentSampler.max;
				return ;
			}
		}

		void CheckForBiomeSwitchErrors()
		{
			error = false;

			var field = inputBiome.GetSampler(samplerName);
			var field3D = inputBiome.GetSampler(samplerName);

			if (field == null || field3D == null)
			{
				errorString = "can't switch on field " + samplerName + ",\ndata not provided !";
				error = true;
			}
			else
				currentSampler = ((field == null) ? field3D : field);

			//Update switchList values:
			switchList.currentBiomeData = inputBiome;
			switchList.currentSampler = currentSampler;
			
			switchList.UpdateSwitchMode(samplerName);
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
				selectedBiomeSamplerName = EditorGUILayout.Popup("switch parameter", selectedBiomeSamplerName, samplerNames);
				samplerName = samplerNames[selectedBiomeSamplerName];
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

			switchList.UpdateMinMax(relativeMin, relativeMax);

			switchList.OnGUI();

			firstPass = false;
		}

		//no process needed else than assignation, this node only exists for user visual organization.

		void AdjustOutputBiomeArraySize()
		{
			int		outputArraySize = switchList.Count;

			//we adjust the size of the outputBiomes array to the size of the switchList
			while (outputBiomes.Count < outputArraySize)
				outputBiomes.Add(inputBiome, "outputBiome");
			while (outputBiomes.Count > outputArraySize)
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
