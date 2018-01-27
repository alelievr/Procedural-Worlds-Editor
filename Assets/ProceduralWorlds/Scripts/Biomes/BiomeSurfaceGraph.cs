using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace PW.Biomator
{
	public class BiomeSurfaceGraph
	{
		enum BiomeSurfaceLinkType
		{
			Height,
			Slope,
			Temperature,
			Wetness,
			//...
		}

		class BiomeSurfaceLink
		{
			public BiomeSurfaceCell	toCell;
			public float			min;
			public float			max;

			public bool		isValid(float value)
			{
				return value >= min && value < max;
			}
		}

		class BiomeSurfaceLinks : Dictionary< BiomeSurfaceLinkType, List< BiomeSurfaceLink > > {}

		class BiomeSurfaceCell
		{
			public BiomeSurfaceLinks	links = new BiomeSurfaceLinks();
			public BiomeSurface			surface;
		}

		//the "first" cell of the graph (startpoint if lastCell is null)
		BiomeSurfaceCell		rootCell;
		//the last cell where the search algorithm stopped
		BiomeSurfaceCell		lastCell;

		List< BiomeSurfaceCell >	cells = new List< BiomeSurfaceCell >();

		List< BiomeSurfaceSwitch >	currentSwitches;

		List< BiomeSurfaceSwitch > SortWhere(Func< BiomeSurfaceSwitch, bool > condition, Comparison< BiomeSurfaceSwitch > comparison)
		{
			var cleanedList = currentSwitches.Where(condition).ToList();
			cleanedList.Sort(comparison);
			return cleanedList;
		}

		public void BuildGraph(IEnumerable< BiomeSurfaceSwitch > surfacesSwitches)
		{
			currentSwitches = surfacesSwitches.ToList();

			var switchSortedByHeight = SortWhere(s => s.heightEnabled, (s1, s2) => s1.maxHeight.CompareTo(s2.maxHeight));
			var switchSortedBySlope = SortWhere(s => s.slopeEnabled, (s1, s2) => s1.maxSlope.CompareTo(s2.maxSlope));
			// var switchSortedByParam = SortWhere(s => s.slopeEnabled, (s1, s2) => s1.maxSlope.CompareTo(s2.maxSlope));

			var bSwitchCellMap = new Dictionary< BiomeSurfaceSwitch, BiomeSurfaceCell >();

			Action< BiomeSurfaceLinkType, BiomeSurfaceCell, BiomeSurfaceSwitch > AddLink = (linkType, cell, s) => {
				var link = new BiomeSurfaceLink();

				link.toCell = bSwitchCellMap[s];

				switch (linkType)
				{
					case BiomeSurfaceLinkType.Height:
						link.min = s.minHeight;
						link.max = s.maxHeight;
						break ;
					case BiomeSurfaceLinkType.Slope:
						link.min = s.minSlope;
						link.max = s.maxSlope;
						break ;
					default :
						break ;
				}

				cell.links[linkType].Add(link);
			};

			//Generate surface switches nodes:
			foreach (var bSwitch in surfacesSwitches)
				bSwitchCellMap[bSwitch] = new BiomeSurfaceCell();

			foreach (var bSwitch in surfacesSwitches)
			{
				BiomeSurfaceCell cell = bSwitchCellMap[bSwitch];
				cell.surface = bSwitch.surface;

				//Initialize links:
				foreach (var surfaceLinkType in Enum.GetValues(typeof(BiomeSurfaceLinkType)))
					cell.links[(BiomeSurfaceLinkType)surfaceLinkType] = new List< BiomeSurfaceLink >();

				int heightIndex = switchSortedByHeight.IndexOf(bSwitch);
				if (heightIndex != -1)
				{
					if (heightIndex > 0)
						AddLink(BiomeSurfaceLinkType.Height, cell, switchSortedByHeight[heightIndex - 1]);
					if (heightIndex < switchSortedByHeight.Count - 1)
						AddLink(BiomeSurfaceLinkType.Height, cell, switchSortedByHeight[heightIndex + 1]);
				}
				
				int slopeIndex = switchSortedBySlope.IndexOf(bSwitch);
				if (slopeIndex != -1)
				{
					if (slopeIndex > 0)
						AddLink(BiomeSurfaceLinkType.Slope, cell, switchSortedBySlope[slopeIndex - 1]);
					if (slopeIndex < switchSortedBySlope.Count - 1)
						AddLink(BiomeSurfaceLinkType.Slope, cell, switchSortedBySlope[slopeIndex + 1]);
				}

			}
		}

		public BiomeSurface GetSurface(float height, float slope)
		{
			//TODO:
			return null;
		}
	}
}
