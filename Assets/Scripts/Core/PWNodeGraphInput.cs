using UnityEngine;
using UnityEditor;

namespace PW
{
	[System.SerializableAttribute]
	public class PWNodeGraphInput : PWNode {

		[PWOutput]
		[PWGeneric(typeof(object))]
		public PWValues				outputValues = null;
		
		[SerializeField]
		public PWNodeGraphExternal	externalGraphNode;

		public override void OnNodeCreate()
		{

		}

		public override void OnNodeGUI()
		{
			outputValues = externalGraphNode.input;
			
			EditorGUILayout.LabelField("inputs:");
			var names = outputValues.GetNames< object >();
			if (names != null)
				foreach (var name in names)
				{
					if (name != null)
						EditorGUILayout.LabelField(name);
				}
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
