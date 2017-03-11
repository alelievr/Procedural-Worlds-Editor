using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;

namespace PW
{
	[System.SerializableAttribute]
	public class PWNodeDebugLog : PWNode {
	
		[PWInput("obj")]
		public object		obj;

		public override void OnNodeCreate()
		{
			name = "NodeDebugLog";
			obj = "null";
		}

		public override void OnNodeGUI()
		{
			if (obj != null)
				EditorGUILayout.LabelField(obj.ToString());
			else
				EditorGUILayout.LabelField("null");
		}

	}
}