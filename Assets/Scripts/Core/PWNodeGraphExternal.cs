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
		[PWMultiple(0, typeof(object))] //accept all entering connections
		public PWValues	input;

		[PWOutput("out")]
		[PWGeneric(typeof(object))]
		public PWValues	output;

		public override void OnNodeCreate()
		{
			if (graphInput != null)
				input = (graphInput as PWNodeGraphInput).outputValues;
			if (graphOutput != null)
				output = (graphOutput as PWNodeGraphOutput).inputValues;
		}

		public override void OnNodeGUI()
		{
			if (GUILayout.Button("go into machine"))
				specialButtonClick = true;
			else
				specialButtonClick = false;
			
			if (graphInput != null)
				input = (graphInput as PWNodeGraphInput).outputValues;
			if (graphOutput != null)
				output = (graphOutput as PWNodeGraphOutput).inputValues;
				
			if (input == null || output == null)
				return ;

			Debug.Log("input count: " + input.Count + " for " + GetHashCode());

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
