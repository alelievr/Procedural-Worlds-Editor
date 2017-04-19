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
			{
				EditorGUILayout.LabelField(obj.ToString());
				if (obj.GetType() == typeof(PWValues))
				{
					var pwv = obj as PWValues;

					for (int i = 0; i < pwv.Count; i++)
					{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("[" + i + "] " + pwv.NameAt(i) + ": " + pwv.At(i));
						EditorGUILayout.EndHorizontal();
					}
				}
			}
			else
				EditorGUILayout.LabelField("null");
		}

		//no process needed, no output.

	}
}