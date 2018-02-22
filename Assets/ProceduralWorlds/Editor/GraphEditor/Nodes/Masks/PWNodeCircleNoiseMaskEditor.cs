using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;

namespace PW.Editor
{
	public class PWNodeCircleNoiseMaskEditor : PWNodeEditor
	{
		public PWNodeCircleNoiseMask node;

		public override void OnNodeEnable()
		{
			node = target as PWNodeCircleNoiseMask;
		}

		public override void OnNodeGUI()
		{
			if (node.samp == null)
			{
				EditorGUILayout.LabelField("Null input noise (Sampler2D)");
				return ;
			}

			EditorGUIUtility.labelWidth = 70;
			EditorGUI.BeginChangeCheck();
			{
				node.blur = EditorGUILayout.Slider("blur", node.blur, 0, 1);
				node.radius = EditorGUILayout.Slider("radius", node.radius, 0, 1);
			}
			if (EditorGUI.EndChangeCheck())
			{
				node.CreateNoiseMask();
				NotifyReload();
			}

			PWGUI.Sampler2DPreview(node.output);
		}

		public override void OnNodeDisable()
		{
			
		}
	}
}