using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeColor : PWNode {

		[PWOutput]
		public Color		outputColor;
	
		public override void OnNodeCreation()
		{
			name = "Color";
		}

		public override void OnNodeGUI()
		{
			PWGUI.ColorPicker(ref outputColor, true, false);
		}

		//no process needed
	}
}