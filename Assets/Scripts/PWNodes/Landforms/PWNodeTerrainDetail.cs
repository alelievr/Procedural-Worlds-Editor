using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using System;

namespace PW.Node
{
	public class PWNodeTerrainDetail : PWNode {

		[PWOutput]
		public TerrainDetail	outputDetail = new TerrainDetail();
	
		public override void OnNodeCreate()
		{
			externalName = "Terrain Detail";
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 100;
			outputDetail.biomeDetailMask = EditorGUILayout.MaskField("details", outputDetail.biomeDetailMask, Enum.GetNames(typeof(TerrainDetailType)));


		}

		public override void OnNodeProcess()
		{
		}
		
	}
}