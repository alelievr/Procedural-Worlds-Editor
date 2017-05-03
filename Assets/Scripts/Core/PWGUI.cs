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
		static GUIStyle		colorPickerStyle;

		static Dictionary< string, FieldState< string > > textFieldStates = new Dictionary< string, FieldState< string > >();
		static Dictionary< string, FieldState< Color > > colorFieldStates = new Dictionary< string, FieldState< Color > >();

		private class FieldState< T >
		{
			public bool			active {private set; get;}
			
			private T			valueBeforeActive;

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
		}
	
		public static void ColorPicker(Rect iconRect, ref Color c, string controlName, bool displayColorPreview = true, GUIStyle style = null)
		{
			var		e = Event.current;

			if (!colorFieldStates.ContainsKey(controlName))
				colorFieldStates[controlName] = new FieldState< Color >();

			if (style == null)
				style = PWGUI.colorPickerStyle;

			if (colorFieldStates[controlName].active)
			{
				if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
					colorFieldStates[controlName].InActive();
				if (e.keyCode == KeyCode.Escape)
					c = colorFieldStates[controlName].InActive();
				
				colorPickerStyle = GUI.skin.FindStyle("ColorPicker");
				int colorPickerWidth = 170;
				int	colorPickerHeight = 220;
				Rect colorPickerRect = new Rect(iconRect.position + new Vector2(iconRect.width + 5, 0), new Vector2(colorPickerWidth, colorPickerHeight));
				GUILayout.BeginArea(colorPickerRect, colorPickerStyle);
				{
					Rect localColorPickerRect = new Rect(Vector2.zero, new Vector2(colorPickerWidth, colorPickerHeight));
					GUILayout.Label(colorPickerTexture, GUILayout.Width(150), GUILayout.Height(150));

					Vector2 mousePosition = e.mousePosition + new Vector2(colorPickerStyle.padding.left, colorPickerStyle.padding.top);

					//TODO: draw color picker thumb

					GUILayout.BeginHorizontal();
					byte r, g, b, a;
					PWColorPalette.ColorToByte(c, out r, out b, out g, out a);
					r = (byte)EditorGUILayout.IntField(r);
					g = (byte)EditorGUILayout.IntField(g);
					b = (byte)EditorGUILayout.IntField(b);
					GUILayout.EndHorizontal();

					//hex field
					int hex = PWColorPalette.ColorToHex(c);
					string hexColor = GUILayout.TextField(hex.ToString("X"));
					Regex reg = new Regex(@"[^A-F0-9 -]");
					hexColor = reg.Replace(hexColor, "");
					hexColor = hexColor.Substring(0, Mathf.Min(hexColor.Length, 8));
					if (hexColor == "")
						hexColor = "0";
					hex = int.Parse(hexColor, System.Globalization.NumberStyles.HexNumber);
					c = PWColorPalette.HexToColor(hex);

					if (e.isMouse && localColorPickerRect.Contains(e.mousePosition))
						e.Use();
				}
				GUILayout.EndArea();
			}
			
			GUI.DrawTexture(iconRect, ic_color);
			if (e.isMouse && e.button == 0)
			{
				if (iconRect.Contains(e.mousePosition))
					colorFieldStates[controlName].Active(c);
				else
					colorFieldStates[controlName].InActive();
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

			if (textFieldStyle == null)
				textFieldStyle = GUI.skin.label;

			if (!textFieldStates.ContainsKey(controlName))
				textFieldStates[controlName] = new FieldState< string >();
			
			Vector2 nameSize = GUI.skin.label.CalcSize(new GUIContent(text));
			textRect.size = nameSize;

			if (textFieldStates[controlName].active == true)
			{
				Color oldCursorColor = GUI.skin.settings.cursorColor;
				GUI.skin.settings.cursorColor = Color.white;
				GUI.SetNextControlName(controlName);
				text = GUI.TextField(textRect, text, textFieldStyle);
				GUI.skin.settings.cursorColor = oldCursorColor;
				if (e.isKey)
				{
					if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
						textFieldStates[controlName].InActive();
					else if (e.keyCode == KeyCode.Escape)
						text = textFieldStates[controlName].InActive();
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
						textFieldStates[controlName].Active(text);
						GUI.FocusControl(controlName);
						var te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
						te.SelectAll();
					}
					else
						textFieldStates[controlName].InActive();
				}
				GUI.DrawTexture(iconRect, ic_edit);
			}
		}
	
		public static void Slider(Rect sliderRect, ref float value, float min, float max, float pad = 0f, GUIStyle sliderStyle = null)
		{
			
		}
	
		public static void IntSlider(Rect intSliderRect, ref int value, int min, int max, int padd = 1, GUIStyle sliderStyle = null)
		{
			float		v = value;
			Slider(intSliderRect, ref v, min, max, padd);
			value = (int)v;
		}
	
	}
}