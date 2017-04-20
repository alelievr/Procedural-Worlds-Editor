using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeGraphExternal : PWNode {

		[SerializeField]
		public PWNodeGraphInput		graphInput;

		[PWInput("in")]
		[PWMultiple(0, typeof(object))] //accept all entering connections
		public PWValues	input = new PWValues();

		[PWOutput("out")]
		[PWGeneric(typeof(object))]
		public PWValues	output = null;

		public override void OnNodeCreate()
		{
		}

		public override void OnNodeGUI()
		{
			if (output == null)
				return ;

			if (GUILayout.Button("go into machine"))
				specialButtonClick = true;
			else
				specialButtonClick = false;

			var inputNames = input.GetNames< object >();
			var outputNames = output.GetNames< object >();
			var inputValues = input.GetValues< object >();
			for (int i = 0; i < inputNames.Count || i < outputNames.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				if (i < input.Count && inputNames[i] != null)
					EditorGUILayout.LabelField(inputNames[i] + ":" + inputValues[i], GUILayout.MaxWidth(200));
				else
					EditorGUILayout.LabelField("");
				if (i < outputNames.Count && outputNames[i] != null)
					EditorGUILayout.LabelField(outputNames[i], GUILayout.MaxWidth(100));
				else
					EditorGUILayout.LabelField("");
				EditorGUILayout.EndHorizontal();
			}
		}

		public override void OnNodeProcess()
		{
			//push input values to the subgraph's input node:
			for (int i = 0; i < input.Count; i++)
				graphInput.outputValues.AssignAt(i, input.At(i), input.NameAt(i), true);
		}

		public void InitGraphOut(PWNode @in, PWNode @out)
		{
			// graphOutput = @out as PWNodeGraphOutput;
			graphInput = @in as PWNodeGraphInput;
		}
	}
}
