using UnityEditor;
using UnityEngine;

namespace PW
{
	public class PWNodeGraphOutput : PWNode {

		[PWMultiple(1, typeof(float), typeof(int), typeof(Vector2), typeof(Vector3), typeof(Vector4))]
		[PWInput("in")]
		[PWOffset(0, 20)]
		public PWValues		inputValues = new PWValues();

		public override void OnNodeCreate()
		{

		}

		public override void OnNodeGUI()
		{
			var names = inputValues.GetNames< object >();
			var values = inputValues.GetValues< object >();

			//if the output node is displayed for an upper layer:
			if (useExternalWinowRect)
			{
				if (GUILayout.Button("go into machine"))
					specialButtonClick = true;
				else
					specialButtonClick = false;
			}

			EditorGUILayout.LabelField("names: [" + names.Count + "]");
			for (int i = 0; i < names.Count; i++)
			{
				if (names[i] != null)
					EditorGUILayout.LabelField(names[i] + " <" + values[i].GetType() + ": " + values[i] + ">");
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
