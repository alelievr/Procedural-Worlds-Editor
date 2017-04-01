using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeGraphExternal : PWNode {

		[SerializeField]
		public PWNode	graphInput;
		[SerializeField]
		public PWNode	graphOutput;

		[PWInput("in")]
		[PWMultiple(1, typeof(object))]
		public PWValues	input = new PWValues();

		[PWOutput("out")]
		public PWValues	output = new PWValues();

		public override void OnNodeGUI()
		{
			if (graphInput != null)
			{
				input = (graphInput as PWNodeGraphInput).inputValues;
				Debug.Log("assigned input");
			}
			if (graphOutput != null)
			{
				Debug.Log("assigned output");
				output = (graphOutput as PWNodeGraphOutput).outputValues;
			}
				
			if (GUILayout.Button("go into machine"))
				specialButtonClick = true;
			else
				specialButtonClick = false;
				
			EditorGUILayout.LabelField("inputs:");
			var names = input.GetNames< object >();
			foreach (var name in names)
			{
				if (name != null)
					EditorGUILayout.LabelField(name);
			}
		}

		public void InitGraphInAndOut(PWNode @in, PWNode @out)
		{
			graphInput = @in;
			graphOutput = @out;
		}
	}
}
