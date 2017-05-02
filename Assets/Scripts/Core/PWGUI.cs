using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PWGUI {

	public static void ColorPicker(Rect iconRect, bool displayColorPreview)
	{
		//TODO: draw the colorPicker field.
	}

	public static void Slider(Rect sliderRect, ref float value, float min, float max, float pad = 0f)
	{

	}

	public static void IntSlider(Rect intSliderRect, ref int value, int min, int max, int padd = 1)
	{
		float		v = value;
		Slider(intSliderRect, ref v, min, max, padd);
		value = (int)v;
	}

}
