using UnityEngine;
using UnityEditor;

namespace PW.Core
{
	[System.SerializableAttribute]
	public class PWNodeGraphInput : PWNode {

		[PWOutput]
		[PWMultiple(0, typeof(object))]
		public PWValues				outputValues = new PWValues();
		
		public override void OnNodeCreation()
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
					if (i < names.Count)
						EditorGUILayout.LabelField(names[i] + ": " + values[i]);
					else if (values[i] != null)
						EditorGUILayout.LabelField(values[i].ToString());
			}
		}

		//no need to process this graph, datas are assigned form PWNodeGraphExternal
	}
}
