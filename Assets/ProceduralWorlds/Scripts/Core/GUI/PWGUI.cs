using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using UnityEditor;
using PW.Biomator;
using UnityEditor.AnimatedValues;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace PW.Core
{
	public enum PWGUIStyleType
	{
		PrefixLabelWidth,
		FieldWidth,
	}

	public class PWGUIStyle
	{
		
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
	
	[System.Serializable]
	public class PWGUIManager
	{

		public static Rect	editorWindowRect;

		Rect				currentWindowRect;

		static Texture2D	icColor;
		static Texture2D	icEdit;
		static Texture2D	icSettingsOutline;
		static GUIStyle		centeredLabel;

		[SerializeField]
		List< PWGUISettings >	settingsStorage = new List< PWGUISettings >();
		int						currentSettingCount = 0;

		PWNode				attachedNode;
		
		[System.NonSerializedAttribute]
		static MethodInfo	gradientField;

		public void SetNode(PWNode node)
		{
			if (node != null)
			{
				attachedNode = node;
				node.OnPostReload += ReloadTextures;
				node.OnPostProcess += ReloadTextures;
				EditorApplication.playModeStateChanged += PlayModeChangedCallback;
			}
		}

		public PWGUIManager() {}

		~PWGUIManager()
		{
			if (attachedNode != null)
			{
				attachedNode.OnPostReload -= ReloadTextures;
				attachedNode.OnPostProcess -= ReloadTextures;
				EditorApplication.playModeStateChanged -= PlayModeChangedCallback;
			}
		}

		void PlayModeChangedCallback(PlayModeStateChange mode)
		{
			if (mode == PlayModeStateChange.EnteredEditMode)
				ReloadTextures();
		}

		void ReloadTextures(PWNode node) { ReloadTextures(); }

		void ReloadTextures()
		{
			if (settingsStorage == null)
				return ;
			
			foreach (var setting in settingsStorage)
			{
				switch (setting.fieldType)
				{
					case PWGUIFieldType.Sampler2DPreview:
						UpdateSampler2D(setting);
						break ;
					case PWGUIFieldType.BiomeMapPreview:
						UpdateBiomeMap2D(setting);
						break ;
					default:
						break ;
				}
				if (setting.texture != null)
				{
					//TODO: call the recalculate function specific to field type
				}
			}
		}
		
	#region Color field

		public void ColorPicker(string prefix, ref Color c, bool displayColorPreview = true, bool previewOnIcon = false)
		{
			Rect colorFieldRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
			ColorPicker(prefix, colorFieldRect, ref c, displayColorPreview, previewOnIcon);
		}
		
		public void ColorPicker(ref Color c, bool displayColorPreview = true, bool previewOnIcon = false)
		{
			Rect colorFieldRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
			ColorPicker("", colorFieldRect, ref c, displayColorPreview, previewOnIcon);
		}

		public void ColorPicker(Rect rect, ref Color c, bool displayColorPreview = true, bool previewOnIcon = false)
		{
			ColorPicker("", rect, ref c, displayColorPreview, previewOnIcon);
		}

		public void ColorPicker(Rect rect, ref SerializableColor c, bool displayColorPreview = true, bool previewOnIcon = false)
		{
			Color color = c;
			ColorPicker("", rect, ref color, displayColorPreview, previewOnIcon);
			c = (SerializableColor)color;
		}
		
		public void ColorPicker(string prefix, Rect rect, ref SerializableColor c, bool displayColorPreview = true, bool previewOnIcon = false)
		{
			ColorPicker(new GUIContent(prefix), rect, ref c, displayColorPreview, previewOnIcon);
		}

		public void ColorPicker(GUIContent prefix, Rect rect, ref SerializableColor c, bool displayColorPreview = true, bool previewOnIcon = false)
		{
			Color color = c;
			ColorPicker(prefix, rect, ref color, displayColorPreview, previewOnIcon);
			c = (SerializableColor)color;
		}
		
		public void ColorPicker(string prefix, Rect rect, ref Color color, bool displayColorPreview = true, bool previewOnIcon = false)
		{
			ColorPicker(new GUIContent(prefix), rect, ref color, displayColorPreview, previewOnIcon);
		}
	
		public void ColorPicker(GUIContent prefix, Rect rect, ref Color color, bool displayColorPreview = true, bool previewOnIcon = false)
		{
			var		e = Event.current;
			Rect	iconRect = rect;
			int		icColorSize = 18;
			Color	localColor = color;

			var fieldSettings = GetGUISettingData(PWGUIFieldType.Color, () => {
				PWGUISettings colorSettings = new PWGUISettings();

				colorSettings.c = (SerializableColor)localColor;

				return colorSettings;
			});

			// if (e.type == EventType.ExecuteCommand)
				// Debug.Log("cmd: " + e.commandName + ", hash: " + fieldSettings.GetHashCode() + ", controlId: " + PWColorPicker.controlId);
			if (e.type == EventType.ExecuteCommand && e.commandName == "ColorPickerUpdate")
				if (fieldSettings.GetHashCode() == PWColorPicker.controlId)
				{
					fieldSettings.c = (SerializableColor)PWColorPicker.currentColor;
					fieldSettings.thumbPosition = PWColorPicker.thumbPosition;
					GUI.changed = true;
				}
			
			color = fieldSettings.c;
			
			//draw the icon
			Rect colorPreviewRect = iconRect;
			if (displayColorPreview)
			{
				int width = (int)rect.width;
				int colorPreviewPadding = 5;
				
				Vector2 prefixSize = Vector2.zero;
				if (prefix != null && !String.IsNullOrEmpty(prefix.text))
				{
					prefixSize = GUI.skin.label.CalcSize(prefix);
					prefixSize.x += 5; //padding of 5 pixels
					colorPreviewRect.position += new Vector2(prefixSize.x, 0);
					Rect prefixRect = new Rect(iconRect.position, prefixSize);
					GUI.Label(prefixRect, prefix);
				}
				colorPreviewRect.size = new Vector2(width - icColorSize - prefixSize.x - colorPreviewPadding, 16);
				iconRect.position += new Vector2(colorPreviewRect.width + prefixSize.x + colorPreviewPadding, 0);
				iconRect.size = new Vector2(icColorSize, icColorSize);
				EditorGUIUtility.DrawColorSwatch(colorPreviewRect, color);
			}
			
			//actions if clicked on/outside of the icon
			if (previewOnIcon)
				GUI.color = color;
			GUI.DrawTexture(iconRect, icColor);
			GUI.color = Color.white;
			if (e.type == EventType.MouseDown && e.button == 0)
			{
				if (iconRect.Contains(e.mousePosition) || colorPreviewRect.Contains(e.mousePosition))
				{
					PWColorPicker.OpenPopup(color, fieldSettings);
					e.Use();
				}
			}
		}
	
	#endregion

	#region Gradient field

	public Gradient GradientField(Gradient gradient)
	{
		return GradientField((GUIContent)null, gradient);
	}

	public Gradient GradientField(string label, Gradient gradient)
	{
		return GradientField(new GUIContent(label), gradient);
	}

	public Gradient GradientField(GUIContent content, Gradient gradient)
	{
		if (content != null && content.text != null)
			EditorGUILayout.PrefixLabel(content);

		if (gradientField == null)
		{
			gradientField = typeof(EditorGUILayout).GetMethod(
				"GradientField",
				BindingFlags.NonPublic | BindingFlags.Static,
				null,
				new Type[] { typeof(string), typeof(Gradient), typeof(GUILayoutOption[]) },
				null
			);
		}

		gradient = (Gradient)gradientField.Invoke(null, new object[] {"", gradient, null});

		return gradient;
	}

	#endregion

	#region Text field
		
		public void TextField(string prefix, ref string text, bool editable = false, GUIStyle textStyle = null)
		{
			TextField(prefix, EditorGUILayout.GetControlRect().position, ref text, editable, textStyle);
		}

		public void TextField(ref string text, bool editable = false, GUIStyle textStyle = null)
		{
			TextField(null, EditorGUILayout.GetControlRect().position, ref text, editable, textStyle);
		}

		public void TextField(Vector2 position, ref string text, bool editable = false, GUIStyle textStyle = null)
		{
			TextField(null, position, ref text, editable, textStyle);
		}

		public void TextField(string prefix, Vector2 textPosition, ref string text, bool editable = false, GUIStyle textFieldStyle = null)
		{
			Rect	textRect = new Rect(textPosition, Vector2.zero);
			var		e = Event.current;

			string	controlName = "textfield-" + text.GetHashCode().ToString();

			var fieldSettings = GetGUISettingData(PWGUIFieldType.Text, () => {
				return new PWGUISettings();
			});
			
			Vector2 nameSize = textFieldStyle.CalcSize(new GUIContent(text + " ")); //add a space for the edit icon beside the text
			textRect.size = nameSize;

			if (!String.IsNullOrEmpty(prefix))
			{
				Vector2 prefixSize = textFieldStyle.CalcSize(new GUIContent(prefix));
				Rect prefixRect = textRect;

				textRect.position += new Vector2(prefixSize.x, 0);
				prefixRect.size = prefixSize;
				GUI.Label(prefixRect, prefix);
			}
			
			Rect iconRect = new Rect(textRect.position + new Vector2(nameSize.x, 0), new Vector2(17, 17));
			bool editClickIn = (editable && e.type == EventType.MouseDown && e.button == 0 && iconRect.Contains(e.mousePosition));
			bool doubleClickText = (textRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.clickCount == 2);

			if (editClickIn)
				fieldSettings.editing = !fieldSettings.editing;
			if (doubleClickText)
				fieldSettings.editing = true;
			
			if (editable)
			{
				GUI.color = (fieldSettings.editing) ? PWColorTheme.selectedColor : Color.white;
				GUI.DrawTexture(iconRect, icEdit);
				GUI.color = Color.white;
			}

			if (fieldSettings.editing)
			{
				Color oldCursorColor = GUI.skin.settings.cursorColor;
				GUI.skin.settings.cursorColor = Color.white;
				GUI.SetNextControlName(controlName);
				text = GUI.TextField(textRect, text, textFieldStyle);
				GUI.skin.settings.cursorColor = oldCursorColor;
				if (e.isKey && fieldSettings.editing)
				{
					if (e.keyCode == KeyCode.Escape || e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
					{
						fieldSettings.editing = false;
						e.Use();
					}
				}
			}
			else
				GUI.Label(textRect, text, textFieldStyle);			
			
			bool editClickOut = (editable && e.rawType == EventType.MouseDown && e.button == 0 && !iconRect.Contains(e.mousePosition));

			if (editClickOut && fieldSettings.editing)
			{
				fieldSettings.editing = false;
				e.Use();
			}

			if ((editClickIn || doubleClickText) && fieldSettings.editing)
			{
				GUI.FocusControl(controlName);
				var te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
				te.SelectAll();
				e.Use();
			}
		}

	#endregion

	#region Slider and IntSlider field

		public float Slider(float value, ref float min, ref float max, float step = 0.01f, bool editableMin = true, bool editableMax = true, params PWGUIStyle[] styles)
		{
			return Slider("", value, ref min, ref max, step, editableMin, editableMax, styles);
		}
		
		public float Slider(float value, float min, float max, float step = 0.01f, params PWGUIStyle[] styles)
		{
			return Slider("", value, ref min, ref max, step, false, false, styles);
		}
		
		public float Slider(string name, float value, float min, float max, float step = 0.01f, params PWGUIStyle[] styles)
		{
			return Slider(new GUIContent(name), value, min, max, step, styles);
		}
		
		public float Slider(GUIContent name, float value, float min, float max, float step = 0.01f, params PWGUIStyle[] styles)
		{
			return Slider(name, value, ref min, ref max, step, false, false, styles);
		}
		
		public float Slider(string name, float value, ref float min, ref float max, float step = 0.01f, bool editableMin = true, bool editableMax = true, params PWGUIStyle[] styles)
		{
			return Slider(new GUIContent(name), value, ref min, ref max, step, editableMin, editableMax, styles);
		}
	
		public float Slider(GUIContent name, float value, ref float min, ref float max, float step = 0.01f, bool editableMin = true, bool editableMax = true, params PWGUIStyle[] styles)
		{
			return Slider(name, value, ref min, ref max, step, editableMin, editableMax, false, styles);
		}
		
		float Slider(GUIContent name, float value, ref float min, ref float max, float step = 0.01f, bool editableMin = true, bool editableMax = true, bool intMode = false, params PWGUIStyle[] styles)
		{
			int		sliderLabelWidth = 30;
			var		e = Event.current;

			foreach (var style in styles)
				if (style.type == PWGUIStyleType.PrefixLabelWidth)
					sliderLabelWidth = style.data;

			if (name == null)
				name = new GUIContent();

			var fieldSettings = GetGUISettingData((intMode) ? PWGUIFieldType.IntSlider : PWGUIFieldType.Slider, () => {
				return new PWGUISettings();
			});
			
			EditorGUILayout.BeginVertical();
			{
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
				EditorGUILayout.BeginHorizontal();
				{
					if (!fieldSettings.editing)
					{
						name.text += value.ToString();
						GUILayout.Label(name, centeredLabel);
						Rect valueRect = GUILayoutUtility.GetLastRect();
						if (valueRect.Contains(e.mousePosition) && e.type == EventType.MouseDown)
						{
							e.Use();
							if (e.clickCount == 2)
								fieldSettings.editing = true;
						}
					}
					else
					{
						GUI.SetNextControlName("slider-value-" + value.GetHashCode());
						GUILayout.FlexibleSpace();
						value = EditorGUILayout.FloatField(value, GUILayout.Width(50));
						Rect valueRect = GUILayoutUtility.GetLastRect();
						GUILayout.FlexibleSpace();
						if ((!valueRect.Contains(e.mousePosition) && e.type == EventType.MouseDown) || (e.isKey && e.keyCode == KeyCode.Return))
							{ fieldSettings.editing = false; e.Use(); }
						if (e.isKey && e.keyCode == KeyCode.Escape)
							{ fieldSettings.editing = false; e.Use(); }
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();

			return value;
		}
		
		public int IntSlider(int value, int min, int max, int step = 1, params PWGUIStyle[] styles)
		{
			return IntSlider((GUIContent)null, value, ref min, ref max, step, false, false, styles);
		}
		
		public int IntSlider(string name, int value, int min, int max, int step = 1, params PWGUIStyle[] styles)
		{
			return IntSlider(new GUIContent(name), value, min, max, step, styles);
		}

		public int IntSlider(GUIContent name, int value, int min, int max, int step = 1, params PWGUIStyle[] styles)
		{
			return IntSlider(name, value, ref min, ref max, step, false, false, styles);
		}
		
		public int IntSlider(string name, int value, ref int min, ref int max, int step = 1, bool editableMin = true, bool editableMax = true, params PWGUIStyle[] styles)
		{
			return IntSlider(new GUIContent(name), value, ref min, ref max, step, editableMin, editableMax, styles);
		}
	
		public int IntSlider(GUIContent name, int value, ref int min, ref int max, int step = 1, bool editableMin = true, bool editableMax = true, params PWGUIStyle[] styles)
		{
			float		v = value;
			float		m_min = min;
			float		m_max = max;
			value = (int)Slider(name, v, ref m_min, ref m_max, step, editableMin, editableMax, true, styles);
			min = (int)m_min;
			max = (int)m_max;
			return value;
		}
	
	#endregion

	#region TexturePreview field

		public void TexturePreview(Texture tex, bool settings = true)
		{
			TexturePreview(tex, settings, true, false);
		}

		public void TexturePreview(Rect previewRect, Texture tex, bool settings = true)
		{
			TexturePreview(previewRect, tex, settings, true, false);
		}
		
		Rect TexturePreview(Texture tex, bool settings, bool settingsStorage, bool debug)
		{
			Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(0));
			previewRect.size = (currentWindowRect.width - 20 - 10) * Vector2.one;
			GUILayout.Space(previewRect.width);
			TexturePreview(previewRect, tex, settings);
			return previewRect;
		}

		void TexturePreview(Rect previewRect, Texture tex, bool settings, bool settingsStorage, bool debug)
		{
			var e = Event.current;

			if (!settingsStorage)
			{
				EditorGUI.DrawPreviewTexture(previewRect, tex);
				if (debug)
					DisplayTextureDebug(previewRect, tex as Texture2D);
				return ;
			}

			//create or load texture settings
			var fieldSettings = GetGUISettingData(PWGUIFieldType.TexturePreview, () => {
				var state = new PWGUISettings();
				state.filterMode = FilterMode.Bilinear;
				state.scaleMode = ScaleMode.ScaleToFit;
				state.scaleAspect = 1;
				state.material = null;
				state.debug = false;
				return state;
			});

			if (e.type == EventType.Repaint)
				fieldSettings.savedRect = previewRect;

			EditorGUI.DrawPreviewTexture(previewRect, tex, fieldSettings.material, fieldSettings.scaleMode, fieldSettings.scaleAspect);

			if (!settings)
				return ;

			//render the texture settings window
			if (e.type == EventType.ExecuteCommand && e.commandName == "TextureSettingsUpdate")
			{
				fieldSettings.scaleAspect = PWTextureSettingsPopup.scaleAspect;
				fieldSettings.scaleMode = PWTextureSettingsPopup.scaleMode;
				fieldSettings.material = PWTextureSettingsPopup.material;
				fieldSettings.filterMode = PWTextureSettingsPopup.filterMode;
				fieldSettings.debug = PWTextureSettingsPopup.debug;
				tex.filterMode = fieldSettings.filterMode;
			}

			//render debug:
			if (fieldSettings.frameSafeDebug)
				DisplayTextureDebug(fieldSettings.savedRect, tex as Texture2D);

			int		icSettingsSize = 16;
			Rect	icSettingsRect = new Rect(previewRect.x + previewRect.width - icSettingsSize, previewRect.y, icSettingsSize, icSettingsSize);
			GUI.DrawTexture(icSettingsRect, icSettingsOutline);
			if (e.type == EventType.MouseDown && e.button == 0)
			{
				if (icSettingsRect.Contains(e.mousePosition))
				{
					PWTextureSettingsPopup.OpenPopup(fieldSettings.filterMode, fieldSettings.scaleMode, fieldSettings.scaleAspect, fieldSettings.material, fieldSettings.debug);
					e.Use();
				}
			}
		}

		void DisplayTextureDebug(Rect textureRect, Texture2D tex)
		{
			var e = Event.current;

			Vector2 pixelPos = e.mousePosition - textureRect.position;

			// Debug.Log("pixel: " + pixelPos + "texwidth: " + tex.width + ", " + textureRect.width+ ", r:" + tex.width / textureRect.width);
			if (textureRect.width > 0)
				pixelPos *= tex.width / textureRect.width;

			if (pixelPos.x >= 0 && pixelPos.y >= 0 && pixelPos.x < tex.width && pixelPos.y < tex.height)
			{
				try {
					Color pixel = tex.GetPixel((int)pixelPos.x, (int)pixelPos.y);
					EditorGUILayout.LabelField("pixel(" + (int)pixelPos.x + ", " + (int)pixelPos.y + ")");
					EditorGUILayout.LabelField(pixel.ToString("F2"));
				} catch {
					EditorGUILayout.LabelField("Texture is not readble !");
				}
			}
		}

	#endregion

	#region Sampler2DPreview field
		
		public void Sampler2DPreview(Sampler2D samp, bool settings = true, FilterMode fm = FilterMode.Bilinear)
		{
			Sampler2DPreview((GUIContent)null, samp, settings, fm);
		}
		
		public void Sampler2DPreview(string prefix, Sampler2D samp, bool settings = true, FilterMode fm = FilterMode.Bilinear)
		{
			Sampler2DPreview(new GUIContent(prefix), samp, settings, fm);
		}
		
		public void Sampler2DPreview(GUIContent prefix, Sampler2D samp, bool settings = true, FilterMode fm = FilterMode.Bilinear)
		{
			int previewSize = (int)currentWindowRect.width - 20 - 20; //padding + texture margin
			var e = Event.current;

			if (samp == null)
				return ;

			if (prefix != null && !String.IsNullOrEmpty(prefix.text))
				EditorGUILayout.LabelField(prefix);

			var fieldSettings = GetGUISettingData(PWGUIFieldType.Sampler2DPreview, () => {
				var state = new PWGUISettings();
				state.filterMode = fm;
				state.debug = false;
				state.gradient = new SerializableGradient(
					PWUtils.CreateGradient(
						new KeyValuePair< float, Color >(0, Color.black),
						new KeyValuePair< float, Color >(1, Color.white)
					)
				);
				state.serializableGradient = (SerializableGradient)state.gradient;
				return state;
			});
		
			fieldSettings.sampler2D = samp;

			//avoid unity's ArgumentException for control position is the sampler value is set outside of the layout event:
			if (fieldSettings.firstRender && e.type != EventType.Layout)
				return ;
			fieldSettings.firstRender = false;
			// Debug.Log("event: " + attachedNode + ":" + e.type);

			//recreated texture if it has been destoryed:
			if (fieldSettings.texture == null)
			{
				fieldSettings.texture = new Texture2D(previewSize, previewSize, TextureFormat.RGBA32, false);
				fieldSettings.texture.filterMode = fieldSettings.filterMode;
				fieldSettings.samplerTextureUpdated = false;
			}

			//same for the gradient:
			if (fieldSettings.gradient == null || fieldSettings.gradient.alphaKeys == null)
				fieldSettings.gradient = fieldSettings.serializableGradient;

			Texture2D	tex = fieldSettings.texture;

			if (samp.size != tex.width)
				UpdateSampler2D(fieldSettings);
			
			//if the preview texture of the sampler have not been updated, we try to update it
			if (!fieldSettings.samplerTextureUpdated || fieldSettings.update)
				UpdateSampler2D(fieldSettings);
			
			Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(0));
			if (previewRect.width > 2)
				fieldSettings.savedRect = previewRect;

			TexturePreview(tex, false, false, false);
			
			if (settings)
			{
				//if the gradient value have been modified, we update the texture
				if (PWSamplerSettingsPopup.controlId == fieldSettings.GetHashCode() && (PWSamplerSettingsPopup.update || (e.type == EventType.ExecuteCommand && e.commandName == "SamplerSettingsUpdate")))
				{
					fieldSettings.gradient = PWSamplerSettingsPopup.gradient;
					fieldSettings.serializableGradient = (SerializableGradient)fieldSettings.gradient;
					fieldSettings.filterMode = PWSamplerSettingsPopup.filterMode;
					fieldSettings.debug = PWSamplerSettingsPopup.debug;

					UpdateSampler2D(fieldSettings);
					
					if (e.type == EventType.ExecuteCommand)
						e.Use();
				}

				//draw the setting icon and manage his events
				int icSettingsSize = 16;
				int	icSettingsPadding = 4;
				Rect icSettingsRect = new Rect(previewRect.x + previewRect.width - icSettingsSize - icSettingsPadding, previewRect.y + icSettingsPadding, icSettingsSize, icSettingsSize);
	
				GUI.DrawTexture(icSettingsRect, icSettingsOutline);
				if (e.type == EventType.MouseDown && e.button == 0)
				{
					if (icSettingsRect.Contains(e.mousePosition))
					{
						PWSamplerSettingsPopup.OpenPopup(fieldSettings);
						e.Use();
					}
				}
			}

			if (!settings && fieldSettings.texture.filterMode != fm)
				fieldSettings.texture.filterMode = fm;

			if (fieldSettings.frameSafeDebug)
			{
				Vector2 pixelPos = e.mousePosition - fieldSettings.savedRect.position;

				pixelPos *= samp.size / fieldSettings.savedRect.width;
				pixelPos.y = samp.size - pixelPos.y;

				EditorGUILayout.LabelField("Sampler2D min: " + samp.min + ", max: " + samp.max);

				if (pixelPos.x >= 0 && pixelPos.y >= 0 && pixelPos.x < samp.size && pixelPos.y < samp.size)
				{
					if (e.type == EventType.MouseMove)
						e.Use();
					EditorGUILayout.LabelField("(" + (int)pixelPos.x + ", " + (int)pixelPos.y + "): " + samp[(int)pixelPos.x, (int)pixelPos.y]);
				}
				else
					EditorGUILayout.LabelField("(NA, NA): NA");
			}
		}

		void UpdateSampler2D(PWGUISettings fieldSettings)
		{
			if (fieldSettings.sampler2D == null || fieldSettings.texture == null)
				return ;
			
			var tex = fieldSettings.texture;
			if (fieldSettings.sampler2D.size != tex.width)	
				tex.Resize(fieldSettings.sampler2D.size, fieldSettings.sampler2D.size, TextureFormat.RGBA32, false);
			
			fieldSettings.sampler2D.Foreach((x, y, val) => {
				tex.SetPixel(x, y, fieldSettings.gradient.Evaluate(Mathf.Clamp01(val)));
			}, true);
			tex.Apply();
			fieldSettings.update = false;
			fieldSettings.samplerTextureUpdated = true;
		}

	#endregion

	#region Sampler field

	public void SamplerPreview(Sampler sampler, bool settings = true)
	{
		SamplerPreview(null, sampler, settings);
	}

	public void SamplerPreview(string name, Sampler sampler, bool settings = true)
	{
		if (sampler == null)
			return ;
		switch (sampler.type)
		{
			case SamplerType.Sampler2D:
				Sampler2DPreview(name, sampler as Sampler2D, settings);
				break ;
			default:
				break ;
		}
	}

	#endregion

	#region BiomeMapPreview field
	
		public void BiomeMap2DPreview(BiomeData map, bool settings = true, bool debug = true)
		{
			BiomeMap2DPreview(new GUIContent(), map, settings, debug);
		}

		public void BiomeMap2DPreview(GUIContent prefix, BiomeData biomeData, bool settings = true, bool debug = true)
		{
			if (biomeData.biomeIds == null)
			{
				Debug.Log("biomeData does not contains biome map 2D");
				return ;
			}
			int texSize = biomeData.biomeIds.size;
			var fieldSettings = GetGUISettingData(PWGUIFieldType.BiomeMapPreview, () => {
				var state = new PWGUISettings();
				state.filterMode = FilterMode.Point;
				state.debug = debug;
				return state;
			});

			fieldSettings.biomeData = biomeData;

			if (fieldSettings.texture == null)
			{
				fieldSettings.texture = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
				fieldSettings.update = true;
				fieldSettings.texture.filterMode = FilterMode.Point;
			}

			if (fieldSettings.update)
				UpdateBiomeMap2D(fieldSettings);

			TexturePreview(fieldSettings.texture, false, false, false);
		}

		void UpdateBiomeMap2D(PWGUISettings fieldSettings)
		{
			if (fieldSettings.biomeData == null || fieldSettings.texture == null)
				return ;
			
			var map = fieldSettings.biomeData.biomeIds;
			int texSize = map.size;
			
			if (texSize != fieldSettings.texture.width)
				fieldSettings.texture.Resize(texSize, texSize, TextureFormat.RGBA32, false);
			
			for (int x = 0; x < texSize; x++)
				for (int y = 0; y < texSize; y++)
				{
					var blendInfo = map.GetBiomeBlendInfo(x, y);
					var biome = fieldSettings.biomeData.biomeTree.GetBiome(blendInfo.firstBiomeId);
					if (biome == null)
						continue ;
					Color firstBiomeColor = biome.previewColor;

					//TODO: second biome color:
					fieldSettings.texture.SetPixel(x, y, firstBiomeColor);
				}
			fieldSettings.texture.Apply();
			fieldSettings.update = false;
		}
	#endregion

	#region ObjectPreview field
		
		public void ObjectPreview(object obj, bool update)
		{
			ObjectPreview((GUIContent)null, obj, update);
		}
		
		public void ObjectPreview(string name, object obj, bool update)
		{
			ObjectPreview(new GUIContent(name), obj, update);
		}

		public void ObjectPreview(GUIContent name, object obj, bool update)
		{
			Type objType = obj.GetType();

			if (objType == typeof(Sampler2D))
				Sampler2DPreview(name, obj as Sampler2D, update);
			else if (obj.GetType().IsSubclassOf(typeof(Object)))
			{
				//unity object preview
			}
			else
				Debug.LogWarning("can't preview the object of type: " + obj.GetType());
		}

	#endregion

	#region Texture2DArray preview field

		public void Texture2DArrayPreview(Texture2DArray textureArray, bool update)
		{
			if (textureArray == null)
				return ;
			var	fieldSettings = GetGUISettingData(PWGUIFieldType.Texture2DArrayPreview, () => new PWGUISettings());
			if (update)
			{
				if (fieldSettings.textures == null || fieldSettings.textures.Length < textureArray.depth)
					fieldSettings.textures = new Texture2D[textureArray.depth];
				for (int i = 0; i < textureArray.depth; i++)
				{
					Texture2D tex = new Texture2D(textureArray.width, textureArray.height, TextureFormat.ARGB32, false);
					tex.SetPixels(textureArray.GetPixels(i));
					tex.Apply();
					fieldSettings.textures[i] = tex;
				}
			}
		
			if (fieldSettings.textures != null)
				foreach (var tex in fieldSettings.textures)
					TexturePreview(tex, false, false, false);
		}
	
	#endregion

	#region PWArray< T > field

	public void PWArrayField< T >(PWArray< T > array)
	{
		var names = array.GetNames();
		var values = array.GetValues();

		EditorGUILayout.LabelField("names: [" + names.Count + "]");
		for (int i = 0; i < values.Count; i++)
		{
			if (i < names.Count && names[i] != null)
			{
				if (values[i] != null)
					EditorGUILayout.LabelField(names[i] + " <" + values[i].GetType() + ": " + values[i] + ">");
				else
					EditorGUILayout.LabelField(names[i]);
			}
			else
				EditorGUILayout.LabelField("null");
		}
	}

	#endregion

	#region FadeGroup block
	
		public bool BeginFade(string header, GUIStyle style = null)
		{
			bool checkbox = false;
			
			return BeginFade(new GUIContent(header), style, ref checkbox, false);
		}
	
		public bool BeginFade(string header, GUIStyle style, ref bool checkbox)
		{
			return BeginFade(new GUIContent(header), style, ref checkbox, true);
		}

		public bool BeginFade(GUIContent header, GUIStyle style, ref bool checkbox, bool checkboxEnabled = true)
		{
			var e = Event.current;
			var settings = GetGUISettingData(PWGUIFieldType.FadeBlock, () => {
				return new PWGUISettings();
			});

			if (settings.fadeStatus == null)
				settings.fadeStatus = new AnimBool(settings.faded);

			EditorGUILayout.BeginVertical(style);

			//header
			EditorGUILayout.BeginHorizontal();
			{
				if (checkboxEnabled)
					checkbox = EditorGUILayout.Toggle(checkbox);
				EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
			}
			EditorGUILayout.EndHorizontal();

			//click in the header to expand block
			Rect headerRect = GUILayoutUtility.GetLastRect();
			if (headerRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
			{
				settings.faded = !settings.faded;
				e.Use();
			}
			
			bool display = EditorGUILayout.BeginFadeGroup(settings.fadeStatus.faded);
			
			settings.fadeStatus.target = settings.faded;

			return display;
		}

		public void EndFade()
		{
			EditorGUILayout.EndFadeGroup();
			EditorGUILayout.EndVertical();
		}

	#endregion

	#region MinMaxSlope field

	public void MinMaxSlope(float min, float max, ref float inputMin, ref float inputMax)
	{
		EditorGUILayout.BeginVertical();
		{
			EditorGUIUtility.labelWidth = 60;
			inputMin = EditorGUILayout.FloatField("Min", inputMin);
			inputMax = EditorGUILayout.FloatField("Max", inputMax);
			EditorGUIUtility.labelWidth = 0;
		}
		EditorGUILayout.EndVertical();
		Rect r = EditorGUILayout.GetControlRect(false, 60);
		MinMaxSlope(r, ref inputMin, ref inputMax);

		inputMax = Mathf.Clamp(inputMax, min, max);
		inputMin = Mathf.Clamp(inputMin, min, max);
	}

	void MinMaxSlope(Rect drawRect, ref float inputMin, ref float inputMax)
	{
		float gapWidth = drawRect.width / 2;
		float minSlopeWidth = Mathf.Cos(inputMin * Mathf.Deg2Rad) * gapWidth;
		float maxSlopwWidth = Mathf.Cos(inputMax * Mathf.Deg2Rad) * gapWidth;
		float minSlopeHeight = Mathf.Sin(inputMin * Mathf.Deg2Rad) * drawRect.height;
		float maxSlopeHeight = Mathf.Sin(inputMax * Mathf.Deg2Rad) * drawRect.height;

		Color minSlopeColor = Color.blue;
		Color maxSlopeColor = Color.red;
		Color baseColor = Color.black;

		Vector2 b1 = new Vector2(drawRect.xMin, drawRect.yMax);
		Vector2 b2 = new Vector2(b1.x + drawRect.width / 4, drawRect.yMax);

		Vector2 m1 = new Vector2((drawRect.width / 4) + minSlopeWidth + drawRect.xMin, drawRect.yMax - minSlopeHeight);
		Vector2 m2 = new Vector2(drawRect.xMax, drawRect.yMax - minSlopeHeight);

		Vector2 n1 = new Vector2((drawRect.width / 4) + maxSlopwWidth + drawRect.xMin, drawRect.yMax - maxSlopeHeight);
		Vector2 n2 = new Vector2(drawRect.xMax, drawRect.yMax - maxSlopeHeight);

		Handles.color = baseColor;
		Handles.DrawAAPolyLine(b1, b2);
		Handles.DrawAAPolyLine(m1, m2);
		Handles.DrawAAPolyLine(n1, n2);

		Vector2 min1 = b2;
		Vector2 min2 = m1;
		
		Handles.color = minSlopeColor;
		Handles.DrawAAPolyLine(min1, min2);

		Vector2 max1 = b2;
		Vector2 max2 = n1;

		Handles.color = maxSlopeColor;
		Handles.DrawAAPolyLine(max1, max2);
	}

	#endregion

	#region Utils

		private T		GetGUISettingData< T >(PWGUIFieldType fieldType, Func< T > newGUISettings) where T : PWGUISettings
		{
			
			if (currentSettingCount == settingsStorage.Count)
			{
				var s = newGUISettings();
				s.fieldType = fieldType;

				s.windowPosition = PWUtils.Round(editorWindowRect.size / 2);
				settingsStorage.Add(s);
			}
			if (settingsStorage[currentSettingCount].fieldType != fieldType)
			{
				//if the fileType does not match, we reset all GUI datas
				settingsStorage[currentSettingCount] = newGUISettings();
				settingsStorage[currentSettingCount].fieldType = fieldType;
			}
			return settingsStorage[currentSettingCount++] as T;
		}

		public void	StartFrame(Rect currentWindowRect)
		{
			currentSettingCount = 0;

			this.currentWindowRect = currentWindowRect;

			if (icColor != null)
				return ;

			icColor = Resources.Load("Icons/ic_color") as Texture2D;
			icEdit = Resources.Load("Icons/ic_edit") as Texture2D;
			icSettingsOutline = Resources.Load("Icons/ic_settings_outline") as Texture2D;
			centeredLabel = new GUIStyle();
			centeredLabel.alignment = TextAnchor.MiddleCenter;
		}

		//A negative value of fieldIndex will take the objectat the specified index starting from the end
		public void SetGradientForField(int fieldIndex, Gradient g)
		{
			//if the specified index is negative, we set it to all the GUISettings
			if (fieldIndex < 0)
			{
				foreach (var setting in settingsStorage)
				{
					if (setting.gradient == g)
						continue ;
					
					setting.gradient = g;
					setting.serializableGradient = (SerializableGradient)g;
					setting.update = true;
				}
				return ;
			}

			if (fieldIndex >= settingsStorage.Count)
			{
				Debug.LogWarning("can't find the PWGUI setting datas at index: " + fieldIndex);
				return ;
			}

			if (settingsStorage[fieldIndex].gradient == g)
				return ;

			settingsStorage[fieldIndex].gradient = g;
			settingsStorage[fieldIndex].serializableGradient = (SerializableGradient)g;
			settingsStorage[fieldIndex].update = true;
		}

		public void SetDebugForField(int fieldIndex, bool value)
		{
			//if the specified index is negative, we set it to all the GUISettings
			if (fieldIndex < 0)
			{
				foreach (var setting in settingsStorage)
					setting.debug = value;
				return ;
			}

			if (fieldIndex >= settingsStorage.Count)
			{
				Debug.LogWarning("can't find the PWGUI setting datas at index: " + fieldIndex);
				return ;
			}
			
			settingsStorage[fieldIndex].debug = value;
		}

		public void SetScaleModeForField(int fieldIndex, ScaleMode mode)
		{
			//if the specified index is negative, we set it to all the GUISettings
			if (fieldIndex < 0)
			{
				foreach (var setting in settingsStorage)
					setting.scaleMode = mode;
				return ;
			}

			if (fieldIndex >= settingsStorage.Count)
			{
				Debug.LogWarning("can't find the PWGUI setting datas at index: " + fieldIndex);
				return ;
			}

			settingsStorage[fieldIndex].scaleMode = mode;
		}
		
		public void SetScaleAspectForField(int fieldIndex, float aspect)
		{
			//if the specified index is negative, we set it to all the GUISettings
			if (fieldIndex < 0)
			{
				foreach (var setting in settingsStorage)
					setting.scaleAspect = aspect;
				return ;
			}

			if (fieldIndex >= settingsStorage.Count)
			{
				Debug.LogWarning("can't find the PWGUI setting datas at index: " + fieldIndex);
				return ;
			}

			settingsStorage[fieldIndex].scaleAspect = aspect;
		}
		
		public void SetMaterialForField(int fieldIndex, Material mat)
		{
			//if the specified index is negative, we set it to all the GUISettings
			if (fieldIndex < 0)
			{
				foreach (var setting in settingsStorage)
					setting.material = mat;
				return ;
			}

			if (fieldIndex >= settingsStorage.Count)
			{
				Debug.LogWarning("can't find the PWGUI setting datas at index: " + fieldIndex);
				return ;
			}

			settingsStorage[fieldIndex].material = mat;
		}
		
		public void SetUpdateForField(int fieldIndex, bool update)
		{
			//if the specified index is negative, we set it to all the GUISettings
			if (fieldIndex < 0)
			{
				foreach (var setting in settingsStorage)
					setting.update = update;
				return ;
			}

			if (fieldIndex >= settingsStorage.Count)
			{
				Debug.LogWarning("can't find the PWGUI setting datas at index: " + fieldIndex);
				return ;
			}

			settingsStorage[fieldIndex].update = update;
		}

	#endregion
	}
}
