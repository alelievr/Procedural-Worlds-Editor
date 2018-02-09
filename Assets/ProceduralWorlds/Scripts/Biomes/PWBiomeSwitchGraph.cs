using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Biomator;
using PW.Node;
using System;
using System.Linq;

namespace PW.Biomator
{
	public class BiomeSwitchGraph
	{
		public class BiomeSwitchCellParam
		{
			public bool			enabled;
			public float		min;
			public float		max;

			public BiomeSwitchCellParam(bool enabled = false, float min = 1e20f, float max = -1e20f)
			{
				this.enabled = enabled;
				this.min = min;
				this.max = max;
			}
		}

		public class BiomeSwitchCell
		{
			public List< BiomeSwitchCell >	links = new List< BiomeSwitchCell >();
			public float					weight;
			public string					name;
			public Color					color;

			public BiomeSwitchCellParam[]	switchParams;

			public			BiomeSwitchCell()
			{
				switchParams = new BiomeSwitchCellParam[(int)BiomeSwitchMode.Count];

				for (int i = 0; i < switchParams.Length; i++)
					switchParams[i] = new BiomeSwitchCellParam(false);
			}

			public void		BindBiomeSwitch(BiomeSwitchData switchData)
			{
				name = switchData.name;
				color = switchData.color;
			}

			public bool		Overlaps(BiomeSwitchCell cell)
			{
				for (int i = 0; i < (int)BiomeSwitchMode.Count; i++)
					if (cell.switchParams[i].enabled && switchParams[i].enabled
						&& !PWUtils.Overlap(switchParams[i].min, switchParams[i].max, cell.switchParams[i].min, cell.switchParams[i].max))
							return false;
				return true;
			}

			public override string ToString()
			{
				string s = name + " = ";

				for (int i = 0; i < switchParams.Length; i++)
					if (switchParams[i].enabled)
						s += ((BiomeSwitchMode)i + ": " + switchParams[i].min + "->" + switchParams[i].max);

				return s;
			}
		}

		public BiomeSwitchCell				rootCell;
		public BiomeSwitchCell				lastCell;
		public List< BiomeSwitchCell >		cells = new List< BiomeSwitchCell >();

		[System.NonSerialized]
		public bool							isBuilt = false;

		[System.NonSerialized]
		public Dictionary< BiomeSwitchMode, Vector2 >	paramRanges = new Dictionary< BiomeSwitchMode, Vector2 >();

		public bool BuildGraph(PWNode rootNode)
		{
			isBuilt = false;

			Stack< PWNode > biomeNodes = new Stack< PWNode >();

			biomeNodes.Push(rootNode);

			BiomeSwitchCell currentCell = null;

			while (biomeNodes.Count > 0)
			{
				PWNode	currentNode = biomeNodes.Pop();
				Type	nodeType = currentNode.GetType();

				if (nodeType == typeof(PWNodeBiome))
				{
					currentCell = new BiomeSwitchCell();

					//get all precedent switch data in the order of the tree (start from rootNode)
					var precedentSwitchDatas = GetPrecedentSwitchDatas(currentNode, rootNode);

					//get last biomeSwitch and initialize the cell wit it
					currentCell.BindBiomeSwitch(precedentSwitchDatas.Last());

					//load all access condition for this biome switch
					foreach (var sd in precedentSwitchDatas)
					{
						var param = currentCell.switchParams[(int)sd.mode];

						param.enabled = true;
						param.min = sd.min;
						param.max = sd.max;
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

			//fill param range dictionary (used to compute weights)
			FillParamRange(rootNode);
			
			foreach (var cell1 in cells)
			{
				foreach (var cell2 in cells)
				{
					if (cell1 == cell2)
						continue ;
					
					if (cell1.Overlaps(cell2))
						AddLink(cell1, cell2);
				}

				cell1.links.Sort((c1, c2) => {
					//reverse sort:
					return c2.weight.CompareTo(c1.weight);
				});
			}

			foreach (var cell in cells)
			{
				Debug.Log("Cell: " + cell);
				foreach (var l in cell.links)
					Debug.Log("	link: " + l);
			}

			rootCell = cells.First();

			if (!CheckValid())
				return false;

			isBuilt = true;

			return true;
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
							precedentSwitchDatas.Add(new BiomeSwitchData(){min = anchorIndex, max = anchorIndex});
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

		void AddLink(BiomeSwitchCell cell1, BiomeSwitchCell cell2)
		{
			if (!cell1.links.Any(c1 => c1 == cell2))
				cell1.links.Add(cell2);
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

	}
}