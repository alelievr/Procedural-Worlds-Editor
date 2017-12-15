using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class DefaultGUISkin : IDisposable
{
	GUISkin		savedSkin;

	public DefaultGUISkin()
	{
		savedSkin = GUI.skin;
		GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
	}

    public void Dispose()
    {
		if (savedSkin != null)
			GUI.skin = savedSkin;
    }
}
