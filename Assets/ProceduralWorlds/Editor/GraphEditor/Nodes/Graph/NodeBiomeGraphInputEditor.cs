using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Node;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiomeGraphInput))]
	public class NodeBiomeGraphInputEditor : NodeEditor
	{
		public NodeBiomeGraphInput	node;

		BiomeDataDrawer					biomeDataDrawer = new BiomeDataDrawer();
		BiomeDataInputDrawer			biomeDataInputDrawer = new BiomeDataInputDrawer();

		readonly string					updateInputKey = "BiomeInputData";

		public override void OnNodeEnable()
		{
			node = target as NodeBiomeGraphInput;
			
			delayedChanges.BindCallback(updateInputKey, (unused) => NotifyReload());
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(14);
			
			PWGUI.PWArrayField(node.outputValues);
			
			EditorGUI.BeginChangeCheck();
			{
				EditorGUIUtility.labelWidth = 90;
				node.inputDataMode = (NodeBiomeGraphInput.BiomeDataInputMode)EditorGUILayout.EnumPopup("Input data mode", node.inputDataMode);
	
				EditorGUILayout.BeginVertical(Styles.box);
				{
					if (node.inputDataMode == NodeBiomeGraphInput.BiomeDataInputMode.WorldGraph)
					{
						EditorGUILayout.LabelField("Preview graph");
						node.previewGraph = EditorGUILayout.ObjectField(node.previewGraph, typeof(WorldGraph), false) as WorldGraph;
		
						if (node.previewGraph == null)
							EditorGUILayout.HelpBox("Can't process the graph without a preview graph ", MessageType.Error);
					}
					else
					{
						if (!biomeDataInputDrawer.isEnabled)
							biomeDataInputDrawer.OnEnable(node.inputDataGenerator);
						biomeDataInputDrawer.OnGUI();
					}
				}
				EditorGUILayout.EndVertical();
				
				if (node.outputPartialBiome != null)
				{
					if (!biomeDataDrawer.isEnabled)
						biomeDataDrawer.OnEnable(node.outputPartialBiome);
					biomeDataDrawer.OnGUI(rect);
				}
			}
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(updateInputKey);
			
			node.calls = 0;
		}
	}
}