using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Node;
using PW.Core;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeBiomeGraphInput))]
	public class PWNodeBiomeGraphInputEditor : PWNodeEditor
	{
		public PWNodeBiomeGraphInput	node;

		BiomeDataDrawer					biomeDataDrawer = new BiomeDataDrawer();
		BiomeDataInputDrawer			biomeDataInputDrawer = new BiomeDataInputDrawer();

		readonly string					updateInputKey = "BiomeInputData";

		public override void OnNodeEnable()
		{
			node = target as PWNodeBiomeGraphInput;
			
			delayedChanges.BindCallback(updateInputKey, (unused) => NotifyReload());
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(14);
			
			PWGUI.PWArrayField(node.outputValues);
			
			EditorGUI.BeginChangeCheck();
			{
				EditorGUIUtility.labelWidth = 90;
				node.inputDataMode = (PWNodeBiomeGraphInput.BiomeDataInputMode)EditorGUILayout.EnumPopup("Input data mode", node.inputDataMode);
	
				EditorGUILayout.BeginVertical(PWStyles.box);
				{
					if (node.inputDataMode == PWNodeBiomeGraphInput.BiomeDataInputMode.MainGraph)
					{
						EditorGUILayout.LabelField("Preview graph");
						node.previewGraph = EditorGUILayout.ObjectField(node.previewGraph, typeof(PWMainGraph), false) as PWMainGraph;
		
						if (node.previewGraph == null)
							EditorGUILayout.HelpBox("Can't process the graph without a preview graph ", MessageType.Error);
						
						if (node.outputPartialBiome != null)
						{
							if (!biomeDataDrawer.isEnabled)
								biomeDataDrawer.OnEnable(node.outputPartialBiome);
							biomeDataDrawer.OnGUI(rect);
						}
					}
					else
					{
						if (!biomeDataInputDrawer.isEnabled)
							biomeDataInputDrawer.OnEnable(node.inputDataGenerator);
						biomeDataInputDrawer.OnGUI();
					}
				}
				EditorGUILayout.EndVertical();
			}
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(updateInputKey);
			
			node.calls = 0;
		}
	}
}