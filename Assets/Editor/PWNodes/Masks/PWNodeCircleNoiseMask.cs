using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeCircleNoiseMask : PWNode {
	
		[PWInput("POS")]
		public Vector2	centerPosition;

		public override void OnNodeCreate()
		{
			name = "Circle noise mask";
		}

		public override void OnNodeGUI()
		{
			EditorGUILayout.LabelField("pos: " + centerPosition);
		}
	}
}