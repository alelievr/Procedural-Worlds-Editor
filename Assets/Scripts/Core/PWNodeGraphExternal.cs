using UnityEngine;
using UnityEditor;

namespace PW.Core
{
	public class PWNodeGraphExternal : PWNode {

		[SerializeField]
		public PWNodeGraphInput		graphInput;
		[SerializeField]
		public PWNodeGraphOutput	graphOutput;

		[PWInput("in")]
		[PWMultiple(0, typeof(object))] //accept all entering connections
		[PWNotRequired]
		public PWValues	input = new PWValues();

		[PWOutput("out")]
		[PWMultiple(0, typeof(object))]
		public PWValues	output = new PWValues();

		public override void OnNodeCreate()
		{
			renamable = true;
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
			for (int i = 0; i < inputNames.Count || i < outputNames.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				if (i < inputNames.Count && inputNames[i] != null)
					EditorGUILayout.LabelField(inputNames[i], GUILayout.MaxWidth(100));
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
			while (input.Count < graphInput.outputValues.Count)
				graphInput.outputValues.RemoveAt(0);

			//push input values to the subgraph's input node:
			for (int i = 0; i < input.Count; i++)
				graphInput.outputValues.AssignAt(i, input.At(i), input.NameAt(i), true);
		}

		public void InitGraphOut(PWNode @in, PWNode @out)
		{
			// graphOutput = @out as PWNodeGraphOutput;
			graphInput = @in as PWNodeGraphInput;
			graphOutput = @out as PWNodeGraphOutput;
		}
	}
}
