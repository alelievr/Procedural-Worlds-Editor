using UnityEngine;
using UnityEditor;

namespace PW
{
	[System.SerializableAttribute]
	public class PWNodeGraphInput : PWNode {

		[PWOutput]
		[PWMultiple(0, typeof(object))]
		[System.NonSerializedAttribute]
		public PWValues				outputValues = null;
		
		[SerializeField]
		public PWNodeGraphExternal	externalGraphNode;

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
		}

		public override void OnNodeProcess()
		{
			if (externalGraphNode != null) //for the highest graph
				outputValues = externalGraphNode.input;
		}
	}
}
