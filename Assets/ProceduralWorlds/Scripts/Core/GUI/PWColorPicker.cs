using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace PW.Core
{
	public class PWColorPicker : PWPopup
	{
		Texture2D		colorPickerTexture;
		Texture2D		colorPickerThumb;


		public static Color		currentColor;
		public static Vector2	thumbPosition;

		public static void OpenPopup(Color color, Vector2 thumbPosition)
		{
			PWPopup.OpenPopup< PWColorPicker >();

			PWColorPicker.currentColor = color;
			PWColorPicker.thumbPosition = thumbPosition;
		}

		protected override void GUIStart()
		{

		}

		protected override void OnGUIEnable()
		{
			colorPickerTexture = Resources.Load("colorPicker") as Texture2D;
			colorPickerThumb = Resources.Load("colorPickerThumb") as Texture2D;
		}

		void DrawRainbowPicker()
		{
			GUIStyle colorPickerStyle = GUI.skin.FindStyle("ColorPicker");
			GUILayout.Label(colorPickerTexture, GUILayout.Width(windowSize.x - 10), GUILayout.Height(windowSize.x - 10));

			Rect colorPickerRect = EditorGUILayout.GetControlRect(false, windowSize.x - 10);

			Vector2 colorPickerMousePosition = e.mousePosition;

			//TODO: better bounds
			if (colorPickerMousePosition.x >= 0 && colorPickerMousePosition.y >= 0 && colorPickerMousePosition.x <= windowSize.x && colorPickerMousePosition.y <= windowSize.x)
			{
				if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
				{
					Vector2 textureCoord = colorPickerMousePosition * (colorPickerTexture.width / windowSize.x);
					textureCoord.y = colorPickerTexture.height - textureCoord.y;
					currentColor = colorPickerTexture.GetPixel((int)textureCoord.x, (int)textureCoord.y);
					thumbPosition = colorPickerMousePosition;
					GUI.changed = true;
				}
			}

			Rect colorPickerThumbRect = new Rect(thumbPosition, new Vector2(8, 8));
			GUI.DrawTexture(colorPickerThumbRect, colorPickerThumb);
		}

		void DrawRGBComponents()
		{
			byte r, g, b, a;
			PWColor.ColorToByte(currentColor, out r, out g, out b, out a);
			EditorGUIUtility.labelWidth = 20;
			r = (byte)EditorGUILayout.IntSlider("R", r, 0, 255);
			g = (byte)EditorGUILayout.IntSlider("G", g, 0, 255);
			b = (byte)EditorGUILayout.IntSlider("B", b, 0, 255);
			a = (byte)EditorGUILayout.IntSlider("A", a, 0, 255);
			currentColor = (SerializableColor)PWColor.ByteToColor(r, g, b, a);
			EditorGUIUtility.labelWidth = 0;
		}

		void DrawHexComponents()
		{
			byte a = (byte)(int)(currentColor.a * 255);
			int hex = PWColor.ColorToHex(currentColor, false); //get color without alpha
			
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
			currentColor = (SerializableColor)PWColor.HexToColor(hex, false);
		}

		protected override void GUIUpdate()
		{
			EditorGUI.BeginChangeCheck();
			{
				DrawRainbowPicker();
	
				DrawRGBComponents();
	
				EditorGUILayout.Space();
	
				DrawHexComponents();
			}
			if (EditorGUI.EndChangeCheck())
			{
				SendUpdate("ColorPickerUpdate");
				Repaint();
			}
		}

	}
}