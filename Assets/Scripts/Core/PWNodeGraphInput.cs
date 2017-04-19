using UnityEngine;
using UnityEditor;

namespace PW
{
	[System.SerializableAttribute]
	public class PWNodeGraphInput : PWNode {

		[PWOutput]
		[PWMultiple(0, typeof(object))]
		public PWValues				outputValues = new PWValues();
		
		public override void OnNodeCreate()
		{

		}

		public override void OnNodeGUI()
		{
			EditorGUILayout.LabelField("inputs:");
			var names = outputValues.GetNames< object >();
			var values = outputValues.GetValues< object >();

			if (names != null && values != null)
			{
				for (int i = 0; i < values.Count; i++)
					EditorGUILayout.LabelField(names[i] + ": " + values[i]);
			}
		}

		public override void OnNodeProcess()
		{
			Debug.Log("input graph vals: ");
			for (int i = 0; i < outputValues.Count; i++)
				Debug.Log("input: " + outputValues.At(i));
		}

		//no need to process this graph, datas are assigned form PWNodeGraphExternal
	}
}
