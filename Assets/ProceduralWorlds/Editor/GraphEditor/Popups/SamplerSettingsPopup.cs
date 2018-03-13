using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using ProceduralWorlds;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	public class SamplerSettingsPopup : Popup
	{
		Gradient		gradient;
		FilterMode		filterMode;
		Texture			texture;
		bool			debug;
		public static bool	update { get; private set; }
		
		[System.NonSerializedAttribute]
		static MethodInfo			gradientField;
		
		SerializableGradient		oldGradient;
		bool						needUpdate;
	
		public static void OpenPopup(PWGUISettings guiSettings)
		{
			var popup = Popup.OpenPopup< SamplerSettingsPopup >(guiSettings.GetHashCode());

			popup.name = "Sampler settings";
			popup.gradient = guiSettings.gradient;
			popup.filterMode = guiSettings.filterMode;
			popup.texture = guiSettings.texture;
			popup.debug = guiSettings.debug;
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
				SendUpdate("SamplerSettingsUpdate");

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

		public static void UpdateDatas(PWGUISettings settings)
		{
			var popup = FindPopup< SamplerSettingsPopup >();

			if (popup == null)
				return ;

			settings.gradient = popup.gradient;
			settings.serializableGradient = (SerializableGradient)settings.gradient;
			settings.filterMode = popup.filterMode;
			settings.debug = popup.debug;
		}
	}
}