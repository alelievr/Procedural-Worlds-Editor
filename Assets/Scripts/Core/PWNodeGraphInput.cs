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
			Debug.Log("outputValues hash: " + outputValues.GetHashCode());
			Debug.Log("count: " + outputValues.Count);
			EditorGUILayout.LabelField("inputs:");
			var names = outputValues.GetNames< object >();
			if (names != null)
				foreach (var name in names)
				{
					if (name != null)
						EditorGUILayout.LabelField(name);
				}
		}

		//no need to process this graph, datas are assigned form PWNodeGraphExternal
	}
}
