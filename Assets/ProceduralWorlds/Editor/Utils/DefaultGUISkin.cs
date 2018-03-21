using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace ProceduralWorlds.Editor
{
	public class DefaultGUISkin : IDisposable
	{
		GUISkin		savedSkin;
	
		static DefaultGUISkin	defaultGUISkin;
	
		//private constructor so the class can't be instatiated somewhere else than in the static Get
		private DefaultGUISkin() {}
	
		public static DefaultGUISkin Get()
		{
			if (defaultGUISkin == null)
				defaultGUISkin = new DefaultGUISkin();
			
			defaultGUISkin.Init();
			
			return defaultGUISkin;
		}
	
		public void Init()
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
}