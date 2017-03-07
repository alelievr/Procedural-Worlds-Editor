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

	public override void OnGUI()
	{
		EditorGUILayout.Slider(value, min, max);

		//do not forget this if you want the node links :)
		base.OnGUI();
	}
}
