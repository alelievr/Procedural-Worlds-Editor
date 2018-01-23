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

		[PWInput, PWNotRequired]
		public BiomeSurfaceSwitch	inputSwitch;

		[PWInput]
		public BiomeSurfaceMaps		inputMaps;

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

		public override void OnNodeGUI()
		{
			if (PWGUI.BeginFade("Height limit", boxStyle, ref surfaceSwitch.heightEnabled))
			{
				EditorGUIUtility.labelWidth = 60;
				surfaceSwitch.minHeight = EditorGUILayout.FloatField("From", surfaceSwitch.minHeight);
				surfaceSwitch.maxHeight = EditorGUILayout.FloatField("To", surfaceSwitch.maxHeight);
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
			}
			PWGUI.EndFade();
		}

		public override void OnNodeProcess()
		{
		}
		
	}
}