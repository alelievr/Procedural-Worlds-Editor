using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Nodes;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeColor))]
	public class NodeColorEditor : BaseNodeEditor
	{
		public NodeColor node;

		public override void OnNodeEnable()
		{
			node = target as NodeColor;
		}

		public override void OnNodeGUI()
		{
			PWGUI.ColorPicker(ref node.outputColor, true, false);
		}
	}
}