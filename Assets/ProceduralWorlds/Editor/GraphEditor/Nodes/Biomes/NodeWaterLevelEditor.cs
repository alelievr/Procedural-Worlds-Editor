using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Nodes;
using ProceduralWorlds.Core;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeWaterLevel))]
	public class NodeWaterLevelEditor : BaseNodeEditor
	{
		public NodeWaterLevel node;
		
		Gradient			waterGradient;
		
		bool				updateWaterPreview;
		
		const string		delayedUpdateKey = "delayedUpdate";

		public override void OnNodeEnable()
		{
			node = target as NodeWaterLevel;
			
			delayedChanges.BindCallback(delayedUpdateKey, (unused) => {
				node.UpdateWaterMap();
				NotifyReload();
			});
		}
		
		void UpdateGradient()
		{
			waterGradient = Utils.CreateGradient(GradientMode.Fixed, 
				new KeyValuePair< float, Color >(node.waterLevel / (node.mapMax - node.mapMin), Color.blue),
				new KeyValuePair< float, Color >(1, Color.white));
			PWGUI.SetGradientForField(PWGUIFieldType.Sampler2DPreview, 0, waterGradient);

			updateWaterPreview = true;
		}

		public override void OnNodeGUI()
		{
			PWGUI.SpaceSkipAnchors();
				
			EditorGUIUtility.labelWidth = 100;

			EditorGUI.BeginChangeCheck();
			{
				node.waterLevel = EditorGUILayout.FloatField("WaterLevel", node.waterLevel);
			}
			if (EditorGUI.EndChangeCheck())
			{
				delayedChanges.UpdateValue(delayedUpdateKey);
				updateWaterPreview = true;
			}
	
			if (node.terrainNoise != null)
			{
				EditorGUI.BeginChangeCheck();
				{
					if (node.terrainNoise.type == SamplerType.Sampler2D)
					{
						EditorGUILayout.LabelField("Map terrain values:");
						EditorGUILayout.BeginHorizontal();
						EditorGUIUtility.labelWidth = 30;
						node.mapMin = EditorGUILayout.FloatField("from", node.mapMin);
						node.mapMax = EditorGUILayout.FloatField("to", node.mapMax);
						EditorGUILayout.EndHorizontal();
	
						PWGUI.Sampler2DPreview(node.terrainNoise as Sampler2D, false, FilterMode.Point);
					}
					else
					{
						EditorGUILayout.LabelField("TODO: water level 3D");
					}
					if (waterGradient == null || EditorGUI.EndChangeCheck() || updateWaterPreview)
					{
						UpdateGradient();
						delayedChanges.UpdateValue(delayedUpdateKey);
						updateWaterPreview = false;
					}
				}
			}
		}

	}
}