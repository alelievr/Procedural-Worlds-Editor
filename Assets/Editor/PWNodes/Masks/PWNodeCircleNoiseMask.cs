using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeCircleNoiseMask : PWNode {
	
		[PWInput("POS")]
		public Vector2	centerPosition;

		public override void OnNodeCreate()
		{
		}

		public override void OnNodeGUI()
		{
			EditorGUILayout.LabelField("pos: " + centerPosition);
		}
	}
}