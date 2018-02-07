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

		static GUIStyle _header;
		public static GUIStyle header
		{
			get
			{
				if (_header == null)
				{
					_header = new GUIStyle(EditorStyles.boldLabel);
					_header.fontSize = 20;
				}

				return _header;
			}
		}
		
		static GUIStyle _centeredLabel;
		public static GUIStyle centeredLabel
		{
			get
			{
				if (_centeredLabel == null)
				{
					_centeredLabel = new GUIStyle(EditorStyles.label);
					_centeredLabel.alignment = TextAnchor.MiddleCenter;
				}

				return _centeredLabel;
			}
		}

		public static GUIStyle ColorizeText(GUIStyle text, Color color)
		{
			text.normal.textColor = color;
			return text;
		}
	}
}