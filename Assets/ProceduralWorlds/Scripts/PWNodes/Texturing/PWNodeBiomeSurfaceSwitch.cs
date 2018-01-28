using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeSurfaceSwitch : PWNode
	{

		[PWInput]
		public BiomeSurfaceMaps		inputMaps;

		[PWInput]
		public BiomeSurfaceColor	inputColor;

		[PWInput]
		public BiomeSurfaceMaterial	inputMaterial;

		[PWOutput]
		public BiomeSurfaceSwitch	outputSwitch = new BiomeSurfaceSwitch();

		GUIStyle			boxStyle;

		readonly string		biomeSurfaceSwitchKey = "BiomeSurfaceSwitch";

		public override void OnNodeCreation()
		{
			name = "Surface switch";
			outputSwitch.minSlope = 0;
			outputSwitch.maxSlope = 90;
		}

		public override void OnNodeEnable()
		{
			//send reload to surface biome to rebuild the graph if switch values are updated
			delayedChanges.BindCallback(biomeSurfaceSwitchKey, (unused) => {
				NotifyReload(typeof(PWNodeBiomeSurface));
			});
		}

		public override void OnNodeLoadStyle()
		{
			using (new DefaultGUISkin())
			{
				boxStyle = new GUIStyle("box");
			}
		}

		public void UpdateSurfaceType(BiomeSurfaceType surfaceType)
		{
			switch (surfaceType)
			{
				case BiomeSurfaceType.SurfaceMaps:
					SetAnchorVisibility("inputMaps", PWVisibility.Visible);
					SetAnchorVisibility("inputColor", PWVisibility.Gone);
					SetAnchorVisibility("inputMaterial", PWVisibility.Gone);
					break ;
				case BiomeSurfaceType.Color:
					SetAnchorVisibility("inputMaps", PWVisibility.Gone);
					SetAnchorVisibility("inputColor", PWVisibility.Visible);
					SetAnchorVisibility("inputMaterial", PWVisibility.Gone);
					break ;
				case BiomeSurfaceType.Material:
					SetAnchorVisibility("inputMaps", PWVisibility.Gone);
					SetAnchorVisibility("inputColor", PWVisibility.Gone);
					SetAnchorVisibility("inputMaterial", PWVisibility.Visible);
					break ;
			}
		}

		public override void OnNodeGUI()
		{
			UpdateSurfaceType(biomeGraphRef.surfaceType);

			EditorGUI.BeginChangeCheck();

			if (PWGUI.BeginFade("Height limit", boxStyle, ref outputSwitch.heightEnabled))
			{
				EditorGUIUtility.labelWidth = 60;
				outputSwitch.minHeight = EditorGUILayout.FloatField("From", outputSwitch.minHeight);
				outputSwitch.maxHeight = EditorGUILayout.FloatField("To", outputSwitch.maxHeight);
				EditorGUIUtility.labelWidth = 0;
			}
			PWGUI.EndFade();
			if (PWGUI.BeginFade("Slope limit", boxStyle, ref outputSwitch.slopeEnabled))
			{
				PWGUI.MinMaxSlope(0, 90, ref outputSwitch.minSlope, ref outputSwitch.maxSlope);
			}
			PWGUI.EndFade();
			if (PWGUI.BeginFade("Param limit", boxStyle, ref outputSwitch.paramEnabled))
			{
				outputSwitch.paramType = (BiomeSwitchMode)EditorGUILayout.EnumPopup(outputSwitch.paramType);
				EditorGUIUtility.labelWidth = 60;
				outputSwitch.minParam = EditorGUILayout.FloatField("Min", outputSwitch.minParam);
				outputSwitch.maxParam = EditorGUILayout.FloatField("Max", outputSwitch.maxParam);
				EditorGUIUtility.labelWidth = 0;
			}
			PWGUI.EndFade();

			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(biomeSurfaceSwitchKey);

			outputSwitch.surface.type = biomeGraphRef.surfaceType;
		}

	}
}