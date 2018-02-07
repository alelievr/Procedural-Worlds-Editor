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
	public class PWBiomeSwitchGraph
	{
		public class BiomeSwitchCell
		{
			public List< BiomeSwitchCell >	links = new List< BiomeSwitchCell >();
			public float					weight;

			public BiomeSwitchData			switchData;
			public BiomeSwitchMode			switchMode;

			public BiomeSwitchCell(BiomeSwitchData switchData, BiomeSwitchMode switchMode)
			{
				this.switchData = switchData;
				this.switchMode = switchMode;
			}
		}

		public BiomeSwitchCell				rootCell;
		public BiomeSwitchCell				lastCell;
		public List< BiomeSwitchCell >		cells = new List< BiomeSwitchCell >();

		[System.NonSerialized]
		public bool							isBuilt = false;

		[System.NonSerialized]
		public Dictionary< BiomeSwitchMode, Vector2 >	paramRanges = new Dictionary< BiomeSwitchMode, Vector2 >();

		public bool BuildGraph(List< BiomeSwitchList > switchDatasList)
		{
			var bSwitchMap = new Dictionary< BiomeSwitchData, BiomeSwitchCell >();

			isBuilt = false;
			
			Action< BiomeSwitchCell, BiomeSwitchData > AddLink = (cell, data) => {
				var toCell = bSwitchMap[data];

				if (!cell.links.Any(c => c == toCell))
					cell.links.Add(toCell);
			};

			//fill param range dictionary (used to compute weights)
			foreach (var switchList in switchDatasList)
			{
				if (switchList.Count == 0)
					continue ;
				
				var firstSwitch = switchList.switchDatas.First();

				paramRanges[switchList.currentSwitchMode] = new Vector2(firstSwitch.absoluteMin, firstSwitch.absoluteMax);
				
				foreach (var switchData in switchList.switchDatas)
					bSwitchMap[switchData] = new BiomeSwitchCell(switchData, switchList.currentSwitchMode);
			}
			
			foreach (var switchList in switchDatasList)
			{
				foreach (var switchData in switchList.switchDatas)
				{
					var cell = bSwitchMap[switchData];

					foreach (var switchData2 in switchDatasList.SelectMany(s => s.switchDatas))
					{
						// if (switchData.Overlaps(switchData2))
							// AddLink(cell, switchData);
					}
				}
			}

			return false;
		}

	}
}