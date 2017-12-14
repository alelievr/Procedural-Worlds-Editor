using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeSlider : PWNode
	{
	
		[PWOutput]
		[PWColor(1, 0, 0)]
		public float	value1 = .5f;
		float	min = 0;
		float	max = 1;

		string changeKey = "Slider";

		public override void OnNodeEnable()
		{
			delayedChanges.BindCallback(changeKey, (value) => { NotifyReload(); });
		}
	
		public override void OnNodeGUI()
		{
			EditorGUI.BeginChangeCheck();
			value1 = EditorGUILayout.Slider(value1, min, max);
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(changeKey, value1);
		}

		//no process needed, value already set by the slider.
	}
}
