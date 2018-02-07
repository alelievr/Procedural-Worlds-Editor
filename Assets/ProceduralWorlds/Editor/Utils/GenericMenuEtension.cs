using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public static class GenericMenuExtension
{
	public static void AddItemState(this GenericMenu menu, string content, bool enabled, Action callback, bool on = false)
	{
		AddItemState(menu, new GUIContent(content), enabled, callback, on);
	}
	
	public static void AddItemState(this GenericMenu menu, GUIContent content, bool enabled, Action callback, bool on = false)
	{
		if (enabled)
			menu.AddItem(new GUIContent(content), on, () => callback());
		else
			menu.AddDisabledItem(new GUIContent(content));
	}
}
