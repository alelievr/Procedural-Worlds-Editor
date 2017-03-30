#if !UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityEditor {

	public class SerializedProperty {}
	public class MessageType {}
	public class ColorPickerHDRConfig {}
	public class EditorSkin {}
	public enum SerializedPropertyType
	{
		Generic = -1,
				Integer,
				Boolean,
				Float,
				String,
				Color,
				ObjectReference,
				LayerMask,
				Enum,
				Vector2,
				Vector3,
				Vector4,
				Rect,
				ArraySize,
				Character,
				AnimationCurve,
				Bounds,
				Gradient,
				Quaternion
	}

	public enum MouseCursor
	{
		Arrow,
		Text,
		ResizeVertical,
		ResizeHorizontal,
		Link,
		SlideArrow,
		ResizeUpRight,
		ResizeUpLeft,
		MoveArrow,
		RotateArrow,
		ScaleArrow,
		ArrowPlus,
		ArrowMinus,
		Pan,
		Orbit,
		Zoom,
		FPS,
		CustomCursor,
		SplitResizeUpDown,
		SplitResizeLeftRight
	}


	public class EditorGUILayout {

		public class ToggleGroupScope : GUI.Scope
		{
			public bool enabled;
	
			public ToggleGroupScope(string label, bool toggle) {}
			public ToggleGroupScope(GUIContent label, bool toggle) {}
			protected override void CloseScope() {}
		}

		public class HorizontalScope : GUI.Scope
		{
			public Rect rect;

			public HorizontalScope(params GUILayoutOption[] options) {}

			public HorizontalScope(GUIStyle style, params GUILayoutOption[] options) {}

			protected override void CloseScope() {}
		}

		public class VerticalScope : GUI.Scope
		{
			public Rect rect;

			public VerticalScope(params GUILayoutOption[] options) {}

			public VerticalScope(GUIStyle style, params GUILayoutOption[] options) {}

			protected override void CloseScope() {}
		}

		public class ScrollViewScope : GUI.Scope
		{
			public Vector2 scrollPosition;

			public bool handleScrollWheel;

			public ScrollViewScope(Vector2 scrollPosition, params GUILayoutOption[] options) {}

			public ScrollViewScope(Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, params GUILayoutOption[] options) {}

			public ScrollViewScope(Vector2 scrollPosition, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params GUILayoutOption[] options) {}

			public ScrollViewScope(Vector2 scrollPosition, GUIStyle style, params GUILayoutOption[] options) {}

			public ScrollViewScope(Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background, params GUILayoutOption[] options) {}

			protected override void CloseScope() {}
		}

		public class FadeGroupScope : GUI.Scope
		{
			public bool visible;

			public FadeGroupScope(float value) {}

			protected override void CloseScope() {}
		}

		public static bool Foldout(bool foldout, string content) { return true; }

		public static bool Foldout(bool foldout, string content, GUIStyle style) { return true; }

	 	public static bool Foldout(bool foldout, GUIContent content) { return true; }

		public static bool Foldout(bool foldout, GUIContent content, GUIStyle style) { return true; }

		public static bool Foldout(bool foldout, string content, bool toggleOnLabelClick) { return true; }

		public static bool Foldout(bool foldout, string content, bool toggleOnLabelClick, GUIStyle style) { return true; }

		public static bool Foldout(bool foldout, GUIContent content, bool toggleOnLabelClick) { return true; }

		public static bool Foldout(bool foldout, GUIContent content, bool toggleOnLabelClick, GUIStyle style) { return true; }

		public static void PrefixLabel(string label) {}

		public static void PrefixLabel(string label, GUIStyle followingStyle) {}

		public static void PrefixLabel(string label, GUIStyle followingStyle, GUIStyle labelStyle) {}

		public static void PrefixLabel(GUIContent label) {}

		public static void PrefixLabel(GUIContent label, GUIStyle followingStyle) {}

		public static void PrefixLabel(GUIContent label, GUIStyle followingStyle, GUIStyle labelStyle) {}

		public static void LabelField(string label, params GUILayoutOption[] options) {}

		public static void LabelField(string label, GUIStyle style, params GUILayoutOption[] options) {}

		public static void LabelField(GUIContent label, params GUILayoutOption[] options) {}

		public static void LabelField(GUIContent label, GUIStyle style, params GUILayoutOption[] options) {}

		public static void LabelField(string label, string label2, params GUILayoutOption[] options) {}

		public static void LabelField(string label, string label2, GUIStyle style, params GUILayoutOption[] options) {}

		public static void LabelField(GUIContent label, GUIContent label2, params GUILayoutOption[] options) {}

		public static void LabelField(GUIContent label, GUIContent label2, GUIStyle style, params GUILayoutOption[] options) {}

		public static bool Toggle(bool value, params GUILayoutOption[] options) { return true; }

		public static bool Toggle(string label, bool value, params GUILayoutOption[] options) { return true; }

		public static bool Toggle(GUIContent label, bool value, params GUILayoutOption[] options) { return true; }

		public static bool Toggle(bool value, GUIStyle style, params GUILayoutOption[] options) { return true; }

		public static bool Toggle(string label, bool value, GUIStyle style, params GUILayoutOption[] options) { return true; }

		public static bool Toggle(GUIContent label, bool value, GUIStyle style, params GUILayoutOption[] options) { return true; }

		public static bool ToggleLeft(string label, bool value, params GUILayoutOption[] options) { return true; }

		public static bool ToggleLeft(GUIContent label, bool value, params GUILayoutOption[] options) { return true; }

		public static bool ToggleLeft(string label, bool value, GUIStyle labelStyle, params GUILayoutOption[] options) { return true; }

		public static bool ToggleLeft(GUIContent label, bool value, GUIStyle labelStyle, params GUILayoutOption[] options) { return true; }

		public static string TextField(string text, params GUILayoutOption[] options) { return null; }

		public static string TextField(string text, GUIStyle style, params GUILayoutOption[] options) { return null; }

		public static string TextField(string label, string text, params GUILayoutOption[] options) { return null; }

		public static string TextField(string label, string text, GUIStyle style, params GUILayoutOption[] options) { return null; }

		public static string TextField(GUIContent label, string text, params GUILayoutOption[] options) { return null; }

		public static string TextField(GUIContent label, string text, GUIStyle style, params GUILayoutOption[] options) { return null; }

		public static string DelayedTextField(string text, params GUILayoutOption[] options) { return null; }

		public static string DelayedTextField(string text, GUIStyle style, params GUILayoutOption[] options) { return null; }

		public static string DelayedTextField(string label, string text, params GUILayoutOption[] options) { return null; }

		public static string DelayedTextField(string label, string text, GUIStyle style, params GUILayoutOption[] options) { return null; }

		public static string DelayedTextField(GUIContent label, string text, params GUILayoutOption[] options) { return null; }

		public static string DelayedTextField(GUIContent label, string text, GUIStyle style, params GUILayoutOption[] options) { return null; }

		public static void DelayedTextField(SerializedProperty property, params GUILayoutOption[] options) {}

		public static void DelayedTextField(SerializedProperty property, GUIContent label, params GUILayoutOption[] options) {}

		public static string TextArea(string text, params GUILayoutOption[] options) { return null; }

		public static string TextArea(string text, GUIStyle style, params GUILayoutOption[] options) { return null; }

		public static void SelectableLabel(string text, params GUILayoutOption[] options) {}

		public static void SelectableLabel(string text, GUIStyle style, params GUILayoutOption[] options) {}

		public static string PasswordField(string password, params GUILayoutOption[] options) { return null; }

		public static string PasswordField(string password, GUIStyle style, params GUILayoutOption[] options) { return null; }

		public static string PasswordField(string label, string password, params GUILayoutOption[] options) { return null; }

		public static string PasswordField(string label, string password, GUIStyle style, params GUILayoutOption[] options) { return null; }

		public static string PasswordField(GUIContent label, string password, params GUILayoutOption[] options) { return null; }

		public static string PasswordField(GUIContent label, string password, GUIStyle style, params GUILayoutOption[] options) { return null; }

		public static float FloatField(float value, params GUILayoutOption[] options) { return 0f; }

		public static float FloatField(float value, GUIStyle style, params GUILayoutOption[] options) { return 0f; }

		public static float FloatField(string label, float value, params GUILayoutOption[] options) { return 0f; }

		public static float FloatField(string label, float value, GUIStyle style, params GUILayoutOption[] options) { return 0f; }

		public static float FloatField(GUIContent label, float value, params GUILayoutOption[] options) { return 0f; }

		public static float FloatField(GUIContent label, float value, GUIStyle style, params GUILayoutOption[] options) { return 0f; }

		public static float DelayedFloatField(float value, params GUILayoutOption[] options) { return 0f; }

		public static float DelayedFloatField(float value, GUIStyle style, params GUILayoutOption[] options) { return 0f; }

		public static float DelayedFloatField(string label, float value, params GUILayoutOption[] options) { return 0f; }

		public static float DelayedFloatField(string label, float value, GUIStyle style, params GUILayoutOption[] options) { return 0f; }

		public static float DelayedFloatField(GUIContent label, float value, params GUILayoutOption[] options) { return 0f; }

		public static float DelayedFloatField(GUIContent label, float value, GUIStyle style, params GUILayoutOption[] options) { return 0f; }

		public static void DelayedFloatField(SerializedProperty property, params GUILayoutOption[] options) {}

		public static void DelayedFloatField(SerializedProperty property, GUIContent label, params GUILayoutOption[] options) {}

		public static double DoubleField(double value, params GUILayoutOption[] options) { return 0.0; }

		public static double DoubleField(double value, GUIStyle style, params GUILayoutOption[] options) { return 0.0; }

		public static double DoubleField(string label, double value, params GUILayoutOption[] options) { return 0.0; }

		public static double DoubleField(string label, double value, GUIStyle style, params GUILayoutOption[] options) { return 0.0; }

		public static double DoubleField(GUIContent label, double value, params GUILayoutOption[] options) { return 0.0; }

		public static double DoubleField(GUIContent label, double value, GUIStyle style, params GUILayoutOption[] options) { return 0.0; }

		public static double DelayedDoubleField(double value, params GUILayoutOption[] options) { return 0.0; }

		public static double DelayedDoubleField(double value, GUIStyle style, params GUILayoutOption[] options) { return 0.0; }

		public static double DelayedDoubleField(string label, double value, params GUILayoutOption[] options) { return 0.0; }

		public static double DelayedDoubleField(string label, double value, GUIStyle style, params GUILayoutOption[] options) { return 0.0; }

		public static double DelayedDoubleField(GUIContent label, double value, params GUILayoutOption[] options) { return 0.0; }

		public static double DelayedDoubleField(GUIContent label, double value, GUIStyle style, params GUILayoutOption[] options) { return 0.0; }

		public static int IntField(int value, params GUILayoutOption[] options) { return 0; }

		public static int IntField(int value, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int IntField(string label, int value, params GUILayoutOption[] options) { return 0; }

		public static int IntField(string label, int value, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int IntField(GUIContent label, int value, params GUILayoutOption[] options) { return 0; }

		public static int IntField(GUIContent label, int value, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int DelayedIntField(int value, params GUILayoutOption[] options) { return 0; }

		public static int DelayedIntField(int value, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int DelayedIntField(string label, int value, params GUILayoutOption[] options) { return 0; }

		public static int DelayedIntField(string label, int value, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int DelayedIntField(GUIContent label, int value, params GUILayoutOption[] options) { return 0; }

		public static int DelayedIntField(GUIContent label, int value, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static void DelayedIntField(SerializedProperty property, params GUILayoutOption[] options) {}

		public static void DelayedIntField(SerializedProperty property, GUIContent label, params GUILayoutOption[] options) {}

		public static long LongField(long value, params GUILayoutOption[] options) { return 0; }

		public static long LongField(long value, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static long LongField(string label, long value, params GUILayoutOption[] options) { return 0; }

		public static long LongField(string label, long value, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static long LongField(GUIContent label, long value, params GUILayoutOption[] options) { return 0; }

		public static long LongField(GUIContent label, long value, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static float Slider(float value, float leftValue, float rightValue, params GUILayoutOption[] options) { return 0f; }

		public static float Slider(string label, float value, float leftValue, float rightValue, params GUILayoutOption[] options) { return 0f; }

		public static float Slider(GUIContent label, float value, float leftValue, float rightValue, params GUILayoutOption[] options) { return 0f; }

		public static void Slider(SerializedProperty property, float leftValue, float rightValue, params GUILayoutOption[] options) {}

		public static void Slider(SerializedProperty property, float leftValue, float rightValue, string label, params GUILayoutOption[] options) {}

		public static void Slider(SerializedProperty property, float leftValue, float rightValue, GUIContent label, params GUILayoutOption[] options) {}

		public static int IntSlider(int value, int leftValue, int rightValue, params GUILayoutOption[] options) { return 0; }

		public static int IntSlider(string label, int value, int leftValue, int rightValue, params GUILayoutOption[] options) { return 0; }

		public static int IntSlider(GUIContent label, int value, int leftValue, int rightValue, params GUILayoutOption[] options) { return 0; }

		public static void IntSlider(SerializedProperty property, int leftValue, int rightValue, params GUILayoutOption[] options) {}

		public static void IntSlider(SerializedProperty property, int leftValue, int rightValue, string label, params GUILayoutOption[] options) {}

		public static void IntSlider(SerializedProperty property, int leftValue, int rightValue, GUIContent label, params GUILayoutOption[] options) {}

		public static void MinMaxSlider(ref float minValue, ref float maxValue, float minLimit, float maxLimit, params GUILayoutOption[] options) {}

		public static void MinMaxSlider(string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, params GUILayoutOption[] options) {}

		public static void MinMaxSlider(GUIContent label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, params GUILayoutOption[] options) {}

		public static int Popup(int selectedIndex, string[] displayedOptions, params GUILayoutOption[] options) { return 0; }

		public static int Popup(int selectedIndex, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int Popup(int selectedIndex, GUIContent[] displayedOptions, params GUILayoutOption[] options) { return 0; }

		public static int Popup(int selectedIndex, GUIContent[] displayedOptions, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int Popup(string label, int selectedIndex, string[] displayedOptions, params GUILayoutOption[] options) { return 0; }

		public static int Popup(string label, int selectedIndex, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int Popup(GUIContent label, int selectedIndex, GUIContent[] displayedOptions, params GUILayoutOption[] options) { return 0; }

		public static int Popup(GUIContent label, int selectedIndex, GUIContent[] displayedOptions, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static Enum EnumPopup(Enum selected, params GUILayoutOption[] options) { return selected; }

		public static Enum EnumPopup(Enum selected, GUIStyle style, params GUILayoutOption[] options) { return selected; }

		public static Enum EnumPopup(string label, Enum selected, params GUILayoutOption[] options) { return selected; }

		public static Enum EnumPopup(string label, Enum selected, GUIStyle style, params GUILayoutOption[] options) { return selected; }

		public static Enum EnumPopup(GUIContent label, Enum selected, params GUILayoutOption[] options) { return selected; }

		public static Enum EnumPopup(GUIContent label, Enum selected, GUIStyle style, params GUILayoutOption[] options) { return selected; }

		public static Enum EnumMaskPopup(string label, Enum selected, params GUILayoutOption[] options) { return selected; }

		public static Enum EnumMaskPopup(string label, Enum selected, GUIStyle style, params GUILayoutOption[] options) { return selected; }

		public static Enum EnumMaskPopup(GUIContent label, Enum selected, params GUILayoutOption[] options) { return selected; }

		public static Enum EnumMaskPopup(GUIContent label, Enum selected, GUIStyle style, params GUILayoutOption[] options) { return selected; }

		public static int IntPopup(int selectedValue, string[] displayedOptions, int[] optionValues, params GUILayoutOption[] options) { return 0; }

		public static int IntPopup(int selectedValue, string[] displayedOptions, int[] optionValues, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int IntPopup(int selectedValue, GUIContent[] displayedOptions, int[] optionValues, params GUILayoutOption[] options) { return 0; }

		public static int IntPopup(int selectedValue, GUIContent[] displayedOptions, int[] optionValues, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int IntPopup(string label, int selectedValue, string[] displayedOptions, int[] optionValues, params GUILayoutOption[] options) { return 0; }

		public static int IntPopup(string label, int selectedValue, string[] displayedOptions, int[] optionValues, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int IntPopup(GUIContent label, int selectedValue, GUIContent[] displayedOptions, int[] optionValues, params GUILayoutOption[] options) { return 0; }

		public static int IntPopup(GUIContent label, int selectedValue, GUIContent[] displayedOptions, int[] optionValues, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static void IntPopup(SerializedProperty property, GUIContent[] displayedOptions, int[] optionValues, params GUILayoutOption[] options) {}

		public static void IntPopup(SerializedProperty property, GUIContent[] displayedOptions, int[] optionValues, GUIContent label, params GUILayoutOption[] options) {}

		[Obsolete("This function is obsolete and the style is not used.")]
		public static void IntPopup(SerializedProperty property, GUIContent[] displayedOptions, int[] optionValues, GUIContent label, GUIStyle style, params GUILayoutOption[] options) {}

		public static string TagField(string tag, params GUILayoutOption[] options) { return null; }

		public static string TagField(string tag, GUIStyle style, params GUILayoutOption[] options) { return null; }

		public static string TagField(string label, string tag, params GUILayoutOption[] options) { return null; }

		public static string TagField(string label, string tag, GUIStyle style, params GUILayoutOption[] options) { return null; }

		public static string TagField(GUIContent label, string tag, params GUILayoutOption[] options) { return null; }

		public static string TagField(GUIContent label, string tag, GUIStyle style, params GUILayoutOption[] options) { return null; }

		public static int LayerField(int layer, params GUILayoutOption[] options) { return 0; }

		public static int LayerField(int layer, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int LayerField(string label, int layer, params GUILayoutOption[] options) { return 0; }

		public static int LayerField(string label, int layer, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int LayerField(GUIContent label, int layer, params GUILayoutOption[] options) { return 0; }

		public static int LayerField(GUIContent label, int layer, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int MaskField(GUIContent label, int mask, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int MaskField(string label, int mask, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int MaskField(GUIContent label, int mask, string[] displayedOptions, params GUILayoutOption[] options) { return 0; }

		public static int MaskField(string label, int mask, string[] displayedOptions, params GUILayoutOption[] options) { return 0; }

		public static int MaskField(int mask, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options) { return 0; }

		public static int MaskField(int mask, string[] displayedOptions, params GUILayoutOption[] options) { return 0; }

		public static Enum EnumMaskField(GUIContent label, Enum enumValue, GUIStyle style, params GUILayoutOption[] options) { return enumValue; }

		public static Enum EnumMaskField(string label, Enum enumValue, GUIStyle style, params GUILayoutOption[] options) { return enumValue; }

		public static Enum EnumMaskField(GUIContent label, Enum enumValue, params GUILayoutOption[] options) { return enumValue; }

		public static Enum EnumMaskField(string label, Enum enumValue, params GUILayoutOption[] options) { return enumValue; }

		public static Enum EnumMaskField(Enum enumValue, GUIStyle style, params GUILayoutOption[] options) { return enumValue; }

		public static Enum EnumMaskField(Enum enumValue, params GUILayoutOption[] options) { return enumValue; }

		[Obsolete("Check the docs for the usage of the new parameter 'allowSceneObjects'.")]
		public static UnityEngine.Object ObjectField(UnityEngine.Object obj, Type objType, params GUILayoutOption[] options) { return null; }

		public static UnityEngine.Object ObjectField(UnityEngine.Object obj, Type objType, bool allowSceneObjects, params GUILayoutOption[] options) { return null; }

		[Obsolete("Check the docs for the usage of the new parameter 'allowSceneObjects'.")]
		public static UnityEngine.Object ObjectField(string label, UnityEngine.Object obj, Type objType, params GUILayoutOption[] options) { return null; }

		public static UnityEngine.Object ObjectField(string label, UnityEngine.Object obj, Type objType, bool allowSceneObjects, params GUILayoutOption[] options) { return null; }

		[Obsolete("Check the docs for the usage of the new parameter 'allowSceneObjects'.")]
		public static UnityEngine.Object ObjectField(GUIContent label, UnityEngine.Object obj, Type objType, params GUILayoutOption[] options) { return null; }

		public static UnityEngine.Object ObjectField(GUIContent label, UnityEngine.Object obj, Type objType, bool allowSceneObjects, params GUILayoutOption[] options) { return null; }

		public static void ObjectField(SerializedProperty property, params GUILayoutOption[] options) {}

		public static void ObjectField(SerializedProperty property, GUIContent label, params GUILayoutOption[] options) {}

		public static void ObjectField(SerializedProperty property, Type objType, params GUILayoutOption[] options) {}

		public static void ObjectField(SerializedProperty property, Type objType, GUIContent label, params GUILayoutOption[] options) {}

		public static Vector2 Vector2Field(string label, Vector2 value, params GUILayoutOption[] options) { return Vector2.zero; }

		public static Vector2 Vector2Field(GUIContent label, Vector2 value, params GUILayoutOption[] options) { return Vector2.zero; }

		public static Vector3 Vector3Field(string label, Vector3 value, params GUILayoutOption[] options) { return Vector3.zero; }

		public static Vector3 Vector3Field(GUIContent label, Vector3 value, params GUILayoutOption[] options) { return Vector3.zero; }

		public static Vector4 Vector4Field(string label, Vector4 value, params GUILayoutOption[] options) { return Vector4.zero; }

		public static Vector4 Vector4Field(GUIContent label, Vector4 value, params GUILayoutOption[] options) { return Vector4.zero; }

		public static Rect RectField(Rect value, params GUILayoutOption[] options) { return new Rect(); }

		public static Rect RectField(string label, Rect value, params GUILayoutOption[] options) { return new Rect(); }

		public static Rect RectField(GUIContent label, Rect value, params GUILayoutOption[] options) { return new Rect(); }

		public static Bounds BoundsField(Bounds value, params GUILayoutOption[] options) { return new Bounds(); }

		public static Bounds BoundsField(string label, Bounds value, params GUILayoutOption[] options) { return new Bounds(); }

		public static Bounds BoundsField(GUIContent label, Bounds value, params GUILayoutOption[] options) { return new Bounds(); }

		public static Color ColorField(Color value, params GUILayoutOption[] options) { return Color.white; }

		public static Color ColorField(string label, Color value, params GUILayoutOption[] options) { return Color.white; }

		public static Color ColorField(GUIContent label, Color value, params GUILayoutOption[] options) { return Color.white; }

		public static Color ColorField(GUIContent label, Color value, bool showEyedropper, bool showAlpha, bool hdr, ColorPickerHDRConfig hdrConfig, params GUILayoutOption[] options) { return Color.white; }

		public static AnimationCurve CurveField(AnimationCurve value, params GUILayoutOption[] options) { return null; }

		public static AnimationCurve CurveField(string label, AnimationCurve value, params GUILayoutOption[] options) { return null; }

		public static AnimationCurve CurveField(GUIContent label, AnimationCurve value, params GUILayoutOption[] options) { return null; }

		public static AnimationCurve CurveField(AnimationCurve value, Color color, Rect ranges, params GUILayoutOption[] options) { return null; }

		public static AnimationCurve CurveField(string label, AnimationCurve value, Color color, Rect ranges, params GUILayoutOption[] options) { return null; }

		public static AnimationCurve CurveField(GUIContent label, AnimationCurve value, Color color, Rect ranges, params GUILayoutOption[] options) { return null; }

		public static void CurveField(SerializedProperty property, Color color, Rect ranges, params GUILayoutOption[] options) {}

		public static void CurveField(SerializedProperty property, Color color, Rect ranges, GUIContent label, params GUILayoutOption[] options) {}

		public static bool InspectorTitlebar(bool foldout, UnityEngine.Object targetObj) { return true; }

		public static bool InspectorTitlebar(bool foldout, UnityEngine.Object targetObj, bool expandable) { return true; }

		public static bool InspectorTitlebar(bool foldout, UnityEngine.Object[] targetObjs) { return true; }

		public static bool InspectorTitlebar(bool foldout, UnityEngine.Object[] targetObjs, bool expandable) { return true; }

		public static void InspectorTitlebar(UnityEngine.Object[] targetObjs) {}

		public static void HelpBox(string message, MessageType type) {}

		public static void HelpBox(string message, MessageType type, bool wide) {}

		public static void Space() {}

		public static void Separator() {}

		public static bool BeginToggleGroup(string label, bool toggle) { return true; }

		public static bool BeginToggleGroup(GUIContent label, bool toggle) { return true; }

		public static void EndToggleGroup() {}

		public static Rect BeginHorizontal(params GUILayoutOption[] options) { return new Rect(); }

		public static Rect BeginHorizontal(GUIStyle style, params GUILayoutOption[] options) { return new Rect(); }

		public static void EndHorizontal() {}

		public static Rect BeginVertical(params GUILayoutOption[] options) { return new Rect(); }

		public static Rect BeginVertical(GUIStyle style, params GUILayoutOption[] options) { return new Rect(); }

		public static void EndVertical() {}

		public static Vector2 BeginScrollView(Vector2 scrollPosition, params GUILayoutOption[] options) { return Vector2.zero; }

		public static Vector2 BeginScrollView(Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, params GUILayoutOption[] options) { return Vector2.zero; }

		public static Vector2 BeginScrollView(Vector2 scrollPosition, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params GUILayoutOption[] options) { return Vector2.zero; }

		public static Vector2 BeginScrollView(Vector2 scrollPosition, GUIStyle style, params GUILayoutOption[] options) { return Vector2.zero; }

		public static Vector2 BeginScrollView(Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background, params GUILayoutOption[] options) { return Vector2.zero; }

		public static void EndScrollView() {}

		public static bool PropertyField(SerializedProperty property, params GUILayoutOption[] options) { return true; }

		public static bool PropertyField(SerializedProperty property, GUIContent label, params GUILayoutOption[] options) { return true; }

		public static bool PropertyField(SerializedProperty property, bool includeChildren, params GUILayoutOption[] options) { return true; }

		public static bool PropertyField(SerializedProperty property, GUIContent label, bool includeChildren, params GUILayoutOption[] options) { return true; }

		public static Rect GetControlRect(params GUILayoutOption[] options) { return new Rect(); }

		public static Rect GetControlRect(bool hasLabel, params GUILayoutOption[] options) { return new Rect(); }

		public static Rect GetControlRect(bool hasLabel, float height, params GUILayoutOption[] options) { return new Rect(); }

		public static Rect GetControlRect(bool hasLabel, float height, GUIStyle style, params GUILayoutOption[] options) { return new Rect(); }

		public static bool BeginFadeGroup(float value) { return true; }

		public static void EndFadeGroup() {}

		public static float Knob(Vector2 knobSize, float value, float minValue, float maxValue, string unit, Color backgroundColor, Color activeColor, bool showValue, params GUILayoutOption[] options) { return 0f; }
	}

	public sealed class EditorGUIUtility : GUIUtility {
		[Obsolete("This field is no longer used by any builtin controls. If passing this field to GetControlID, explicitly use the FocusType enum instead.", false)]
		public static FocusType native;

		public static float singleLineHeight = 16f;

		public static float standardVerticalSpacing = 16f;

		public static bool isProSkin = EditorGUIUtility.skinIndex == 1;

		internal static int skinIndex = 0;

		public static Texture2D whiteTexture = null;

		public static bool editingTextField = true;

		public static bool hierarchyMode = true;

	 	public static bool wideMode = true;

		public static float currentViewWidth = 0;

		public static float labelWidth = 0;

		public static float fieldWidth;

		public static float pixelsPerPoint;

		public static string SerializeMainMenuToString() { return null; }

		public static void SetMenuLocalizationTestMode(bool onoff) {}

		public static GUIContent IconContent(string name) { return null; }

		public static GUIContent IconContent(string name, string text) { return null; }

		public static GUIContent ObjectContent(UnityEngine.Object obj, Type type) { return null; }

		public static bool HasObjectThumbnail(Type objType) { return true; }

		public static void SetIconSize(Vector2 size) {}

		public static Vector2 GetIconSize() { return Vector2.zero; }

		public static Texture2D FindTexture(string name) { return null; }

		public static GUISkin GetBuiltinSkin(EditorSkin skin) { return null; }

		public static UnityEngine.Object LoadRequired(string path) { return null; }

		public static UnityEngine.Object Load(string path) { return null; }

		public static void PingObject(UnityEngine.Object obj) {}

		public static void PingObject(int targetInstanceID) {}

		public static void RenderGameViewCameras(RenderTexture target, int targetDisplay, Rect screenRect, Vector2 mousePosition, bool gizmos) {}

		private static void INTERNAL_CALL_RenderGameViewCameras(RenderTexture target, int targetDisplay, ref Rect screenRect, ref Vector2 mousePosition, bool gizmos) {}

		[Obsolete("RenderGameViewCameras is no longer supported. Consider rendering cameras manually.", true)]
		public static void RenderGameViewCameras(Rect cameraRect, bool gizmos, bool gui) {}

		[Obsolete("RenderGameViewCameras is no longer supported. Consider rendering cameras manually.", true)]
		public static void RenderGameViewCameras(Rect cameraRect, Rect statsRect, bool gizmos, bool gui) {}

		public static bool IsDisplayReferencedByCameras(int displayIndex) { return true; }

		public static void QueueGameViewInputEvent(Event evt) {}

		internal static void SetDefaultFont(Font font) {}

		private static GUIStyle GetStyle(string styleName) { return null; }

		[Obsolete("LookLikeControls and LookLikeInspector modes are deprecated. Use EditorGUIUtility.labelWidth and EditorGUIUtility.fieldWidth to control label and field widths.")]
		public static void LookLikeControls(float labelWidth) {}

		[Obsolete("LookLikeControls and LookLikeInspector modes are deprecated. Use EditorGUIUtility.labelWidth and EditorGUIUtility.fieldWidth to control label and field widths.")]
		public static void LookLikeControls() {}

		[Obsolete("LookLikeControls and LookLikeInspector modes are deprecated. Use EditorGUIUtility.labelWidth and EditorGUIUtility.fieldWidth to control label and field widths.")]
		public static void LookLikeControls(float labelWidth, float fieldWidth) {}

		[Obsolete("LookLikeControls and LookLikeInspector modes are deprecated.")]
		public static void LookLikeInspector() {}

		public static Event CommandEvent(string commandName) { return null; }

		public static void DrawColorSwatch(Rect position, Color color) {}

		public static void DrawCurveSwatch(Rect position, AnimationCurve curve, SerializedProperty property, Color color, Color bgColor) {}

		public static void DrawCurveSwatch(Rect position, AnimationCurve curve, SerializedProperty property, Color color, Color bgColor, Color topFillColor, Color bottomFillColor) {}

		public static void DrawCurveSwatch(Rect position, AnimationCurve curve, SerializedProperty property, Color color, Color bgColor, Color topFillColor, Color bottomFillColor, Rect curveRanges) {}

		public static void DrawCurveSwatch(Rect position, AnimationCurve curve, SerializedProperty property, Color color, Color bgColor, Rect curveRanges) {}

		public static void DrawRegionSwatch(Rect position, SerializedProperty property, SerializedProperty property2, Color color, Color bgColor, Rect curveRanges) {}

		public static void DrawRegionSwatch(Rect position, AnimationCurve curve, AnimationCurve curve2, Color color, Color bgColor, Rect curveRanges) {}

		[Obsolete("EditorGUIUtility.RGBToHSV is obsolete. Use Color.RGBToHSV instead (UnityUpgradable) -> [UnityEngine] UnityEngine.Color.RGBToHSV(*)", true)]
		public static void RGBToHSV(Color rgbColor, out float H, out float S, out float V) {H = 0; S = 0; V = 0;}

		[Obsolete("EditorGUIUtility.HSVToRGB is obsolete. Use Color.HSVToRGB instead (UnityUpgradable) -> [UnityEngine] UnityEngine.Color.HSVToRGB(*)", true)] 
		public static Color HSVToRGB(float H, float S, float V) { return Color.white; }

		[Obsolete("EditorGUIUtility.HSVToRGB is obsolete. Use Color.HSVToRGB instead (UnityUpgradable) -> [UnityEngine] UnityEngine.Color.HSVToRGB(*)", true)]
		public static Color HSVToRGB(float H, float S, float V, bool hdr) { return Color.white; }

		public static void AddCursorRect(Rect position, MouseCursor mouse) {}

		public static void AddCursorRect(Rect position, MouseCursor mouse, int controlID) {}

		public static void SetWantsMouseJumping(int wantz) {}

		public static void ShowObjectPicker<T>(UnityEngine.Object obj, bool allowSceneObjects, string searchFilter, int controlID) where T : UnityEngine.Object {}

		public static UnityEngine.Object GetObjectPickerObject() { return null; }

		public static int GetObjectPickerControlID() { return 0; }

		public static Rect PointsToPixels(Rect rect) { return new Rect(); }

		public static Rect PixelsToPoints(Rect rect) { return new Rect(); }

		public static Vector2 PointsToPixels(Vector2 position) { return Vector2.zero; }

		public static Vector2 PixelsToPoints(Vector2 position) { return Vector2.zero; }

		public static List<Rect> GetFlowLayoutedRects(Rect rect, GUIStyle style, float horizontalSpacing, float verticalSpacing, List<string> items) { return new List< Rect >(); }
	}

	public sealed class EditorGUI
	{
		public class DisabledGroupScope : GUI.Scope {
			public DisabledGroupScope(bool disabled) {}
			protected override void CloseScope() {}
		}

		public struct DisabledScope : IDisposable {
			public DisabledScope(bool disabled) {}
			public void Dispose() {}
		}
		public class ChangeCheckScope : GUI.Scope {
			public bool changed;
			public ChangeCheckScope() {}
			protected override void CloseScope() {}
		}
		public class PropertyScope : GUI.Scope
		{
			public GUIContent content;

			public PropertyScope(Rect totalPosition, GUIContent label, SerializedProperty property) {}

			protected override void CloseScope()
			{
				EditorGUI.EndProperty();
			}
		}

		public static bool showMixedValue;

		public static bool actionKey;

		public static int indentLevel;

		public static void LabelField(Rect position, string label) {}

		public static void LabelField(Rect position, string label, GUIStyle style) {}

		public static void LabelField(Rect position, GUIContent label) {}

		public static void LabelField(Rect position, GUIContent label, GUIStyle style) {}

		public static void LabelField(Rect position, string label, string label2) {}

		public static void LabelField(Rect position, string label, string label2, GUIStyle style) {}

		
		public static void LabelField(Rect position, GUIContent label, GUIContent label2) {}

		public static void LabelField(Rect position, GUIContent label, GUIContent label2, GUIStyle style) {}
		
		public static bool ToggleLeft(Rect position, string label, bool value) { return true; }

		public static bool ToggleLeft(Rect position, string label, bool value, GUIStyle labelStyle) { return true; }

		
		public static bool ToggleLeft(Rect position, GUIContent label, bool value) { return true; }

		public static bool ToggleLeft(Rect position, GUIContent label, bool value, GUIStyle labelStyle) { return true; }
		
		public static string TextField(Rect position, string text) { return null; }

		public static string TextField(Rect position, string text, GUIStyle style) { return null; }
		
		public static string TextField(Rect position, string label, string text) { return null; }

		public static string TextField(Rect position, string label, string text, GUIStyle style) { return null; }

		
		public static string TextField(Rect position, GUIContent label, string text) { return null; }

		public static string TextField(Rect position, GUIContent label, string text, GUIStyle style) { return null; }

		
		public static string DelayedTextField(Rect position, string text) { return null; }

		public static string DelayedTextField(Rect position, string text, GUIStyle style) { return null; }

		
		public static string DelayedTextField(Rect position, string label, string text) { return null; }

		public static string DelayedTextField(Rect position, string label, string text, GUIStyle style) { return null; }

		
		public static string DelayedTextField(Rect position, GUIContent label, string text) { return null; }

		public static string DelayedTextField(Rect position, GUIContent label, string text, GUIStyle style) { return null; }

		
		public static void DelayedTextField(Rect position, SerializedProperty property) {}

		public static void DelayedTextField(Rect position, SerializedProperty property, GUIContent label) {}

		
		public static string DelayedTextField(Rect position, int controlId, GUIContent label, string text) { return null; }

		public static string DelayedTextField(Rect position, int controlId, GUIContent label, string text, GUIStyle style) { return null; }

		
		public static string TextArea(Rect position, string text) { return null; }

		public static string TextArea(Rect position, string text, GUIStyle style) { return null; }

		
		public static void SelectableLabel(Rect position, string text) {}

		public static void SelectableLabel(Rect position, string text, GUIStyle style) {}

		
		public static string PasswordField(Rect position, string password) { return null; }

		public static string PasswordField(Rect position, string password, GUIStyle style) { return null; }

		
		public static string PasswordField(Rect position, string label, string password) { return null; }

		public static string PasswordField(Rect position, string label, string password, GUIStyle style) { return null; }

		
		public static string PasswordField(Rect position, GUIContent label, string password) { return null; }

		public static string PasswordField(Rect position, GUIContent label, string password, GUIStyle style) { return null; }

		
		public static float FloatField(Rect position, float value) { return 0; }

		public static float FloatField(Rect position, float value, GUIStyle style) { return 0; }

		
		public static float FloatField(Rect position, string label, float value) { return 0; }

		public static float FloatField(Rect position, string label, float value, GUIStyle style) { return 0; }

		
		public static float FloatField(Rect position, GUIContent label, float value) { return 0; }

		public static float FloatField(Rect position, GUIContent label, float value, GUIStyle style) { return 0; }

		
		public static float DelayedFloatField(Rect position, float value) { return 0; }

		public static float DelayedFloatField(Rect position, float value, GUIStyle style) { return 0; }

		
		public static float DelayedFloatField(Rect position, string label, float value) { return 0; }

		public static float DelayedFloatField(Rect position, string label, float value, GUIStyle style) { return 0; }

		
		public static float DelayedFloatField(Rect position, GUIContent label, float value) { return 0; }

		public static float DelayedFloatField(Rect position, GUIContent label, float value, GUIStyle style) { return 0; }

		
		public static void DelayedFloatField(Rect position, SerializedProperty property) {}

		public static void DelayedFloatField(Rect position, SerializedProperty property, GUIContent label) {}

		
		public static double DoubleField(Rect position, double value) { return 0; }

		public static double DoubleField(Rect position, double value, GUIStyle style) { return 0; }

		
		public static double DoubleField(Rect position, string label, double value) { return 0; }

		public static double DoubleField(Rect position, string label, double value, GUIStyle style) { return 0; }

		
		public static double DoubleField(Rect position, GUIContent label, double value) { return 0; }

		public static double DoubleField(Rect position, GUIContent label, double value, GUIStyle style) { return 0; }

		
		public static double DelayedDoubleField(Rect position, double value) { return 0; }

		public static double DelayedDoubleField(Rect position, double value, GUIStyle style) { return 0; }

		
		public static double DelayedDoubleField(Rect position, string label, double value) { return 0; }

		public static double DelayedDoubleField(Rect position, string label, double value, GUIStyle style) { return 0; }

		
		public static double DelayedDoubleField(Rect position, GUIContent label, double value) { return 0; }

		public static double DelayedDoubleField(Rect position, GUIContent label, double value, GUIStyle style) { return 0; }

		
		public static int IntField(Rect position, int value) { return 0; }

		public static int IntField(Rect position, int value, GUIStyle style) { return 0; }

		
		public static int IntField(Rect position, string label, int value) { return 0; }

		public static int IntField(Rect position, string label, int value, GUIStyle style) { return 0; }

		
		public static int IntField(Rect position, GUIContent label, int value) { return 0; }

		public static int IntField(Rect position, GUIContent label, int value, GUIStyle style) { return 0; }

		
		public static int DelayedIntField(Rect position, int value) { return 0; }

		public static int DelayedIntField(Rect position, int value, GUIStyle style) { return 0; }

		
		public static int DelayedIntField(Rect position, string label, int value) { return 0; }

		public static int DelayedIntField(Rect position, string label, int value, GUIStyle style) { return 0; }

		
		public static int DelayedIntField(Rect position, GUIContent label, int value) { return 0; }

		public static int DelayedIntField(Rect position, GUIContent label, int value, GUIStyle style) { return 0; }

		
		public static void DelayedIntField(Rect position, SerializedProperty property) {}

		public static void DelayedIntField(Rect position, SerializedProperty property, GUIContent label) {}

		
		public static long LongField(Rect position, long value) { return 0; }

		public static long LongField(Rect position, long value, GUIStyle style) { return 0; }

		
		public static long LongField(Rect position, string label, long value) { return 0; }

		public static long LongField(Rect position, string label, long value, GUIStyle style) { return 0; }

		
		public static long LongField(Rect position, GUIContent label, long value) { return 0; }

		public static long LongField(Rect position, GUIContent label, long value, GUIStyle style) { return 0; }

		
		public static int Popup(Rect position, int selectedIndex, string[] displayedOptions) { return 0; }

		public static int Popup(Rect position, int selectedIndex, string[] displayedOptions, GUIStyle style) { return 0; }

		
		public static int Popup(Rect position, int selectedIndex, GUIContent[] displayedOptions) { return 0; }

		public static int Popup(Rect position, int selectedIndex, GUIContent[] displayedOptions, GUIStyle style) { return 0; }

		
		public static int Popup(Rect position, string label, int selectedIndex, string[] displayedOptions) { return 0; }

		public static int Popup(Rect position, string label, int selectedIndex, string[] displayedOptions, GUIStyle style) { return 0; }

		
		public static int Popup(Rect position, GUIContent label, int selectedIndex, GUIContent[] displayedOptions) { return 0; }

		public static int Popup(Rect position, GUIContent label, int selectedIndex, GUIContent[] displayedOptions, GUIStyle style) { return 0; }

		
		public static Enum EnumPopup(Rect position, Enum selected) { return selected; }

		public static Enum EnumPopup(Rect position, Enum selected, GUIStyle style) { return selected; }

		
		public static Enum EnumPopup(Rect position, string label, Enum selected) { return selected; }

		public static Enum EnumPopup(Rect position, string label, Enum selected, GUIStyle style) { return selected; }

		
		public static Enum EnumPopup(Rect position, GUIContent label, Enum selected) { return selected; }

		public static Enum EnumPopup(Rect position, GUIContent label, Enum selected, GUIStyle style) { return selected; }

		
		public static Enum EnumMaskPopup(Rect position, string label, Enum selected) { return selected; }

		public static Enum EnumMaskPopup(Rect position, string label, Enum selected, GUIStyle style) { return selected; }

		
		public static Enum EnumMaskPopup(Rect position, GUIContent label, Enum selected) { return selected; }

		public static Enum EnumMaskPopup(Rect position, GUIContent label, Enum selected, GUIStyle style) { return selected; }

		public static int IntPopup(Rect position, int selectedValue, string[] displayedOptions, int[] optionValues) { return 0; }

		public static int IntPopup(Rect position, int selectedValue, string[] displayedOptions, int[] optionValues, GUIStyle style) { return 0; }

		
		public static int IntPopup(Rect position, int selectedValue, GUIContent[] displayedOptions, int[] optionValues) { return 0; }

		public static int IntPopup(Rect position, int selectedValue, GUIContent[] displayedOptions, int[] optionValues, GUIStyle style) { return 0; }

		
		public static int IntPopup(Rect position, GUIContent label, int selectedValue, GUIContent[] displayedOptions, int[] optionValues) { return 0; }

		public static int IntPopup(Rect position, GUIContent label, int selectedValue, GUIContent[] displayedOptions, int[] optionValues, GUIStyle style) { return 0; }

		
		public static void IntPopup(Rect position, SerializedProperty property, GUIContent[] displayedOptions, int[] optionValues) {}

		public static void IntPopup(Rect position, SerializedProperty property, GUIContent[] displayedOptions, int[] optionValues, GUIContent label) {}

		
		public static int IntPopup(Rect position, string label, int selectedValue, string[] displayedOptions, int[] optionValues) { return 0; }

		public static int IntPopup(Rect position, string label, int selectedValue, string[] displayedOptions, int[] optionValues, GUIStyle style) { return 0; }

		
		public static string TagField(Rect position, string tag) { return null; }

		public static string TagField(Rect position, string tag, GUIStyle style) { return null; }

		
		public static string TagField(Rect position, string label, string tag) { return null; }

		public static string TagField(Rect position, string label, string tag, GUIStyle style) { return null; }

		
		public static string TagField(Rect position, GUIContent label, string tag) { return null; }

		public static string TagField(Rect position, GUIContent label, string tag, GUIStyle style) { return null; }

		
		public static int LayerField(Rect position, int layer) { return 0; }

		public static int LayerField(Rect position, int layer, GUIStyle style) { return 0; }

		
		public static int LayerField(Rect position, string label, int layer) { return 0; }

		public static int LayerField(Rect position, string label, int layer, GUIStyle style) { return 0; }

		
		public static int LayerField(Rect position, GUIContent label, int layer) { return 0; }

		public static int LayerField(Rect position, GUIContent label, int layer, GUIStyle style) { return 0; }

		
		public static int MaskField(Rect position, GUIContent label, int mask, string[] displayedOptions) { return 0; }

		public static int MaskField(Rect position, GUIContent label, int mask, string[] displayedOptions, GUIStyle style) { return 0; }

		
		public static int MaskField(Rect position, string label, int mask, string[] displayedOptions) { return 0; }

		public static int MaskField(Rect position, string label, int mask, string[] displayedOptions, GUIStyle style) { return 0; }

		
		public static int MaskField(Rect position, int mask, string[] displayedOptions) { return 0; }

		public static int MaskField(Rect position, int mask, string[] displayedOptions, GUIStyle style) { return 0; }

		
		public static Enum EnumMaskField(Rect position, GUIContent label, Enum enumValue) { return enumValue; }

		public static Enum EnumMaskField(Rect position, GUIContent label, Enum enumValue, GUIStyle style) { return enumValue; }

		
		public static Enum EnumMaskField(Rect position, string label, Enum enumValue) { return enumValue; }

		public static Enum EnumMaskField(Rect position, string label, Enum enumValue, GUIStyle style) { return enumValue; }

		
		public static Enum EnumMaskField(Rect position, Enum enumValue) { return enumValue; }

		public static Enum EnumMaskField(Rect position, Enum enumValue, GUIStyle style) { return enumValue; }

		
		public static bool Foldout(Rect position, bool foldout, string content) { return true; }

		public static bool Foldout(Rect position, bool foldout, string content, GUIStyle style) { return true; }

		
		public static bool Foldout(Rect position, bool foldout, string content, bool toggleOnLabelClick) { return true; }

		public static bool Foldout(Rect position, bool foldout, string content, bool toggleOnLabelClick, GUIStyle style) { return true; }

		
		public static bool Foldout(Rect position, bool foldout, GUIContent content) { return true; }

		public static bool Foldout(Rect position, bool foldout, GUIContent content, GUIStyle style) { return true; }

		
		public static bool Foldout(Rect position, bool foldout, GUIContent content, bool toggleOnLabelClick) { return true; }

		public static bool Foldout(Rect position, bool foldout, GUIContent content, bool toggleOnLabelClick, GUIStyle style) { return true; }

		
		public static void HandlePrefixLabel(Rect totalPosition, Rect labelPosition, GUIContent label, int id) {}

		
		public static void HandlePrefixLabel(Rect totalPosition, Rect labelPosition, GUIContent label) {}

		public static void HandlePrefixLabel(Rect totalPosition, Rect labelPosition, GUIContent label, GUIStyle style) {}

		
		public static void DrawTextureAlpha(Rect position, Texture image, ScaleMode scaleMode) {}

		
		public static void DrawTextureAlpha(Rect position, Texture image) {}

		public static void DrawTextureAlpha(Rect position, Texture image, float imageAspect) {}

		
		public static void DrawTextureTransparent(Rect position, Texture image, ScaleMode scaleMode) {}

		
		public static void DrawTextureTransparent(Rect position, Texture image) {}

		public static void DrawTextureTransparent(Rect position, Texture image, float imageAspect) {}

		
		public static void DrawPreviewTexture(Rect position, Texture image, Material mat, ScaleMode scaleMode) {}

		
		public static void DrawPreviewTexture(Rect position, Texture image, Material mat) {}

		
		public static void DrawPreviewTexture(Rect position, Texture image) {}

		public static void DrawPreviewTexture(Rect position, Texture image, float imageAspect) {}

		
		public static float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 0; }

		
		public static float GetPropertyHeight(SerializedProperty property) { return 0; }

		public static float GetPropertyHeight(SerializedProperty property, bool includeChildren) { return 0; }

		
		public static bool PropertyField(Rect position, SerializedProperty property) { return true; }

		public static bool PropertyField(Rect position, SerializedProperty property, bool includeChildren) { return true; }

		public static bool PropertyField(Rect position, SerializedProperty property, GUIContent label) { return true; }

		public static bool PropertyField(Rect position, SerializedProperty property, GUIContent label, bool includeChildren) { return true; }

		public static void FocusTextInControl(string name) {}


		public static void BeginDisabledGroup(bool disabled) {}

		public static void EndDisabledGroup() {}



		public static void BeginChangeCheck() {}

		public static bool EndChangeCheck() { return true; }

		public static void DropShadowLabel(Rect position, string text) {}

		public static void DropShadowLabel(Rect position, GUIContent content) {}

		public static void DropShadowLabel(Rect position, string text, GUIStyle style) {}

		public static void DropShadowLabel(Rect position, GUIContent content, GUIStyle style) {}

		public static bool Toggle(Rect position, bool value) { return true; }

		public static bool Toggle(Rect position, string label, bool value) { return true; }

		public static bool Toggle(Rect position, bool value, GUIStyle style) { return true; }

		public static bool Toggle(Rect position, string label, bool value, GUIStyle style) { return true; }

		public static bool Toggle(Rect position, GUIContent label, bool value) { return true; }

		public static bool Toggle(Rect position, GUIContent label, bool value, GUIStyle style) { return true; }

		[Obsolete("Use PasswordField instead.")]
		public static string DoPasswordField(int id, Rect position, string password, GUIStyle style) { return null; }

		[Obsolete("Use PasswordField instead.")]
		public static string DoPasswordField(int id, Rect position, GUIContent label, string password, GUIStyle style) { return null; }

		public static float Slider(Rect position, float value, float leftValue, float rightValue) { return 0; }

		public static float Slider(Rect position, string label, float value, float leftValue, float rightValue) { return 0; }

		public static float Slider(Rect position, GUIContent label, float value, float leftValue, float rightValue) { return 0; }

		public static void Slider(Rect position, SerializedProperty property, float leftValue, float rightValue) {}

		public static void Slider(Rect position, SerializedProperty property, float leftValue, float rightValue, string label) {}

		public static void Slider(Rect position, SerializedProperty property, float leftValue, float rightValue, GUIContent label) {}

		public static int IntSlider(Rect position, int value, int leftValue, int rightValue) { return 0; }

		public static int IntSlider(Rect position, string label, int value, int leftValue, int rightValue) { return 0; }

		public static int IntSlider(Rect position, GUIContent label, int value, int leftValue, int rightValue) { return 0; }

		public static void IntSlider(Rect position, SerializedProperty property, int leftValue, int rightValue) {}

		public static void IntSlider(Rect position, SerializedProperty property, int leftValue, int rightValue, string label) {}

		public static void IntSlider(Rect position, SerializedProperty property, int leftValue, int rightValue, GUIContent label) {}


		[Obsolete("Switch the order of the first two parameters.")]
		public static void MinMaxSlider(GUIContent label, Rect position, ref float minValue, ref float maxValue, float minLimit, float maxLimit) {}

		public static void MinMaxSlider(Rect position, string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit) {}

		public static void MinMaxSlider(Rect position, GUIContent label, ref float minValue, ref float maxValue, float minLimit, float maxLimit) {}

		public static void MinMaxSlider(Rect position, ref float minValue, ref float maxValue, float minLimit, float maxLimit) {}


		public static void ObjectField(Rect position, SerializedProperty property) {}

		public static void ObjectField(Rect position, SerializedProperty property, GUIContent label) {}

		public static void ObjectField(Rect position, SerializedProperty property, Type objType) {}

		public static void ObjectField(Rect position, SerializedProperty property, Type objType, GUIContent label) {}



		public static UnityEngine.Object ObjectField(Rect position, UnityEngine.Object obj, Type objType, bool allowSceneObjects) { return null; }

		[Obsolete("Check the docs for the usage of the new parameter 'allowSceneObjects'.")]
		public static UnityEngine.Object ObjectField(Rect position, UnityEngine.Object obj, Type objType) { return null; }

		public static UnityEngine.Object ObjectField(Rect position, string label, UnityEngine.Object obj, Type objType, bool allowSceneObjects) { return null; }

		[Obsolete("Check the docs for the usage of the new parameter 'allowSceneObjects'.")]
		public static UnityEngine.Object ObjectField(Rect position, string label, UnityEngine.Object obj, Type objType) { return null; }

		public static UnityEngine.Object ObjectField(Rect position, GUIContent label, UnityEngine.Object obj, Type objType, bool allowSceneObjects) { return null; }



		[Obsolete("Check the docs for the usage of the new parameter 'allowSceneObjects'.")]
		public static UnityEngine.Object ObjectField(Rect position, GUIContent label, UnityEngine.Object obj, Type objType) { return null; }




		public static Rect IndentedRect(Rect source) { return new Rect(); }

		public static Vector2 Vector2Field(Rect position, string label, Vector2 value) { return Vector2.zero; }

		public static Vector2 Vector2Field(Rect position, GUIContent label, Vector2 value) { return Vector2.zero; }

		public static Vector3 Vector3Field(Rect position, string label, Vector3 value) { return Vector3.zero; }

		public static Vector3 Vector3Field(Rect position, GUIContent label, Vector3 value) { return Vector3.zero; }

		public static Vector4 Vector4Field(Rect position, string label, Vector4 value) { return Vector4.zero; }

		public static Vector4 Vector4Field(Rect position, GUIContent label, Vector4 value) { return Vector4.zero; }

		public static Rect RectField(Rect position, Rect value) { return new Rect(); }

		public static Rect RectField(Rect position, string label, Rect value) { return new Rect(); }

		public static Rect RectField(Rect position, GUIContent label, Rect value) { return new Rect(); }

		public static Bounds BoundsField(Rect position, Bounds value) { return new Bounds(); }

		public static Bounds BoundsField(Rect position, string label, Bounds value) { return new Bounds(); }

		public static Bounds BoundsField(Rect position, GUIContent label, Bounds value) { return new Bounds(); }

		public static void MultiFloatField(Rect position, GUIContent label, GUIContent[] subLabels, float[] values) {}

		public static void MultiFloatField(Rect position, GUIContent[] subLabels, float[] values) {}


		public static void MultiPropertyField(Rect position, GUIContent[] subLabels, SerializedProperty valuesIterator, GUIContent label) {}

		public static void MultiPropertyField(Rect position, GUIContent[] subLabels, SerializedProperty valuesIterator) {}




		public static Color ColorField(Rect position, Color value) { return Color.white; }


		public static Color ColorField(Rect position, string label, Color value) { return Color.white; }

		public static Color ColorField(Rect position, GUIContent label, Color value) { return Color.white; }


		public static Color ColorField(Rect position, GUIContent label, Color value, bool showEyedropper, bool showAlpha, bool hdr, ColorPickerHDRConfig hdrConfig) { return Color.white; }


		public static AnimationCurve CurveField(Rect position, AnimationCurve value) { return null; }

		public static AnimationCurve CurveField(Rect position, string label, AnimationCurve value) { return null; }

		public static AnimationCurve CurveField(Rect position, GUIContent label, AnimationCurve value) { return null; }

		public static AnimationCurve CurveField(Rect position, AnimationCurve value, Color color, Rect ranges) { return null; }

		public static AnimationCurve CurveField(Rect position, string label, AnimationCurve value, Color color, Rect ranges) { return null; }

		public static AnimationCurve CurveField(Rect position, GUIContent label, AnimationCurve value, Color color, Rect ranges) { return null; }

		public static void CurveField(Rect position, SerializedProperty property, Color color, Rect ranges) {}

		public static void CurveField(Rect position, SerializedProperty property, Color color, Rect ranges, GUIContent label) {}


		public static void InspectorTitlebar(Rect position, UnityEngine.Object[] targetObjs) {}

		public static bool InspectorTitlebar(Rect position, bool foldout, UnityEngine.Object targetObj, bool expandable) { return true; }

		public static bool InspectorTitlebar(Rect position, bool foldout, UnityEngine.Object[] targetObjs, bool expandable) { return true; }

		public static void ProgressBar(Rect position, float value, string text) {}

		public static void HelpBox(Rect position, string message, MessageType type) {}

		public static Rect PrefixLabel(Rect totalPosition, GUIContent label) { return new Rect(); }

		public static Rect PrefixLabel(Rect totalPosition, GUIContent label, GUIStyle style) { return new Rect(); }

		public static Rect PrefixLabel(Rect totalPosition, int id, GUIContent label) { return new Rect(); }

		public static Rect PrefixLabel(Rect totalPosition, int id, GUIContent label, GUIStyle style) { return new Rect(); }

		public static GUIContent BeginProperty(Rect totalPosition, GUIContent label, SerializedProperty property) { return null; }

		public static void EndProperty() {}

		public static float GetPropertyHeight(SerializedPropertyType type, GUIContent label) { return 0f; }

		public static void DrawRect(Rect rect, Color color) {}
	}
}
#endif
