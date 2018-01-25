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
		public BiomeSurfaceSwitch	outputSwitch;

		string propUpdateKey = "PWNodeBiomeSurfaceSwitch";

		[SerializeField]
		BiomeSurfaceSwitch	surfaceSwitch = new BiomeSurfaceSwitch();

		GUIStyle		boxStyle;

		public override void OnNodeCreation()
		{
			name = "Surface switch";
			surfaceSwitch.minSlope = 0;
			surfaceSwitch.maxSlope = 90;
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

			if (PWGUI.BeginFade("Height limit", boxStyle, ref surfaceSwitch.heightEnabled))
			{
				EditorGUIUtility.labelWidth = 60;
				surfaceSwitch.minHeight = EditorGUILayout.FloatField("From", surfaceSwitch.minHeight);
				surfaceSwitch.maxHeight = EditorGUILayout.FloatField("To", surfaceSwitch.maxHeight);
				EditorGUIUtility.labelWidth = 0;
			}
			PWGUI.EndFade();
			if (PWGUI.BeginFade("Slope limit", boxStyle, ref surfaceSwitch.slopeEnabled))
			{
				PWGUI.MinMaxSlope(0, 90, ref surfaceSwitch.minSlope, ref surfaceSwitch.maxSlope);
			}
			PWGUI.EndFade();
			if (PWGUI.BeginFade("Param limit", boxStyle, ref surfaceSwitch.paramEnabled))
			{
				surfaceSwitch.paramType = (BiomeSwitchMode)EditorGUILayout.EnumPopup(surfaceSwitch.paramType);
				EditorGUIUtility.labelWidth = 60;
				surfaceSwitch.minParam = EditorGUILayout.FloatField("Min", surfaceSwitch.minParam);
				surfaceSwitch.maxParam = EditorGUILayout.FloatField("Max", surfaceSwitch.maxParam);
				EditorGUIUtility.labelWidth = 0;
			}
			PWGUI.EndFade();
		}

		public override void OnNodeProcess()
		{
		}
		
	}
}