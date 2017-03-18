using UnityEditor;

namespace PW
{
	public class PWNodeGraphInput : PWNode {

		[PWMultiple(0)]
		[PWInput]
		public PWValues		inputValues = new PWValues();
		
		[PWOutput]
		[PWMirror("inputValues")]
		public PWValues		outputValues = new PWValues();

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
		}

		public override void OnNodeProcess()
		{
			var values = inputValues.GetValues< object >();
			var names = inputValues.GetNames< object >();

			for (int i = 0; i < values.Count; i++)
				outputValues.AssignAt(i, values[i], names[i]);
		}
	}
}
