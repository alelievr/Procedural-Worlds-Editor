using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace PW
{
	public enum PWGUIStyleType {
		PrefixLabelWidth,
		FieldWidth,
	}

	public class PWGUIStyle {
		
		public int				data;
		public PWGUIStyleType	type;

		public PWGUIStyle(int data, PWGUIStyleType type)
		{
			this.data = data;
			this.type = type;
		}

		public PWGUIStyle SliderLabelWidth(int pixels)
		{
			return new PWGUIStyle(pixels, PWGUIStyleType.PrefixLabelWidth);
		}
	}

	public class PWGUI {

		public static Rect						currentWindowRect;
		public static PWGUISettingsStorage		currentGUISettingsStorage;

		static Texture2D	ic_color;
		static Texture2D	ic_edit;
		static Texture2D	ic_settings;
		static Texture2D	colorPickerTexture;
		static Texture2D	colorPickerThumb;
		static Texture2D	settingsBackgroundTexture;
		static GUIStyle		colorPickerStyle;
		static GUIStyle		centeredLabel;

		[System.NonSerializedAttribute]
		static Dictionary< string, FieldState< string > > textFieldStates = new Dictionary< string, FieldState< string > >();
		[System.NonSerializedAttribute]
		static Dictionary< string, FieldState< Color > > colorFieldStates = new Dictionary< string, FieldState< Color > >();
		[System.NonSerializedAttribute]
		static Dictionary< string, FieldState > sampler2DFieldStates = new Dictionary< string, FieldState >();
		[System.NonSerializedAttribute]
		static Dictionary< string, FieldState > textureFieldStates = new Dictionary< string, FieldState >();

		[System.NonSerializedAttribute]
		static MethodInfo	gradientField;

		[System.SerializableAttribute]
		private class FieldState< T >
		{
			[SerializeField]
			public bool			active {private set; get;}
			
			private T			valueBeforeActive;
			[SerializeField]
			public object[]		datas;

			public FieldState(params object[] datas)
			{
				this.datas = datas;
			}

			public T Active(T value)
			{
				valueBeforeActive = value;
				active = true;
				return value;
			}

			public T InActive()
			{
				active = false;
				return valueBeforeActive;
			}

			public T Invert(T value)
			{
				if (active)
					return InActive();
				else
					return Active(value);
			}
		}

		private class FieldState : FieldState< object > {
			public FieldState(params object[] datas) : base(datas) {}
		}

		static PWGUI() {
			ic_color = Resources.Load("ic_color") as Texture2D;
			ic_edit = Resources.Load("ic_edit") as Texture2D;
			ic_settings = Resources.Load("ic_settings") as Texture2D;
			colorPickerTexture = Resources.Load("colorPicker") as Texture2D;
			colorPickerStyle = GUI.skin.FindStyle("ColorPicker");
			colorPickerThumb = Resources.Load("colorPickerThumb") as Texture2D;
			settingsBackgroundTexture = PWColorPalette.ColorToTexture(PWColorPalette.GetColor("transparentBackground"));
			centeredLabel = new GUIStyle();
			centeredLabel.alignment = TextAnchor.MiddleCenter;
			gradientField = typeof(EditorGUILayout).GetMethod(
				"GradientField",
				BindingFlags.NonPublic | BindingFlags.Static,
				null,
				new Type[] { typeof(GUIContent), typeof(Gradient), typeof(GUILayoutOption[]) },
				null
			);
		}
		
		public static void ColorPicker(string prefix, ref Color c, bool displayColorPreview = true)
		{
			Rect colorFieldRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
			ColorPicker(prefix, colorFieldRect, ref c, displayColorPreview);
		}
		
		public static void ColorPicker(ref Color c, bool displayColorPreview = true)
		{
			Rect colorFieldRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
			ColorPicker(null, colorFieldRect, ref c, displayColorPreview);
		}

		public static void ColorPicker(Rect rect, ref Color c, bool displayColorPreview = true)
		{
			ColorPicker("", rect, ref c, displayColorPreview);
		}

		public static void ColorPicker(Rect rect, ref SerializableColor c, bool displayColorPreview = true)
		{
			Color color = c;
			ColorPicker("", rect, ref color, displayColorPreview);
			c = (SerializableColor)color;
		}

		public static void ColorPicker(string prefix, Rect rect, ref SerializableColor c, bool displayColorPreview = true)
		{
			Color color = c;
			ColorPicker(prefix, rect, ref color, displayColorPreview);
			c = (SerializableColor)color;
		}
	
		public static void ColorPicker(string prefix, Rect rect, ref Color c, bool displayColorPreview = true)
		{
			var		e = Event.current;
			Rect	iconRect = rect;
			int		icColorSize = 18;
			
			string key = c.GetHashCode().ToString();

			if (!colorFieldStates.ContainsKey(key))
				colorFieldStates[key] = new FieldState< Color >(Vector2.zero);

			var fieldState = colorFieldStates[key];

			if (fieldState.active)
			{
				if (e.type == EventType.KeyDown)
				{
					if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
					{
						fieldState.InActive();
						e.Use();
					}
					if (e.keyCode == KeyCode.Escape)
					{
						c = fieldState.InActive();
						e.Use();
					}
				}
				
				//draw the color picker window
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
							Vector2 textureCoord = colorPickerMousePosition * (colorPickerTexture.width / 150f);
							textureCoord.y = colorPickerTexture.height - textureCoord.y;
							c = colorPickerTexture.GetPixel((int)textureCoord.x, (int)textureCoord.y);
							fieldState.datas[0] = colorPickerMousePosition + new Vector2(6, 9);
						}
					}

					Rect colorPickerThumbRect = new Rect((Vector2)fieldState.datas[0], new Vector2(8, 8));
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
			
			//draw the icon
			Rect colorPreviewRect = iconRect;
			if (displayColorPreview)
			{
				int width = (int)rect.width;
				int colorPreviewPadding = 5;
				
				Vector2 prefixSize = Vector2.zero;
				if (!String.IsNullOrEmpty(prefix))
				{
					prefixSize = GUI.skin.label.CalcSize(new GUIContent(prefix));
					prefixSize.x += 5; //padding of 5 pixels
					colorPreviewRect.position += new Vector2(prefixSize.x, 0);
					Rect prefixRect = new Rect(iconRect.position, prefixSize);
					GUI.Label(prefixRect, prefix);
				}
				colorPreviewRect.size = new Vector2(width - icColorSize - prefixSize.x - colorPreviewPadding, 16);
				iconRect.position += new Vector2(colorPreviewRect.width + prefixSize.x + colorPreviewPadding, 0);
				iconRect.size = new Vector2(icColorSize, icColorSize);
				EditorGUIUtility.DrawColorSwatch(colorPreviewRect, c);
			}
			

			//actions if clicked on/outside of the icon
			GUI.DrawTexture(iconRect, ic_color);
			if (e.type == EventType.MouseDown && e.button == 0)
			{
				if (iconRect.Contains(e.mousePosition) || colorPreviewRect.Contains(e.mousePosition))
				{
					fieldState.Active(c);
					e.Use();
				}
				else if (fieldState.active)
				{
					fieldState.InActive();
					e.Use();
				}
			}

			string newKey = c.GetHashCode().ToString();
			if (key != newKey)
			{
				colorFieldStates[newKey] = colorFieldStates[key];
				colorFieldStates.Remove(key);
			}
		}
		
		public static void TextField(string prefix, ref string text, bool editable = false, GUIStyle textStyle = null)
		{
			TextField(prefix, EditorGUILayout.GetControlRect().position, ref text, editable, textStyle);
		}

		public static void TextField(ref string text, bool editable = false, GUIStyle textStyle = null)
		{
			TextField(null, EditorGUILayout.GetControlRect().position, ref text, editable, textStyle);
		}

		public static void TextField(Vector2 position, ref string text, bool editable = false, GUIStyle textStyle = null)
		{
			TextField(null, position, ref text, editable, textStyle);
		}

		public static void TextField(string prefix, Vector2 textPosition, ref string text, bool editable = false, GUIStyle textFieldStyle = null)
		{
			Rect	textRect = new Rect(textPosition, Vector2.zero);
			var		e = Event.current;

			string key = text.GetHashCode().ToString();

			if (textFieldStyle == null)
				textFieldStyle = GUI.skin.label;

			if (!textFieldStates.ContainsKey(key))
				textFieldStates[key] = new FieldState< string >();

			var fieldState = textFieldStates[key];
			
			Vector2 nameSize = textFieldStyle.CalcSize(new GUIContent(text));
			textRect.size = nameSize;

			if (!String.IsNullOrEmpty(prefix))
			{
				Vector2 prefixSize = textFieldStyle.CalcSize(new GUIContent(prefix));
				Rect prefixRect = textRect;

				textRect.position += new Vector2(prefixSize.x, 0);
				prefixRect.size = prefixSize;
				GUI.Label(prefixRect, prefix);
			}
			
			Rect iconRect = new Rect(textRect.position + new Vector2(nameSize.x + 10, 0), new Vector2(17, 17));
			bool editClickIn = (editable && e.type == EventType.MouseDown && e.button == 0 && iconRect.Contains(e.mousePosition));

			if (editClickIn)
				fieldState.Invert(text);
			
			if (editable)
			{
				GUI.color = (fieldState.active) ? PWColorPalette.GetColor("selected") : Color.white;
				GUI.DrawTexture(iconRect, ic_edit);
				GUI.color = Color.white;
			}

			if (fieldState.active == true)
			{
				Color oldCursorColor = GUI.skin.settings.cursorColor;
				GUI.skin.settings.cursorColor = Color.white;
				GUI.SetNextControlName(key);
				text = GUI.TextField(textRect, text, textFieldStyle);
				GUI.skin.settings.cursorColor = oldCursorColor;
				if (e.isKey && fieldState.active)
				{
					if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
					{
						fieldState.InActive();
						e.Use();
					}
					else if (e.keyCode == KeyCode.Escape)
					{
						text = fieldState.InActive();
						e.Use();
					}
				}
			}
			else
				GUI.Label(textRect, text, textFieldStyle);			
			
			bool editClickOut = (editable && e.type == EventType.MouseDown && e.button == 0 && !iconRect.Contains(e.mousePosition));

			if (editClickOut && fieldState.active)
			{
				fieldState.InActive();
				e.Use();
			}

			string newKey = text.GetHashCode().ToString();
			if (newKey != key)
			{
				textFieldStates[newKey] = textFieldStates[key];
				textFieldStates.Remove(key);
			}

			if (editClickIn)
			{
				GUI.FocusControl(key);
				var te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
				te.SelectAll();
				e.Use();
			}
		}
		
		public static void Slider(ref float value, float min, float max, float step = 0.01f, params PWGUIStyle[] styles)
		{
			Slider(null, ref value, ref min, ref max, step, false, false, styles);
		}

		public static void Slider(string name, ref float value, float min, float max, float step = 0.01f, params PWGUIStyle[] styles)
		{
			Slider(name, ref value, ref min, ref max, step, false, false, styles);
		}
	
		public static void Slider(string name, ref float value, ref float min, ref float max, float step = 0.01f, bool editableMin = true, bool editableMax = true, params PWGUIStyle[] styles)
		{
			int		sliderLabelWidth = 30;

			foreach (var style in styles)
				if (style.type == PWGUIStyleType.PrefixLabelWidth)
					sliderLabelWidth = style.data;

			if (name == null)
				name = "";

			EditorGUILayout.BeginHorizontal();
			{
				EditorGUI.BeginDisabledGroup(!editableMin);
					min = EditorGUILayout.FloatField(min, GUILayout.Width(sliderLabelWidth));
				EditorGUI.EndDisabledGroup();
				
				if (step != 0)
				{
					float m = 1 / step;
					value = Mathf.Round(GUILayout.HorizontalSlider(value, min, max) * m) / m;
				}
				else
					value = GUILayout.HorizontalSlider(value, min, max);

				EditorGUI.BeginDisabledGroup(!editableMax);
					max = EditorGUILayout.FloatField(max, GUILayout.Width(sliderLabelWidth));
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndHorizontal();
			
			GUILayout.Space(-4);
			GUILayout.Label(name + value.ToString(), centeredLabel);
		}
		
		public static void IntSlider(ref int value, int min, int max, int step = 1, params PWGUIStyle[] styles)
		{
			IntSlider(null, ref value, ref min, ref max, step, false, false, styles);
		}

		public static void IntSlider(string name, ref int value, int min, int max, int step = 1, params PWGUIStyle[] styles)
		{
			IntSlider(name, ref value, ref min, ref max, step, false, false, styles);
		}
	
		public static void IntSlider(string name, ref int value, ref int min, ref int max, int step = 1, bool editableMin = true, bool editableMax = true, params PWGUIStyle[] styles)
		{
			float		v = value;
			float		m_min = min;
			float		m_max = max;
			Slider(name, ref v, ref m_min, ref m_max, step, editableMin, editableMax, styles);
			value = (int)v;
			min = (int)m_min;
			max = (int)m_max;
		}

		public static void TexturePreview(Texture tex, bool settings = true)
		{
			Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(0));
			previewRect.size = (currentWindowRect.width - 20 - 10) * Vector2.one;
			TexturePreview(previewRect, tex, settings);
			GUILayout.Space(previewRect.width);
		}

		public static void TexturePreview(Rect previewRect, Texture tex, bool settings = true)
		{
			var e = Event.current;
			string key = tex.GetHashCode().ToString();

			if (!textureFieldStates.ContainsKey(key))
			{
				FilterMode	mode = FilterMode.Point;
				ScaleMode	scaleMode = ScaleMode.ScaleToFit;
				float		scaleAspect = 0;
				Material	mat = null;
				var state = new FieldState(mode, scaleMode, scaleAspect, mat);
				textureFieldStates[key] = state;
			}

			var fieldState = textureFieldStates[key];

			EditorGUI.DrawPreviewTexture(previewRect, tex, (Material)fieldState.datas[3], (ScaleMode)fieldState.datas[1], (float)fieldState.datas[2]);

			if (!settings)
				return ;

			if (fieldState.active)
			{
				if (e.type == EventType.KeyDown)
					if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Escape)
					{
						fieldState.InActive();
						e.Use();
					}
				
				Rect settingsRect = EditorGUILayout.BeginVertical(GUILayout.Width(previewRect.width - 20));
				{
					FilterMode	newMode;

					GUI.DrawTexture(settingsRect, settingsBackgroundTexture);

					EditorGUIUtility.labelWidth = 80;
					EditorGUI.BeginChangeCheck();
						newMode = (FilterMode)EditorGUILayout.EnumPopup("filter mode", tex.filterMode);
					if (EditorGUI.EndChangeCheck())
						tex.filterMode = newMode;
					fieldState.datas[1] = EditorGUILayout.EnumPopup("scale mode", (ScaleMode)fieldState.datas[1]);
					fieldState.datas[2] = EditorGUILayout.FloatField("scale aspect", (float)fieldState.datas[2]);
					fieldState.datas[3] = EditorGUILayout.ObjectField("material", (Object)fieldState.datas[3], typeof(Material), false);
					EditorGUIUtility.labelWidth = 0;
				}
				EditorGUILayout.EndVertical();
				GUILayout.Space(-74); //settingsRect height
			}

			int		icSettingsSize = 16;
			Rect	icSettingsRect = new Rect(previewRect.x + previewRect.width - icSettingsSize, previewRect.y, icSettingsSize, icSettingsSize);
			GUI.DrawTexture(icSettingsRect, ic_settings);
			if (e.type == EventType.MouseDown && e.button == 0)
			{
				if (icSettingsRect.Contains(e.mousePosition))
				{
					fieldState.Invert(0);
					e.Use();
				}
				else if (fieldState.active)
				{
					fieldState.InActive();
					e.Use();
				}
			}
		}
		
		public static void Sampler2DPreview(Sampler2D samp, bool update, bool settings = true)
		{
			Sampler2DPreview(null, samp, update, settings);
		}
		
		public static void Sampler2DPreview(string prefix, Sampler2D samp, bool update, bool settings = true)
		{
			int previewSize = (int)currentWindowRect.width - 20 - 20; //padding + texture margin
			var e = Event.current;

			string key = samp.GetHashCode().ToString();

			if (!String.IsNullOrEmpty(prefix))
				EditorGUILayout.LabelField(prefix);

			if (!sampler2DFieldStates.ContainsKey(key) || sampler2DFieldStates[key].datas[0] == null)
			{
				Texture2D	data;
				FilterMode	mode = FilterMode.Point;
				Gradient	grad = PWUtils.CreateGradient(
					new KeyValuePair< float, Color >(0, Color.black),
					new KeyValuePair< float, Color >(1, Color.white)
				);

				data = new Texture2D(previewSize, previewSize, TextureFormat.RGBA32, false);
				data.filterMode = FilterMode.Point;
				var state = new FieldState(data, grad, mode);
				sampler2DFieldStates[key] = state;
			}

			var fieldState = sampler2DFieldStates[key];

			Texture2D	tex = fieldState.datas[0] as Texture2D;
			Gradient	gradient = fieldState.datas[1] as Gradient;
			

			if (samp.size != tex.width)
				tex.Resize(samp.size, samp.size, TextureFormat.RGBA32, false);

			Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(0));
			
			TexturePreview(tex, false);
			
			//draw the settings window
			if (settings && fieldState.active)
			{
				EditorGUILayout.BeginVertical();
				{
					EditorGUI.BeginChangeCheck();
					fieldState.datas[2] = EditorGUILayout.EnumPopup((FilterMode)fieldState.datas[2]);
					if (EditorGUI.EndChangeCheck())
						tex.filterMode = (FilterMode)fieldState.datas[2];
					gradient = (Gradient)gradientField.Invoke(null, new object[] {new GUIContent(""), fieldState.datas[1], null});
					//TODO: check if the gradient have changed
					update = true;
					fieldState.datas[1] = gradient;
				}
				EditorGUILayout.EndVertical();
				
				if (e.type == EventType.KeyDown && fieldState.active)
				{
					if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Escape)
					{
						fieldState.InActive();
						e.Use();
					}
				}
			}
			
			//draw the setting icon and manage his events
			int icSettingsSize = 16;
			int	icSettingsPadding = 4;
			Rect icSettingsRect = new Rect(previewRect.x + previewRect.width - icSettingsSize - icSettingsPadding, previewRect.y + icSettingsPadding, icSettingsSize, icSettingsSize);

			GUI.DrawTexture(icSettingsRect, ic_settings);
			if (e.type == EventType.MouseDown && e.button == 0)
			{
				if (icSettingsRect.Contains(e.mousePosition))
				{
					fieldState.Invert(null);
					e.Use();
				}
				else if (fieldState.active)
				{
					fieldState.InActive();
					e.Use();
				}
			}

			//update the texture with the gradient
			if (update)
			{
				samp.Foreach((x, y, val) => {
					tex.SetPixel(x, y, gradient.Evaluate(Mathf.Clamp01(val)));
				});
				tex.Apply();
			}
			
			string newKey = samp.GetHashCode().ToString();
			if (key != newKey)
			{
				sampler2DFieldStates[newKey] = sampler2DFieldStates[key];
				sampler2DFieldStates.Remove(key);
			}
		}
		
		public static void ObjectPreview(object obj, bool update)
		{
			ObjectPreview(null, obj, update);
		}

		public static void ObjectPreview(string name, object obj, bool update)
		{
			Type objType = obj.GetType();

			if (objType == typeof(Sampler2D))
				Sampler2DPreview(name, obj as Sampler2D, update);
			else if (obj.GetType().IsInstanceOfType(typeof(Object)))
			{
				//unity object preview
			}
			else
				Debug.LogWarning("can't preview the object of type: " + obj.GetType());
		}
	}
}