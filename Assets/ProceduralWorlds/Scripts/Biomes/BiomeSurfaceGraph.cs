using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace PW.Biomator
{
	public class BiomeSurfaceGraph
	{
		public class BiomeSurfaceLink
		{
			public BiomeSurfaceCell		toCell;
		}

		public class BiomeSurfaceCell
		{
			public List< BiomeSurfaceLink >	links = new List< BiomeSurfaceLink >();
			public BiomeSurface				surface;
			public BiomeSurfaceSwitch		surfaceSwitch;
			public float					weight;
		}

		//the "first" cell of the graph (startpoint if lastCell is null)
		public BiomeSurfaceCell		rootCell;
		//the last cell where the search algorithm stopped
		public BiomeSurfaceCell		lastCell;

		public List< BiomeSurfaceCell >	cells = new List< BiomeSurfaceCell >();

		public BiomeSurfaceType		surfaceType;

		[System.NonSerialized]
		public bool					isBuilt = false;

		public bool BuildGraph(List< BiomeSurfaceSwitch > surfacesSwitches)
		{
			var bSwitchCellMap = new Dictionary< BiomeSurfaceSwitch, BiomeSurfaceCell >();

			isBuilt = false;

			if (surfacesSwitches.Count == 0)
				return false;

			surfaceType = surfacesSwitches.First().surface.type;

			Action< BiomeSurfaceCell, BiomeSurfaceSwitch > AddLink = (cell, s) => {
				var link = new BiomeSurfaceLink();

				link.toCell = bSwitchCellMap[s];

				if (!cell.links.Any(c => c.toCell == link.toCell))
					cell.links.Add(link);
			};

			//calcul ranges
			float heightRange = surfacesSwitches.Max(s => s.maxHeight) - surfacesSwitches.Min(s => s.minHeight);
			float slopeRange = surfacesSwitches.Max(s => s.maxSlope) - surfacesSwitches.Min(s => s.minSlope);
			float paramRange = surfacesSwitches.Max(s => s.maxParam) - surfacesSwitches.Min(s => s.minParam);

			//Generate surface switches nodes:
			foreach (var bSwitch in surfacesSwitches)
				bSwitchCellMap[bSwitch] = new BiomeSurfaceCell();
			
			cells.Clear();
			lastCell = null;

			foreach (var bSwitch in surfacesSwitches)
			{
				BiomeSurfaceCell cell = bSwitchCellMap[bSwitch];
				cell.surface = bSwitch.surface;
				cell.surfaceSwitch = bSwitch;
				cell.weight = bSwitch.GetWeight(heightRange, slopeRange, paramRange);

				foreach (var biomeSwitch in surfacesSwitches)
					if (biomeSwitch.Overlaps(bSwitch))
						AddLink(cell, biomeSwitch);
					
				cell.links.Sort((l1, l2) => {
					float w1 = l1.toCell.weight;
					float w2 = l2.toCell.weight;

					//reverse sort
					return w2.CompareTo(w1);
				});
				
				cells.Add(cell);
			}

			rootCell = cells.First();

			if (!CheckValid())
				return false;

			isBuilt = true;

			return true;
		}

		bool CheckValid()
		{
			List< BiomeSurfaceCell >	checkedCells = new List< BiomeSurfaceCell >();

			Stack< BiomeSurfaceCell >	currentCells = new Stack< BiomeSurfaceCell >();

			currentCells.Push(rootCell);

			while (currentCells.Count > 0)
			{
				var currentCell = currentCells.Pop();
				
				if (!checkedCells.Contains(currentCell))
					checkedCells.Add(currentCell);

				foreach (var link in currentCell.links)
					if (!checkedCells.Contains(link.toCell))
						currentCells.Push(link.toCell);
			}

			//if all graph cells are contained in the checkedCell list then it's good
			if (cells.All(cell => checkedCells.Contains(cell)))
				return true;
			
			return false;
		}

		BiomeSurfaceCell	CheckForBetterCell(BiomeSurfaceCell cell, float height, float slope, float param)
		{
			foreach (var link in cell.links)
			{
				if (link.toCell.surfaceSwitch.Matches(height, slope, param))
				{
					if (link.toCell.weight < cell.weight)
						return link.toCell;
				}
			}

			return null;
		}

		BiomeSurfaceCell	FindBestCell(BiomeSurfaceCell currentCell, float height, float slope, float param)
		{
			BiomeSurfaceCell	bestCell = null;
			float				minGap = 1e20f;

			foreach (var link in currentCell.links)
			{
				if (link.toCell.surfaceSwitch.Matches(height, slope, param))
					return link.toCell;
				
				float gap = link.toCell.surfaceSwitch.GapWidth(currentCell.surfaceSwitch);
				if (gap > 0 && gap < minGap)
				{
					minGap = gap;
					bestCell = link.toCell;
				}
			}

			return bestCell;
		}

		public BiomeSurface GetSurface(float height = 0, float slope = 0, float param = 0)
		{
			BiomeSurfaceCell	currentCell = (lastCell == null) ? rootCell : lastCell;

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
				if (currentCell.surfaceSwitch.Matches(height, slope, param))
				{
					//try to find a better link
					var betterCell = CheckForBetterCell(currentCell, height, slope, param);
					
					//if there is nothing better, we've the solution
					if (betterCell == null)
					{
						lastCell = currentCell;
						return currentCell.surface;
					}
					else
						currentCell = betterCell;
					
					continue ;
				}
				else
				{
					//find the best link to take to get near from the node we look for
					currentCell = FindBestCell(currentCell, height, slope, param);

					if (currentCell == null)
						return null;
				}
			}
		}

		public IEnumerable< BiomeSurface > GetSurfaces()
		{
			foreach (var cell in cells)
				yield return cell.surface;
		}
	}
}
