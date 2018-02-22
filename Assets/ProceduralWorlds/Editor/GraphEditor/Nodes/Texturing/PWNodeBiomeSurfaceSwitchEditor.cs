using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using PW.Core;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeBiomeSurfaceSwitch))]
	public class PWNodeBiomeSurfaceSwicthEditor : PWNodeEditor
	{
		public PWNodeBiomeSurfaceSwitch node;
		
		const string		biomeSurfaceSwitchKey = "BiomeSurfaceSwitch";

		public override void OnNodeEnable()
		{
			node = target as PWNodeBiomeSurfaceSwitch;
			
			//send reload to surface biome to rebuild the graph if switch values are updated
			delayedChanges.BindCallback(biomeSurfaceSwitchKey, (unused) => {
				NotifyReload();
			});
		}

		public override void OnNodeGUI()
		{
			UpdateSurfaceType(biomeGraphRef.surfaceType);

			EditorGUI.BeginChangeCheck();

			var outputSwitch = node.outputSwitch;

			if (PWGUI.BeginFade("Height limit", PWStyles.box, ref outputSwitch.heightEnabled))
			{
				EditorGUIUtility.labelWidth = 60;
				outputSwitch.minHeight = EditorGUILayout.FloatField("From", outputSwitch.minHeight);
				outputSwitch.maxHeight = EditorGUILayout.FloatField("To", outputSwitch.maxHeight);
				EditorGUIUtility.labelWidth = 0;
			}
			PWGUI.EndFade();
			if (PWGUI.BeginFade("Slope limit", PWStyles.box, ref outputSwitch.slopeEnabled))
			{
				PWGUI.MinMaxSlope(0, 90, ref outputSwitch.minSlope, ref outputSwitch.maxSlope);
			}
			PWGUI.EndFade();
			if (PWGUI.BeginFade("Param limit", PWStyles.box, ref outputSwitch.paramEnabled))
			{
				//TODO: modular input from BiomeSamplerName
				// outputSwitch.paramType = (BiomeSwitchMode)EditorGUILayout.EnumPopup(outputSwitch.paramType);
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