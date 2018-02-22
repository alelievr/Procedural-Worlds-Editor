using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeColor))]
	public class PWNodeColorEditor : PWNodeEditor
	{
		public PWNodeColor node;

		public override void OnNodeEnable()
		{
			node = target as PWNodeColor;
		}

		public override void OnNodeGUI()
		{
			PWGUI.ColorPicker(ref node.outputColor, true, false);
		}
	}
}