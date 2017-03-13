using UnityEditor;

namespace PW
{
	public class PWNodeDebugLog : PWNode {
	
		[PWInput("obj")]
		public object		obj;

		public override void OnNodeCreate()
		{
			name = "Debug log node";
			renamable = true;
			obj = "null";
		}

		public override void OnNodeGUI()
		{
			if (obj != null)
				EditorGUILayout.LabelField(obj.ToString());
			else
				EditorGUILayout.LabelField("null");
		}

		//no process needed, no output.

	}
}