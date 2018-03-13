using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ProceduralWorlds.Editor
{
	public class PWGUISkin : IDisposable
	{
		GUISkin	savedSkin;

		static PWGUISkin	instance;
		static GUISkin		proceduralWorldSkin;

		//private constructor so the class can't be instatiated somewhere else than in the static Get
		private PWGUISkin() {}
		
		public static PWGUISkin Get()
		{
			if (instance == null)
			{
				instance = new PWGUISkin();
				proceduralWorldSkin = Resources.Load< GUISkin >("PWEditorSkin");
			}
			
			instance.Init();

			return instance;
		}

		public void Init()
		{
			savedSkin = GUI.skin;
			GUI.skin = proceduralWorldSkin;
		}

		public void Dispose()
		{
			if (savedSkin != null)
				GUI.skin=  savedSkin;
		}
	}
}