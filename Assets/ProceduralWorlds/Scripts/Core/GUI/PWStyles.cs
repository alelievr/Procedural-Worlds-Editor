using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW.Core
{
	public static class PWStyles
	{
		static GUIStyle _redLabel;
		public static GUIStyle redLabel
		{
			get
			{
				if (_redLabel == null)
				{
					_redLabel = EditorStyles.whiteLabel;
					_redLabel.normal.textColor = Color.red;
				}
				return _redLabel;
			}
		}

		public static GUIStyle errorLabel { get { return redLabel; } }
	}
}