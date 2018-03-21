using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Node;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiomeSurfaceColor))]
	public class NodeBiomeSurfaceColorEditor : BaseNodeEditor
	{
		public NodeBiomeSurfaceColor node;

		public override void OnNodeEnable()
		{
			node = target as NodeBiomeSurfaceColor;
		}

		public override void OnNodeGUI()
		{
			PWGUI.ColorPicker("Base color", ref node.surfaceColor.baseColor);
		}
	}
}