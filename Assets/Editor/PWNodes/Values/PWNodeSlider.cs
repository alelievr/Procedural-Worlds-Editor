using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW;
using UnityEditor;

namespace PW
{
	public class PWNodeSlider : PWNode {
	
		[PWOutput("V")]
		[PWColor(1, 0, 0)]
		public float	value1 = .5f;
		float	min = 0;
		float	max = 1;
	
		public override void OnNodeCreate()
		{
		}
	
		public override void OnNodeGUI()
		{
			value1 = EditorGUILayout.Slider(value1, min, max);
		}
	}
}