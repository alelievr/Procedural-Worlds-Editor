using UnityEngine;
using UnityEditor;

namespace PW
{
	[System.SerializableAttribute]
	public class PWNodeGraphInput : PWNode {

		[PWOutput]
		[PWGeneric(typeof(object))]
		public PWValues		outputValues = new PWValues();

		public override void OnNodeCreate()
		{

		}

		public override void OnNodeGUI()
		{
			EditorGUILayout.LabelField("inputs:");
			var names = outputValues.GetNames< object >();
			if (names != null)
				foreach (var name in names)
				{
					if (name != null)
						EditorGUILayout.LabelField(name);
				}
			Debug.Log("input node input count: " + outputValues.Count);
		}

		public override void OnNodeProcess()
		{
			//done by PWNode when mirrored.
			/*var values = inputValues.GetValues< object >();
			var names = inputValuesGetNames< object >();

			for (int i = 0; i < values.Count; i++)
				outputValues.AssignAt(i, values[i], names[i]);*/
		}
	}
}
