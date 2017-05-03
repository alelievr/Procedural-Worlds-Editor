using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWGUI {

		static Texture2D	ic_color;
		static Texture2D	ic_edit;
		static Texture2D	colorPickerTexture;
		static Texture2D	colorPickerThumb;
		static GUIStyle		colorPickerStyle;

		static Dictionary< string, FieldState< string, int > > textFieldStates = new Dictionary< string, FieldState< string, int > >();
		static Dictionary< string, FieldState< Color, Vector2 > > colorFieldStates = new Dictionary< string, FieldState< Color, Vector2 > >();

		private class FieldState< T, U >
		{
			public bool			active {private set; get;}
			
			private T			valueBeforeActive;
			public U			data;

			public void Active(T value)
			{
				valueBeforeActive = value;
				active = true;
			}

			public T InActive()
			{
				active = false;
				return valueBeforeActive;
			}

			public bool	IsActive()
			{
				return active;
			}
		}

		static PWGUI() {
			ic_color = Resources.Load("ic_color") as Texture2D;
			ic_edit = Resources.Load("ic_edit") as Texture2D;
			colorPickerTexture = Resources.Load("colorPicker") as Texture2D;
			colorPickerStyle = GUI.skin.FindStyle("ColorPicker");
			colorPickerThumb = Resources.Load("colorPickerThumb") as Texture2D;
		}
	
		public static void ColorPicker(Rect iconRect, ref Color c, string controlName, bool displayColorPreview = true, GUIStyle style = null)
		{
			var		e = Event.current;
			
			if (controlName == null)
				Debug.LogWarning("controlname is null for colorField !");

			if (!colorFieldStates.ContainsKey(controlName))
				colorFieldStates[controlName] = new FieldState< Color, Vector2 >();

			var fieldState = colorFieldStates[controlName];

			if (style == null)
				style = PWGUI.colorPickerStyle;

			if (fieldState.active)
			{
				if (e.type == EventType.KeyDown)
				{
					if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
						fieldState.InActive();
					if (e.keyCode == KeyCode.Escape)
						c = fieldState.InActive();
				}
				
				colorPickerStyle = GUI.skin.FindStyle("ColorPicker");
				int colorPickerWidth = 170;
				int	colorPickerHeight = 270;
				Rect colorPickerRect = new Rect(iconRect.position + new Vector2(iconRect.width + 5, 0), new Vector2(colorPickerWidth, colorPickerHeight));
				GUILayout.BeginArea(colorPickerRect, colorPickerStyle);
				{
					Rect localColorPickerRect = new Rect(Vector2.zero, new Vector2(colorPickerWidth, colorPickerHeight));
					GUILayout.Label(colorPickerTexture, GUILayout.Width(150), GUILayout.Height(150));

					Vector2 colorPickerMousePosition = e.mousePosition - new Vector2(colorPickerStyle.padding.left + 1, colorPickerStyle.padding.top + 5);

					if (colorPickerMousePosition.x >= 0 && colorPickerMousePosition.y >= 0 && colorPickerMousePosition.x <= 150 && colorPickerMousePosition.y <= 150)
					{
						if (e.isMouse)
						{
							Debug.Log("pixel coord: " + colorPickerMousePosition);
							Vector2 textureCoord = colorPickerMousePosition * (colorPickerTexture.width / 150f);
							textureCoord.y = colorPickerTexture.height - textureCoord.y;
							c = colorPickerTexture.GetPixel((int)textureCoord.x, (int)textureCoord.y);
							fieldState.data = colorPickerMousePosition + new Vector2(6, 9);
						}
					}
					//TODO: draw color picker thumb

					Rect colorPickerThumbRect = new Rect(fieldState.data, new Vector2(8, 8));
					GUI.DrawTexture(colorPickerThumbRect, colorPickerThumb);

					byte r, g, b, a;
					PWColorPalette.ColorToByte(c, out r, out g, out b, out a);
					EditorGUIUtility.labelWidth = 20;
					r = (byte)EditorGUILayout.IntSlider("R", r, 0, 255);
					g = (byte)EditorGUILayout.IntSlider("G", g, 0, 255);
					b = (byte)EditorGUILayout.IntSlider("B", b, 0, 255);
					a = (byte)EditorGUILayout.IntSlider("A", a, 0, 255);
					c = PWColorPalette.ByteToColor(r, g, b, a);
					EditorGUIUtility.labelWidth = 0;

					EditorGUILayout.Space();

					//hex field
					int hex = PWColorPalette.ColorToHex(c, false); //get color without alpha
					EditorGUIUtility.labelWidth = 80;
					EditorGUI.BeginChangeCheck();
					string hexColor = EditorGUILayout.TextField("Hex color", hex.ToString("X6"));
					if (EditorGUI.EndChangeCheck())
						a = 255;
					EditorGUIUtility.labelWidth = 0;
					Regex reg = new Regex(@"[^A-F0-9 -]");
					hexColor = reg.Replace(hexColor, "");
					hexColor = hexColor.Substring(0, Mathf.Min(hexColor.Length, 6));
					if (hexColor == "")
						hexColor = "0";
					hex = int.Parse(a.ToString("X2") + hexColor, System.Globalization.NumberStyles.HexNumber);
					c = PWColorPalette.HexToColor(hex, false);

					if (e.isMouse && localColorPickerRect.Contains(e.mousePosition))
						e.Use();
				}
				GUILayout.EndArea();
			}
			
			GUI.DrawTexture(iconRect, ic_color);
			if (e.type == EventType.MouseDown && e.button == 0)
			{
				if (iconRect.Contains(e.mousePosition))
					fieldState.Active(c);
				else
					fieldState.InActive();
			}
		}

		public static void ColorPicker(Rect iconRect, ref SerializableColor c, string controlName, bool displayColorPreview = true, GUIStyle colorPickerStyle = null)
		{
			Color color = c;
			ColorPicker(iconRect, ref color, controlName, displayColorPreview, colorPickerStyle);
			c = (SerializableColor)color;
		}

		public static void TextField(Vector2 textPosition, ref string text, string controlName, bool editable = false, GUIStyle textFieldStyle = null)
		{
			Rect	textRect = new Rect(textPosition, Vector2.zero);
			var		e = Event.current;

			if (controlName == null)
				Debug.LogWarning("controlname is null for textField !");

			if (textFieldStyle == null)
				textFieldStyle = GUI.skin.label;

			if (!textFieldStates.ContainsKey(controlName))
				textFieldStates[controlName] = new FieldState< string, int >();

			var fieldState = textFieldStates[controlName];
			
			Vector2 nameSize = GUI.skin.label.CalcSize(new GUIContent(text));
			textRect.size = nameSize;

			if (fieldState.active == true)
			{
				Color oldCursorColor = GUI.skin.settings.cursorColor;
				GUI.skin.settings.cursorColor = Color.white;
				GUI.SetNextControlName(controlName);
				text = GUI.TextField(textRect, text, textFieldStyle);
				GUI.skin.settings.cursorColor = oldCursorColor;
				if (e.isKey)
				{
					if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
						fieldState.InActive();
					else if (e.keyCode == KeyCode.Escape)
						text = fieldState.InActive();
				}
			}
			else
				GUI.Label(textRect, text, textFieldStyle);
				
			if (editable)
			{
				Rect iconRect = new Rect(textRect.position + new Vector2(nameSize.x + 10, -2), new Vector2(17, 17));
				if (e.isMouse && e.button == 0)
				{
					if (iconRect.Contains(Event.current.mousePosition))
					{
						fieldState.Active(text);
						GUI.FocusControl(controlName);
						var te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
						te.SelectAll();
					}
					else
						fieldState.InActive();
				}
				GUI.DrawTexture(iconRect, ic_edit);
			}
		}
	
		public static void Slider(Rect sliderRect, string controlName, ref float value, float min, float max, float pad = 0f, GUIStyle sliderStyle = null)
		{
			if (controlName == null)
				Debug.LogWarning("controlname is null for colorField !");
			
		}
	
		public static void IntSlider(Rect intSliderRect, string controlName, ref int value, int min, int max, int padd = 1, GUIStyle sliderStyle = null)
		{
			float		v = value;
			Slider(intSliderRect, controlName, ref v, min, max, padd);
			value = (int)v;
		}
	}
}