using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Biomator;
using PW.Node;
using System;
using System.Linq;
using System.Diagnostics;
using PW.Biomator.SwitchGraph;

using Debug = UnityEngine.Debug;

namespace PW.Biomator
{
	public class BiomeSwitchGraph
	{
		public BiomeSwitchCell				rootCell;
		public BiomeSwitchCell				lastCell;
		public List< BiomeSwitchCell >		cells = new List< BiomeSwitchCell >();

		[System.NonSerialized]
		public bool							isBuilt = false;

		[System.NonSerialized]
		short								biomeIdCount = 0;

		[System.NonSerialized]
		Dictionary< string, PartialBiome >		partialBiomePerName = new Dictionary< string, PartialBiome >();
		[System.NonSerialized]
		Dictionary< short, PartialBiome >		partialBiomePerId = new Dictionary< short, PartialBiome >();
		[System.NonSerialized]
		Dictionary< BiomeSwitchMode, float >	biomeCoverage = new Dictionary< BiomeSwitchMode, float >();

		[System.NonSerialized]
		public Dictionary< BiomeSwitchMode, Vector2 >	paramRanges = new Dictionary< BiomeSwitchMode, Vector2 >();

		public bool BuildGraph(PWNode rootNode)
		{
			ResetGraph();

			Stack< PWNode > biomeNodes = new Stack< PWNode >();

			biomeNodes.Push(rootNode);

			BiomeSwitchCell currentCell = null;

			//fill param range dictionary (used to compute weights)
			FillParamRange(rootNode);

			while (biomeNodes.Count > 0)
			{
				PWNode	currentNode = biomeNodes.Pop();
				Type	nodeType = currentNode.GetType();

				if (nodeType == typeof(PWNodeBiome))
				{
					//get all precedent switch data in the order of the tree (start from rootNode)
					var precedentSwitchDatas = GetPrecedentSwitchDatas(currentNode, rootNode);

					var biomeNode = currentNode as PWNodeBiome;
					var lastSwitch = precedentSwitchDatas.Last();
					currentCell = new BiomeSwitchCell();
					
					//Set cell and partial biome remaining empty params
					currentCell.name = biomeNode.outputBiome.name = lastSwitch.name;
					currentCell.id = biomeNode.outputBiome.id = biomeIdCount++;
					currentCell.color = biomeNode.outputBiome.previewColor = lastSwitch.color;
					currentCell.weight = currentCell.GetWeight(paramRanges);

					//add the partial biome to utility dic accessors:
					partialBiomePerName[currentCell.name] = biomeNode.outputBiome;
					partialBiomePerId[currentCell.id] = biomeNode.outputBiome;

					//load all access condition for this biome switch
					foreach (var sd in precedentSwitchDatas)
					{
						var param = currentCell.switchParams[sd.mode];

						param.enabled = true;
						param.min = sd.min;
						param.max = sd.max;
						biomeCoverage[sd.mode] += (sd.max - sd.min) / (sd.absoluteMax - sd.absoluteMin);
					}
				}
				else if (nodeType == typeof(PWNodeBiomeBlender))
				{
					if (currentCell == null)
						throw new Exception("[PWBiomeSwitchGraph] idk what happened but this is really bad");
					
					//if the flow reaches the biomeblender everything is OK and add the current cell to the graph
					cells.Add(currentCell);
					continue ;
				}

				foreach (var outputNode in currentNode.GetOutputNodes())
					biomeNodes.Push(outputNode);
			}

			//Generate links between all linkable nodes
			BuildLinks();
			
			rootCell = cells.First();

			if (!CheckValid())
				return false;

			isBuilt = true;

			return true;
		}

		void ResetGraph()
		{
			isBuilt = false;
			biomeIdCount = 0;

			cells.Clear();
			partialBiomePerId.Clear();
			partialBiomePerName.Clear();
			paramRanges.Clear();

			for (int i = 0; i < (int)BiomeSwitchMode.Count; i++)
				biomeCoverage[(BiomeSwitchMode)i] = 0;
		}

		void FillParamRange(PWNode rootNode)
		{
			Stack< PWNode >	currentNodes = new Stack< PWNode >();
			PWNode			currentNode = rootNode;

			while (currentNodes.Count > 0)
			{
				currentNode = currentNodes.Pop();
				
				foreach (var outputNode in currentNode.GetOutputNodes())
					currentNodes.Push(outputNode);

				if (!(currentNode is PWNodeBiomeSwitch))
					continue ;

				var switchList = (currentNode as PWNodeBiomeSwitch).switchList;

				var firstSwitch = switchList.switchDatas.First();

				Vector2 currentRange = paramRanges[switchList.currentSwitchMode];
				currentRange.x = Mathf.Min(currentRange.x, firstSwitch.absoluteMin);
				currentRange.y = Mathf.Max(currentRange.y, firstSwitch.absoluteMax);
				paramRanges[switchList.currentSwitchMode] = currentRange;
			}
		}

		List< BiomeSwitchData > GetPrecedentSwitchDatas(PWNode currentNode, PWNode rootNode)
		{
			var precedentSwitchDatas = new List< BiomeSwitchData >();

			PWNode n = currentNode;
			while (n != rootNode)
			{
				PWNode prevNode = n;
				n = n.GetInputNodes().First();
				if (n.GetType() != typeof(PWNodeBiomeSwitch))
					break ;
				
				var switches = (n as PWNodeBiomeSwitch).switchList;
				
				if (switches.currentSwitchMode == BiomeSwitchMode.Water)
				{
					var outputLinks = n.GetOutputLinks();

					foreach (var link in outputLinks)
						if (link.toNode == prevNode)
						{
							//terrestrial	-> 0
							//aquatic		-> 1

							int anchorIndex = link.fromAnchor.fieldIndex;
							var waterBiomeSwitchData = new BiomeSwitchData(){min = anchorIndex - .5f, max = anchorIndex + .5f, absoluteMin = -.5f, absoluteMax = 1.5f};
							waterBiomeSwitchData.color = (SerializableColor)((anchorIndex == 0) ? Color.red : Color.blue);
							precedentSwitchDatas.Add(waterBiomeSwitchData);
						}
				}
				else
				{
					int index = n.GetOutputNodes().ToList().FindIndex(c => c == prevNode);
	
					if (index == -1)
						throw new Exception("[PWBiomeSwitchGraph] IMPOSSIBRU !!!!! check your node API !");
					
					precedentSwitchDatas.Add(switches[index]);
				}
			}

			precedentSwitchDatas.Reverse();

			return precedentSwitchDatas;
		}

		void BuildLinks()
		{
			foreach (var cell1 in cells)
			{
				foreach (var cell2 in cells)
				{
					if (cell1 == cell2)
						continue ;
					
					bool b = (cell1.Overlaps(cell2));

					if (b)
					{
						if (!cell1.links.Any(c1 => c1 == cell2))
							cell1.links.Add(cell2);
					}
				}

				cell1.links.Sort((c1, c2) => {
					//reverse sort:
					return c2.weight.CompareTo(c1.weight);
				});
			}
		}

		bool CheckValid()
		{
			var	checkedCells = new List< BiomeSwitchCell >();
			var currentCells = new Stack< BiomeSwitchCell >();

			currentCells.Push(rootCell);

			while (currentCells.Count > 0)
			{
				var currentCell = currentCells.Pop();

				if (!checkedCells.Contains(currentCell))
					checkedCells.Add(currentCell);
				
				foreach (var link in currentCell.links)
					if (!checkedCells.Contains(link))
						currentCells.Push(link);
			}
			
			//if all graph cells are contained in the checkedCell list then it's good
			if (cells.All(cell => checkedCells.Contains(cell)))
				return true;
			
			return false;
		}

		BiomeSwitchCell		CheckForBetterCell(BiomeSwitchCell cell, BiomeSwitchValues values)
		{
			foreach (var link in cell.links)
				if (link.Matches(values))
					return link;
			
			return null;
		}

		BiomeSwitchCell		FindBestCell(BiomeSwitchCell cell, BiomeSwitchValues values)
		{
			BiomeSwitchCell	bestCell = null;
			float			minGap = 1e20f;

			foreach (var link in cell.links)
			{
				if (link.Matches(values))
					return link;
				
				float gap = link.GapWidth(cell);
				if (gap > 0 && gap < minGap)
				{
					minGap = gap;
					bestCell = link;
				}
			}

			return bestCell;
		}

		public BiomeSwitchCell	FindBiome(BiomeSwitchValues values)
		{
			BiomeSwitchCell currentCell = (lastCell == null) ? rootCell : lastCell;

			if (!isBuilt || currentCell == null)
				return null;
			
			int maxSearchDepth = 100;
			int	i = 0;
			

			while (true)
			{
				if (i > maxSearchDepth)
				{
					Debug.LogError("[BiomeSurfaceGraph] Infinite loop detected, exiting ...");
					return null;
				}
				
				i++;
				if (currentCell.Matches(values))
				{
					//try to find a better link
					var betterCell = CheckForBetterCell(currentCell, values);
					
					//if there is nothing better, we've the solution
					if (betterCell == null)
					{
						lastCell = currentCell;
						return currentCell;
					}
					else
						currentCell = betterCell;
					
					continue ;
				}
				else
				{
					//find the best link to take to get near from the node we look for
					currentCell = FindBestCell(currentCell, values);

					if (currentCell == null)
						return null;
				}
			}
		}

		public void FillBiomeMap(BiomeData biomeData)
		{
			biomeData.ids.Clear();

			int		terrainSize = biomeData.terrainRef.size;
			var		biomeSwitchValues = new BiomeSwitchValues();

			var		waterHeight = biomeData.waterHeight;
			var		terrainHeight = biomeData.terrain;
			var		temperature = biomeData.temperature;
			var		wetness = biomeData.wetness;
			
			biomeData.biomeIds = new BiomeMap2D(terrainHeight.size, terrainHeight.step);

			Stopwatch sw = new Stopwatch();

			sw.Start();

			for (int x = 0; x < terrainSize; x++)
				for (int y = 0; y < terrainSize; y++)
				{
					biomeSwitchValues[BiomeSwitchMode.Water] = (waterHeight != null && waterHeight[x, y] > 0) ? 1 : 0;
					biomeSwitchValues[BiomeSwitchMode.Height] = (terrainHeight != null) ? terrainHeight[x, y] : 0;
					biomeSwitchValues[BiomeSwitchMode.Temperature] = (temperature != null) ? temperature[x, y] : 0;
					biomeSwitchValues[BiomeSwitchMode.Wetness] = (wetness != null) ? wetness[x, y] : 0;

					var		biomeSwicthCell = FindBiome(biomeSwitchValues);
					short	biomeId = biomeSwicthCell.id;

					//TODO: optimize this with a dictionary, it's way too slow
					if (!biomeData.ids.Contains(biomeId))
						biomeData.ids.Add(biomeId);
					
					biomeData.biomeIds.SetFirstBiomeId(x, y, biomeId);
				}
			
			sw.Stop();

			Debug.Log("Graph build time: " + sw.ElapsedMilliseconds);
		}

		public int GetBiomeCount()
		{
			return cells.Count;
		}

		public PartialBiome GetBiome(string name)
		{
			PartialBiome	ret;

			partialBiomePerName.TryGetValue(name, out ret);

			return ret;
		}

		public PartialBiome GetBiome(short id)
		{
			PartialBiome	ret;

			partialBiomePerId.TryGetValue(id, out ret);

			return ret;
		}

		public Dictionary< BiomeSwitchMode, float > GetBiomeCoverage()
		{
			return biomeCoverage;
		}
	
	}
}