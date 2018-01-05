using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

namespace PW.Core
{
	public class PWSamplerSettingsPopup : PWPopup
	{
		public static Gradient		gradient;
		public static FilterMode	filterMode;
		public static Texture		texture;
		public static bool			debug;
		public static bool			update;
		
		[System.NonSerializedAttribute]
		static MethodInfo	gradientField;
		
		SerializableGradient		oldGradient;
		bool						needUpdate = false;
	
		public static void OpenPopup(Gradient gradient, FilterMode filterMode, Texture texture, bool debug = false)
		{
			PWPopup.OpenPopup< PWSamplerSettingsPopup >();

			PWSamplerSettingsPopup.gradient = gradient;
			PWSamplerSettingsPopup.filterMode = filterMode;
			PWSamplerSettingsPopup.texture = texture;
			PWSamplerSettingsPopup.debug = debug;
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

			oldGradient = (SerializableGradient)gradient;
		}
	}
}