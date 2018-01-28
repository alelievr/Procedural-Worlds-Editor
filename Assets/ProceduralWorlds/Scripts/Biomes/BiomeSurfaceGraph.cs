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

		List< BiomeSurfaceSwitch >	currentSwitches;

		public void BuildGraph(IEnumerable< BiomeSurfaceSwitch > surfacesSwitches)
		{
			currentSwitches = surfacesSwitches.ToList();

			var bSwitchCellMap = new Dictionary< BiomeSurfaceSwitch, BiomeSurfaceCell >();

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
		}

		bool IsFinalCell(BiomeSurfaceCell cell, float height, float slope)
		{
			if (cell.surfaceSwitch.Matches(height, slope, 0))
			{
				//TODO: weight calcul and check if there is a better node in links
				return true;
			}
			return false;
		}

		public BiomeSurface GetSurface(float height, float slope)
		{
			BiomeSurfaceCell	currentCell = (lastCell == null) ? rootCell : lastCell;

			if (IsFinalCell(currentCell, height, slope))
				return currentCell.surface;
			
			while (true)
			{
				foreach (var link in currentCell.links)
				{
					if (link.toCell.surfaceSwitch.Matches(height, slope, 0))
					{
						if (link.toCell.weight < currentCell.weight)
						{
							currentCell = link.toCell;
							lastCell = currentCell;
							return currentCell.surface;
						}
					}
				}
			}
		}
	}
}
