using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeBiomeSurfaceColor))]
	public class PWNodeBiomeSurfaceColorEditor : PWNodeEditor
	{
		public PWNodeBiomeSurfaceColor node;

		public override void OnNodeEnable()
		{
			node = target as PWNodeBiomeSurfaceColor;
		}

		public override void OnNodeGUI()
		{
			PWGUI.ColorPicker("Base color", ref node.surfaceColor.baseColor);
		}
	}
}