using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using PW;
using PW.Core;

namespace PW.Editor
{
	public class PWSamplerSettingsPopup : PWPopup
	{
		public static Gradient		gradient;
		public static FilterMode	filterMode;
		public static Texture		texture;
		public static bool			debug;
		public static bool			update;
		
		[System.NonSerializedAttribute]
		static MethodInfo			gradientField;
		
		SerializableGradient		oldGradient;
		bool						needUpdate = false;
	
		public static void OpenPopup(PWGUISettings guiSettings)
		{
			var popup = PWPopup.OpenPopup< PWSamplerSettingsPopup >();

			popup.name = "Sampler settings";
			gradient = guiSettings.gradient;
			filterMode = guiSettings.filterMode;
			texture = guiSettings.texture;
			debug = guiSettings.debug;
			controlId = guiSettings.GetHashCode();
		}

		protected override void OnGUIEnable()
		{
		}

		protected override void GUIStart()
		{
			gradientField = typeof(EditorGUILayout).GetMethod(
				"GradientField",
				BindingFlags.NonPublic | BindingFlags.Static,
				null,
				new Type[] { typeof(string), typeof(Gradient), typeof(GUILayoutOption[]) },
				null
			);
		}

		protected override void GUIUpdate()
		{
			EditorGUI.BeginChangeCheck();
			{
				EditorGUILayout.BeginVertical();
				{
					EditorGUI.BeginChangeCheck();
					filterMode = (FilterMode)EditorGUILayout.EnumPopup(filterMode);
					if (EditorGUI.EndChangeCheck())
						texture.filterMode = filterMode;
					gradient = (Gradient)gradientField.Invoke(null, new object[] {"", gradient, null});
					if (oldGradient != null && !gradient.Compare(oldGradient))
						needUpdate = true;
					if (e.type == EventType.Repaint)
						update = false;
				}
				EditorGUILayout.EndVertical();
				
				if (e.type == EventType.KeyDown)
				{
					if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Escape)
						e.Use();
				}
	
				EditorGUIUtility.labelWidth = 100;
				debug = EditorGUILayout.Toggle("debug", debug);
			}
			if (EditorGUI.EndChangeCheck())
				SendUpdate("SamplerSettingsUpdate");
				
			if (needUpdate && e.type != EventType.Layout)
			{
				GUI.changed = true;
				needUpdate = false;

				//for an unknown reason, EditorWindow.SendEvent dooes not works with gradient field
				//so here is a workaround to update the main window:
				if (windowToUpdate != null)
				{
					update = true;
					windowToUpdate.Repaint();
				}
			}

			oldGradient = (SerializableGradient)gradient;
		}
	}
}