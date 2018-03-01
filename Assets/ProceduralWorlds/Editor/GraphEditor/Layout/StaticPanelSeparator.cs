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

		public override void Initialize(PWGraphEditor graphEditor)
		{

		}

		public override Rect Begin()
		{
			if (vertical)
				return EditorGUILayout.BeginVertical();
			else
				return EditorGUILayout.BeginHorizontal();
		}

		public override Rect Split()
		{
			return new Rect();
		}

		public override Rect End()
		{
			if (vertical)
				EditorGUILayout.EndVertical();
			else
				EditorGUILayout.EndHorizontal();
			return new Rect();
		}
	}

}