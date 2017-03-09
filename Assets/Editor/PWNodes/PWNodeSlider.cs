using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW;
using UnityEditor;

public class PWNodeSlider : PWNode {

	[PWOutput]
	public float	value = .5f;
	float	min = 0;
	float	max = 1;

	public override void OnCreate()
	{
		name = "slider";
	}

	public override void OnNodeGUI()
	{
		value = EditorGUILayout.Slider(value, min, max);
	}
}