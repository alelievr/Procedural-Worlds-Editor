using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using System;

namespace PW.Node
{
	public class PWNodeTerrainDetail : PWNode
	{

		[PWOutput]
		public TerrainDetail	outputDetail = new TerrainDetail();
	
		public override void OnNodeCreation()
		{
			name = "Terrain Detail";
		}
		
	}
}