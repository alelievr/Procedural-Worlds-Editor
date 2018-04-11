using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Nodes;
using ProceduralWorlds.Core;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeBiomeSurfaceSwitch))]
	public class NodeBiomeSurfaceSwicthEditor : BaseNodeEditor
	{
		public NodeBiomeSurfaceSwitch node;
		
		const string		biomeSurfaceSwitchKey = "BiomeSurfaceSwitch";

		public override void OnNodeEnable()
		{
			node = target as NodeBiomeSurfaceSwitch;
			
			//send reload to surface biome to rebuild the graph if switch values are updated
			delayedChanges.BindCallback(biomeSurfaceSwitchKey, (unused) => {
				NotifyReload();
			});
		}

		public override void OnNodeGUI()
		{
			node.UpdateSurfaceType(biomeGraphRef.surfaceType);

			EditorGUI.BeginChangeCheck();

			var outputSwitch = node.outputSwitch;

			if (PWGUI.BeginFade("Height limit", Styles.box, ref outputSwitch.heightEnabled))
			{
				EditorGUIUtility.labelWidth = 60;
				outputSwitch.minHeight = EditorGUILayout.FloatField("From", outputSwitch.minHeight);
				outputSwitch.maxHeight = EditorGUILayout.FloatField("To", outputSwitch.maxHeight);
				EditorGUIUtility.labelWidth = 0;
			}
			PWGUI.EndFade();
			if (PWGUI.BeginFade("Slope limit", Styles.box, ref outputSwitch.slopeEnabled))
			{
				PWGUI.MinMaxSlope(0, 90, ref outputSwitch.minSlope, ref outputSwitch.maxSlope);
			}
			PWGUI.EndFade();
			if (PWGUI.BeginFade("Param limit", Styles.box, ref outputSwitch.paramEnabled))
			{
				//TODO: modular input from BiomeSamplerName
				EditorGUIUtility.labelWidth = 60;
				outputSwitch.minParam = EditorGUILayout.FloatField("Min", outputSwitch.minParam);
				outputSwitch.maxParam = EditorGUILayout.FloatField("Max", outputSwitch.maxParam);
				EditorGUIUtility.labelWidth = 0;
			}
			PWGUI.EndFade();

			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(biomeSurfaceSwitchKey);

			outputSwitch.surface.type = biomeGraphRef.surfaceType;
			outputSwitch.details = node.inputDetails;
		}

	}
}