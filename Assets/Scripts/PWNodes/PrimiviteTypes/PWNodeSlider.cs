using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeSlider : PWNode {
	
		[PWOutput]
		[PWColor(1, 0, 0)]
		public float	value1 = .5f;
		float	min = 0;
		float	max = 1;
	
		public override void OnNodeGUI()
		{
			EditorGUI.BeginChangeCheck();
			value1 = EditorGUILayout.Slider(value1, min, max);
			if (EditorGUI.EndChangeCheck())
				NotifyReload();
		}

		//no process needed, value is alrealy here with slider.
	}
}
