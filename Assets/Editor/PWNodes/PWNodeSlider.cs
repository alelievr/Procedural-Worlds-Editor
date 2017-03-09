using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW;
using UnityEditor;

[System.SerializableAttribute]
public class PWNodeSlider : PWNode {

	[PWOutput("V")]
	public float	value1 = .5f;
	float	min = 0;
	float	max = 1;

	public override void OnNodeCreate()
	{
		name = "slider";
	}

	public override void OnNodeGUI()
	{
		value1 = EditorGUILayout.Slider(value1, min, max);
	}
}