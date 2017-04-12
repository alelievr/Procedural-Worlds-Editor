using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeGraphExternal : PWNode {

		[SerializeField]
		public PWNode	graphOutput;

		[PWInput("in")]
		[PWMultiple(0, typeof(object))] //accept all entering connections
		public PWValues	input = new PWValues();

		[PWOutput("out")]
		[PWGeneric(typeof(object))]
		public PWValues	output;

		public override void OnNodeCreate()
		{
			if (graphOutput != null)
				output = (graphOutput as PWNodeGraphOutput).inputValues;
		}

		public override void OnNodeGUI()
		{
			if (graphOutput != null)
				output = (graphOutput as PWNodeGraphOutput).inputValues;
				
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
				if (i < input.Count && inputNames[i] != null)
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

		public void InitGraphOut(PWNode @out)
		{
			graphOutput = @out;
		}
	}
}
