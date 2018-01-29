using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeSlider : PWNode
	{
	
		[PWOutput]
		public float	outValue = .5f;
		public float	min = 0;
		public float	max = 1;

		string changeKey = "Slider";

		public override void OnNodeEnable()
		{
			delayedChanges.BindCallback(changeKey, (value) => { NotifyReload(); });
		}
	
		public override void OnNodeGUI()
		{
			EditorGUI.BeginChangeCheck();
			outValue = EditorGUILayout.Slider(outValue, min, max);
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(changeKey, outValue);
		}

		//no process needed, value already set by the slider.
	}
}
