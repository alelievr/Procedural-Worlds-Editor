using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using System.Linq;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Node
{
	public class NodeBiomeSwitch : BaseNode
	{
		[Input]
		public BiomeData			inputBiome;

		[Output]
		[Offset(72, 18)] //hardcoded padding and margin for anchors
		public PWArray< BiomeData >	outputBiomes = new PWArray< BiomeData >();

		[SerializeField]
		public BiomeSwitchList		switchList = new BiomeSwitchList();

		public string				samplerName;
		public string[]				samplerNames;

		public bool				alreadyModified = false;
		public int				selectedBiomeSamplerName;
		public bool				error;
		public string			errorString;
		
		public Sampler			currentSampler;

		public float			relativeMin = float.MinValue;
		public float			relativeMax = float.MaxValue;

		public override void OnNodeCreation()
		{
			name = "Biome switch";
		}

		public override void OnNodeEnable()
		{
			samplerNames = BiomeSamplerName.GetNames().ToArray();
			samplerName = samplerNames[selectedBiomeSamplerName];

			UpdateSwitchMode();
		}

		public void UpdateSwitchMode()
		{
			UpdateRelativeBounds();

			if (switchList.Count > 2 || alreadyModified)
				return ;
			
			while (switchList.Count < 2)
				switchList.switchDatas.Add(new BiomeSwitchData(currentSampler, samplerName));

			var d1 = switchList.switchDatas[0];
			var d2 = switchList.switchDatas[1];
			
			d1.min = relativeMin;
			d1.max = relativeMax / 2;
			d2.min = relativeMax / 2;
			d2.max = relativeMax;
			d1.color = (SerializableColor)UnityEngine.Random.ColorHSV();
			d2.color = (SerializableColor)UnityEngine.Random.ColorHSV();

			switch (samplerName)
			{
				case BiomeSamplerName.terrainHeight:
					d1.name = "Low lands";
					d2.name = "High lands";
					break ;
				case BiomeSamplerName.waterHeight:
					if (relativeMin < 0 && relativeMax > 0)
					{
						d1.max = 0;
						d2.min = 0;
					}
					d1.name = "Terrestrial";
					d2.name = "Aquatic";
					break ;
				case BiomeSamplerName.temperature:
					d1.name = "Cold";
					d2.name = "Hot";
					break ;
				case BiomeSamplerName.wetness:
					d1.name = "Dry";
					d2.name = "Wet";
					break ;
			}

			SetMultiAnchor("outputBiomes", switchList.Count, null);
		}

		void UpdateRelativeBounds()
		{
			var inputNodes = GetInputNodes();

			if (inputNodes.Count() == 0)
				return ;
			
			BaseNode prevNode = inputNodes.First();
			BaseNode currentNode = this;

			while (prevNode.GetType() == typeof(NodeBiomeSwitch))
			{
				NodeBiomeSwitch prevSwitch = (NodeBiomeSwitch)prevNode;

				if (prevSwitch.samplerName != samplerName)
				{
					inputNodes = prevNode.GetInputNodes();

					if (inputNodes.Count() == 0)
						return ;
					
					currentNode = prevNode;
					prevNode = inputNodes.First();
					continue ;
				}

				var prevNodeOutputAnchors = prevSwitch.outputAnchors.ToList();
	
				for (int i = 0; i < prevNodeOutputAnchors.Count; i++)
				{
					var anchor = prevNodeOutputAnchors[i];
	
					if (anchor.links.Any(l => l.toNode == currentNode))
					{
						relativeMin = prevSwitch.switchList.switchDatas[i].min;
						relativeMax = prevSwitch.switchList.switchDatas[i].max;
					}
				}

				break ;
			}

			if (prevNode.GetType() != typeof(NodeBiomeSwitch))
			{
				//update currentSampler value:
				if (inputBiome != null)
					currentSampler = inputBiome.GetSampler(samplerName);
				
				if (currentSampler == null)
					return ;
				
				relativeMin = currentSampler.min;
				relativeMax = currentSampler.max;
				return ;
			}
		}

		public void CheckForBiomeSwitchErrors()
		{
			error = false;

			if (inputBiome == null)
				return ;

			var field = inputBiome.GetSampler(samplerName);
			var field3D = inputBiome.GetSampler(samplerName);

			if (field == null || field3D == null)
			{
				errorString = "can't switch on field " + samplerName + ",\ndata not provided !";
				error = true;
			}
			else
				currentSampler = ((field == null) ? field3D : field);
			
			currentSampler = inputBiome.GetSampler(samplerName);

			//Update switchList values:
			switchList.UpdateSampler(samplerName, currentSampler);
		}
		
		public void UpdateSwitchList()
		{
			if (inputBiome != null)
				currentSampler = inputBiome.GetSampler(samplerName);

			UpdateRelativeBounds();
			
			switchList.UpdateMinMax(relativeMin, relativeMax);
			switchList.UpdateSampler(samplerName, currentSampler);
		}

		void AdjustOutputBiomeArraySize()
		{
			int		outputArraySize = switchList.Count;

			//we adjust the size of the outputBiomes array to the size of the switchList
			while (outputBiomes.Count < outputArraySize)
				outputBiomes.Add(inputBiome, "outputBiome");
			while (outputBiomes.Count > outputArraySize)
				outputBiomes.RemoveAt(outputBiomes.Count - 1);
		}

		//no process needed else than assignation, this node only exists for user visual organization.
		public override void OnNodeProcess()
		{
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
