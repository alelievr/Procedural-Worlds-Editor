using System.Collections;
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

		static Dictionary< string, bool >	selectedTextFields = new Dictionary< string, bool >();
		static Dictionary< string, bool >	openedColorFields = new Dictionary< string, bool >();

		static PWGUI() {
			ic_color = Resources.Load("ic_color") as Texture2D;
			ic_edit = Resources.Load("ic_edit") as Texture2D;
			colorPickerTexture = Resources.Load("colorPicker") as Texture2D;
			colorPickerStyle = GUI.skin.FindStyle("ColorPicker");
		}
	
		public static void ColorPicker(Rect iconRect, ref Color c, string controlName, bool displayColorPreview = true, GUIStyle colorPickerStyle = null)
		{
			//TODO: draw the colorPicker field.
			GUI.DrawTexture(iconRect, ic_color);
			if (Event.current.isMouse)
			{
				if (iconRect.Contains(Event.current.mousePosition))
					openedColorFields[controlName] = true;
				else
					openedColorFields[controlName] = false;
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

			if (textFieldStyle == null)
				textFieldStyle = GUI.skin.label;
			
			Vector2 nameSize = GUI.skin.label.CalcSize(new GUIContent(text));
			textRect.size = nameSize;
			if (editable)
			{
				GUI.SetNextControlName(controlName);
				text = GUI.TextField(textRect, text, textFieldStyle);
			}
			else
				GUI.Label(textRect, text, textFieldStyle);
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