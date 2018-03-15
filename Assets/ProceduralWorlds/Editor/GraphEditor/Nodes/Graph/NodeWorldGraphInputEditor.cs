using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeWorldGraphInput))]
	public class NodeWorldGraphInputEditor : BaseNodeEditor
	{
		NodeWorldGraphInput	node;
	
		string propUpdateKey = "NodeWorldGraphInput";

		public override void OnNodeEnable()
		{
			//initialize here all unserializable datas used for GUI (like Texture2D, ...)

			node = target as NodeWorldGraphInput;
			
			delayedChanges.BindCallback(propUpdateKey, (unused) => { NotifyReload(); });
		}

		public override void OnNodeGUI()
		{
			//write here the process which take inputs, transform them and set outputs.

			PWGUI.PWArrayField(node.outputValues);
		}
		
	}
}