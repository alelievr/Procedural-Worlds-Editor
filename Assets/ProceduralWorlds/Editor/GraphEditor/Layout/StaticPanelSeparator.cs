using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using UnityEditor;

namespace PW.Editor
{
	public class StaticPanelSeparator : PWLayoutSeparator
	{
		public bool		vertical;

		public StaticPanelSeparator(PWLayoutOrientation orientation)
		{
			vertical = orientation == PWLayoutOrientation.Vertical;
		}

		public override Rect Begin()
		{
			if (vertical)
				return EditorGUILayout.BeginVertical(GUILayout.Width(layoutSetting.separatorPosition), GUILayout.ExpandHeight(true));
			else
				return EditorGUILayout.BeginHorizontal(GUILayout.Height(layoutSetting.separatorPosition), GUILayout.ExpandWidth(true));
		}

		public override Rect Split()
		{
			if (vertical)
				EditorGUILayout.EndVertical();
			else
				EditorGUILayout.EndHorizontal();
			
			return new Rect();
		}

		public override void End()
		{
		}
	}

}