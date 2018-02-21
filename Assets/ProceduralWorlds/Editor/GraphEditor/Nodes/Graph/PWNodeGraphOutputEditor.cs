using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using PW.Core;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeGraphOutput))]
	public class PWNodeGraphOutputEditor : PWNodeEditor
	{
		public PWNodeGraphOutput node;

		public override void OnNodeEnable()
		{
			node = target as PWNodeGraphOutput;
		}

		public override void OnNodeGUI()
		{
			PWGUI.PWArrayField(node.inputValues);
		}
	}
}