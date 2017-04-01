using UnityEditor;
using UnityEngine;

namespace PW
{
	[System.SerializableAttribute]
	public class PWNodeGraphOutput : PWNode {

		//Mark all possible output types:
		[PWMultiple(1, typeof(SideView2DData), typeof(TopDown2DData))]
		[PWInput("in")]
		[PWOffset(0, 20)]
		public PWValues		outputValues = new PWValues();

		public override void OnNodeCreate()
		{

		}

		public override void OnNodeGUI()
		{
			var names = outputValues.GetNames< object >();
			var values = outputValues.GetValues< object >();

			EditorGUILayout.LabelField("names: [" + names.Count + "]");
			for (int i = 0; i < names.Count; i++)
			{
				if (names[i] != null && values[i] != null)
					EditorGUILayout.LabelField(names[i] + " <" /*+ values[i].GetType() + ": "*/ + values[i] + ">");
				else
					EditorGUILayout.LabelField("null");
			}

			//TODO: dynamically remove unlinked nodes.
		}

		public override void OnNodeProcess()
		{
		}
	}
}
