using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	public class StaticPanelSeparator : PWLayoutSeparator
	{
		public bool		vertical;

		Rect			lastRect;

		public StaticPanelSeparator(PWLayoutOrientation orientation)
		{
			vertical = orientation == PWLayoutOrientation.Vertical;
		}

		public override Rect Begin()
		{
			Rect r;
			
			if (vertical)
			{
				r = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
				r.width = layoutSetting.separatorPosition;
			}
			else
			{
				r = EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
				r.height = layoutSetting.separatorPosition;
			}
			
			if (e.type == EventType.Repaint)
				lastRect = r;
			
			return lastRect;
		}

		public override void End()
		{
			if (vertical)
				EditorGUILayout.EndVertical();
			else
				EditorGUILayout.EndHorizontal();
		}
	}

}