using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;
using System;
using PW.Core;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeTerrainDetail))]
	public class PWNodeTerrainDetailEditor : PWNodeEditor
	{
		public PWNodeTerrainDetail node;

		public override void OnNodeEnable()
		{
			node = target as PWNodeTerrainDetail;
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 100;
			node.outputDetail.biomeDetailMask = EditorGUILayout.MaskField("details", node.outputDetail.biomeDetailMask, Enum.GetNames(typeof(TerrainDetailType)));
		}

		public override void OnNodeDisable()
		{
			
		}
	}
}