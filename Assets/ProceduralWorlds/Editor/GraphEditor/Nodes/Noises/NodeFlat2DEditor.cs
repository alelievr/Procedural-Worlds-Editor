using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;
using ProceduralWorlds.Nodes;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeFlat2D))]
	public class NodeFlat2DEditor : BaseNodeEditor
	{
		NodeFlat2D	node;

		string propUpdateKey = "NodeFlat2D";

		public override void OnNodeEnable()
		{
			node = target as NodeFlat2D;
			
			delayedChanges.BindCallback(propUpdateKey, (unused) => { NotifyReload(); });
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 70;
			EditorGUI.BeginChangeCheck();
			{
				node.value = EditorGUILayout.FloatField("Value", node.value);
			}
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(propUpdateKey);
		}
		
	}
}