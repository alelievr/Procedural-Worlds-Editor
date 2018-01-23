using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using PW.Core;

namespace PW.Node
{
	public class PWNodeBiomeSurfaceSwitch : PWNode
	{
	
		string propUpdateKey = "PWNodeBiomeSurfaceSwitch";

		GUIStyle	boxStyle;
		AnimBool	showHeightLimit;
		AnimBool	showSlopeLimit;
		AnimBool	showParamLimit;

		public override void OnNodeCreation()
		{
			name = "Surface switch";
		}

		public override void OnNodeEnable()
		{
			using (new DefaultGUISkin())
			{
				boxStyle = new GUIStyle("box");
			}
		}

		public override void OnNodeGUI()
		{
			EditorGUILayout.BeginVertical(boxStyle);
			EditorGUILayout.BeginFadeGroup(showHeightLimit.faded);
			{
				EditorGUILayout.LabelField("Label");
			}
			EditorGUILayout.EndFadeGroup();
			EditorGUILayout.EndVertical();
		}

		public override void OnNodeProcess()
		{
		}
		
	}
}