using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Node;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeColor))]
	public class NodeColorEditor : NodeEditor
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