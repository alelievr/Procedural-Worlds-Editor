using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;
using ProceduralWorlds.Nodes;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeFlat3D))]
	public class NodeFlat3DEditor : BaseNodeEditor
	{
		NodeFlat3D	node;

		string propUpdateKey = "NodeFlat3D";

		public override void OnNodeEnable()
		{
			node = target as NodeFlat3D;
			
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