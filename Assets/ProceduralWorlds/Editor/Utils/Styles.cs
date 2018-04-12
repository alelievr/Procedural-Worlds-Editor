using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProceduralWorlds.Editor
{
	public static class Styles
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
		
		static GUIStyle _greenLabel;
		public static GUIStyle greenLabel
		{
			get
			{
				if (_greenLabel == null)
				{
					_greenLabel = EditorStyles.whiteLabel;
					_greenLabel.normal.textColor = Color.green;
				}
				return _greenLabel;
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

		static GUIStyle _errorFoldout;
		public static GUIStyle errorFoldout
		{
			get
			{
				if (_errorFoldout == null)
				{
					_errorFoldout = new GUIStyle(EditorStyles.foldout);
					_errorFoldout.normal.textColor = Color.red;
				}

				return _errorFoldout;
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

		static GUIStyle _debugBox;
		public static GUIStyle debugBox
		{
			get
			{
				if (_debugBox == null)
				{
					using (DefaultGUISkin.Get())
					{
						_debugBox = new GUIStyle("box");
					}
				}

				return _debugBox;
			}
		}

		static GUIStyle _box;
		public static GUIStyle box
		{
			get
			{
				if (_box == null)
				{
					using (DefaultGUISkin.Get())
					{
						_box = new GUIStyle("box");
					}
				}

				return _box;
			}
		}

		static GUIStyle _button;
		public static GUIStyle button
		{
			get
			{
				if (_button == null)
				{
					using (DefaultGUISkin.Get())
					{
						_button = new GUIStyle("Button");
					}
				}

				return _button;
			}
		}

		static GUIStyle _pressedButton;
		public static GUIStyle pressedButton
		{
			get
			{
				if (_pressedButton == null)
				{
					using (DefaultGUISkin.Get())
					{
						_pressedButton = new GUIStyle("Button");
						_pressedButton.normal.background = _pressedButton.active.background;
					}
				}

				return _pressedButton;
			}
		}

		static GUIStyle _prefixLabel;
		public static GUIStyle prefixLabel
		{
			get
			{
				if (_prefixLabel == null)
				{
					_prefixLabel = new GUIStyle("PrefixLabel");
					_prefixLabel.normal.textColor = EditorStyles.label.normal.textColor;
					_prefixLabel.active.textColor = EditorStyles.label.normal.textColor;
				}

				return _prefixLabel;
			}
		}

		static Texture2D Colorize(Texture2D original, Color filter)
		{
			Texture2D result = new Texture2D(original.width, original.height, original.format, original.mipmapCount > 1);
			Graphics.CopyTexture(original, result);
			for (int x = 0; x < result.width; x++)
				for (int y = 0; y < result.height; y++)
					result.SetPixel(x, y, result.GetPixel(x, y) * filter);
			result.Apply();
			
			return result;
		}

		static GUIStyle _redButton;
		public static GUIStyle redButton
		{
			get
			{
				if (_redButton == null)
				{
					_redButton = new GUIStyle("Button");
					Texture2D redButtonTexture = Colorize(_redButton.normal.background, Color.red);
					_redButton.normal.background = redButtonTexture;
				}

				return _redButton;
			}
		}

		static GUIStyle _yellowButton;
		public static GUIStyle yellowButton
		{
			get
			{
				if (_yellowButton == null)
				{
					_yellowButton = new GUIStyle("Button");
					Texture2D yellowButtontexture = Colorize(_yellowButton.normal.background, Color.yellow);
					_yellowButton.normal.background = yellowButtontexture;
				}

				return _yellowButton;
			}
		}

		public static GUIStyle ColorizeText(GUIStyle text, Color color)
		{
			text.normal.textColor = color;
			return text;
		}
	}
}