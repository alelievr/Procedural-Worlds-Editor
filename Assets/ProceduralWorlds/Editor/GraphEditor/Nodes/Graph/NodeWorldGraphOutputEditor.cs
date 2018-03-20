using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeWorldGraphOutput))]
	public class NodeWorldGraphOutputEditor : BaseNodeEditor
	{
		NodeWorldGraphOutput node;

		public override void OnNodeEnable()
		{
			node = target as NodeWorldGraphOutput;
			//initialize here all unserializable datas used for GUI (like Texture2D, ...)
		}

		public override void OnNodeGUI()
		{
			//write here the process which take inputs, transform them and set outputs.

			PWGUI.SpaceSkipAnchors();

			PWGUI.PWArrayField(node.inputValues);
		}
		
	}
}