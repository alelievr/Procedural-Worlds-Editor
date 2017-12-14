using UnityEditor;
using UnityEngine;

namespace PW.Core
{
	[System.SerializableAttribute]
	public class PWNodeGraphOutput : PWNode {

		//allow everything as output type
		[PWInput]
		public PWArray< object >	inputValues = new PWArray< object >();

		public override void OnNodeCreation()
		{

		}

		public override void OnNodeGUI()
		{
			var names = inputValues.GetNames();
			var values = inputValues.GetValues();

			EditorGUILayout.LabelField("names: [" + names.Count + "]");
			for (int i = 0; i < values.Count; i++)
			{
				if (i < names.Count && names[i] != null)
				{
					if (values[i] != null)
						EditorGUILayout.LabelField(names[i] + " <" + values[i].GetType() + ": " + values[i] + ">");
					else
						EditorGUILayout.LabelField(names[i]);
				}
				else
					EditorGUILayout.LabelField("null");
			}
		}

		public override void OnNodeProcess()
		{
		}
	}
}
