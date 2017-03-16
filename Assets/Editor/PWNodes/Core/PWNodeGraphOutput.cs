using UnityEditor;
using UnityEngine;

namespace PW
{
	public class PWNodeGraphOutput : PWNode {

		[PWMultiple(1, typeof(float), typeof(int), typeof(Vector2), typeof(Vector3), typeof(Vector4))]
		[PWInput]
		public PWValues		inputValues = new PWValues();

		public override void OnNodeCreate()
		{

		}

		public override void OnNodeGUI()
		{
			var names = inputValues.GetNames< object >();
			foreach (var name in names)
			{
				if (name != null)
					EditorGUILayout.LabelField(name);
			}
			EditorGUILayout.Space();
		}

		public override void OnNodeProcess()
		{

		}
	}
}
