using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using PW.Core;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeBiomeData))]
	public class PWNodeBiomeDataEditor : PWNodeEditor
	{
		public PWNodeBiomeData node;

		public override void OnNodeEnable()
		{
			node = target as PWNodeBiomeData;
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(GUI.skin.label.lineHeight * 2f);
			EditorGUIUtility.labelWidth = 100;
			if (node.terrain != null)
			{
				if (node.terrain.type == SamplerType.Sampler2D)
				{
					EditorGUILayout.LabelField("Map terrain values:");
					EditorGUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth = 30;
					node.mapMin = EditorGUILayout.FloatField("from", node.mapMin);
					node.mapMax = EditorGUILayout.FloatField("to", node.mapMax);
					EditorGUILayout.EndHorizontal();
				}
			}
			else
				EditorGUILayout.LabelField("Connect a terrain plz.");
		}
	}
}