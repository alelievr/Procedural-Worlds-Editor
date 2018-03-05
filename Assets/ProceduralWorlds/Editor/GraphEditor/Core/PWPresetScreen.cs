using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using PW.Core;

namespace PW.Editor
{
	public abstract class PWPresetScreen 
	{
		public class PresetCell
		{
			public Texture2D		texture;
			public string			name;
			public string			header;
			public Action			callback;
			public bool				enabled;

			public PresetCellList	childs;
		}
	
		public class PresetCellList : List< PresetCell >
		{
			public void Add(string header, Texture2D texture, string name, Action callback, bool enabled = true, PresetCellList childList = null)
			{
				PresetCell	pc = new PresetCell
				{
					texture = texture,
					name = name,
					header = header,
					callback = callback,
					enabled = enabled,
					childs = childList
				};
	
				this.Add(pc);
			}
		}
		
		public Func< PresetCell, bool, bool > onDrawCell;
		public Action< string >			onDrawHeader;
	
		public readonly int				maxColumnCells = 3;
		
		protected PWGraph				currentGraph;
	
		[System.NonSerialized]
		PresetCellList					presetList = null;
		
		//scroll position on the preset screen
		Vector2							presetScrollPos;
		//id for the object picker
		int								currentPickerWindow;
		
		int								columns = 4;
		List< int >						selectedIndices = new List< int >();
	
		GUIStyle						buttonStyle;
		GUIStyle						selectedButtonStyle;
	
		public PWPresetScreen()
		{
			onDrawHeader = DefaultDrawHeader;
			onDrawCell = DefaultDrawCell;
		}
	
		protected void LoadPresetList(PresetCellList presets)
		{
			presetList = presets;

			UpdateColumnCount();
			UpdateSelectedList();
	
			using (DefaultGUISkin.Get())
			{
				buttonStyle = new GUIStyle("LargeButton");
				buttonStyle.imagePosition = ImagePosition.ImageAbove;
				buttonStyle.alignment = TextAnchor.MiddleCenter;
				buttonStyle.fontSize = 12;
				selectedButtonStyle = new GUIStyle(buttonStyle);
				selectedButtonStyle.normal.background = selectedButtonStyle.focused.background;
			}
		}

		void UpdateColumnCount()
		{
			columns = 0;
			Stack< PresetCellList > currentPresetLists = new Stack< PresetCellList >();

			currentPresetLists.Push(presetList);
			while (currentPresetLists.Count != 0)
			{
				columns = Mathf.Max(columns, currentPresetLists.Count);

				if (columns > 10)
					break ;

				var cl = currentPresetLists.Pop();

				foreach (var c in cl)
					if (c.childs != null)
						currentPresetLists.Push(c.childs);
			}
		}

		void UpdateSelectedList()
		{
			int i = 0;

			selectedIndices.Clear();
			PresetCellList currentList = presetList;
			while (currentList != null)
			{
				int index = currentList.FindIndex(l => l.enabled);

				if (index == -1)
					return ;
				
				currentList = currentList[index].childs;
				
				selectedIndices.Add(index);
				i++;
			}
		}
	
		void DefaultDrawHeader(string header)
		{
			EditorGUILayout.BeginVertical();
	
			var headerSize = PWStyles.header.CalcSize(new GUIContent(header));
	
			Rect headerRect = EditorGUILayout.GetControlRect(false, headerSize.y);
	
			EditorGUI.LabelField(headerRect, header, PWStyles.header);
	
			EditorGUILayout.EndVertical();
		}
	
		bool DefaultDrawCell(PresetCell cell, bool selected)
		{
			bool pressed = false;

			EditorGUILayout.BeginVertical();
			{
				GUILayout.FlexibleSpace();
				EditorGUI.BeginDisabledGroup(!cell.enabled);
	
				GUIContent content = new GUIContent(cell.name, cell.texture);
				if (GUILayout.Button(content, (selected) ? selectedButtonStyle : buttonStyle, GUILayout.Width(200), GUILayout.Height(200)))
				{
					pressed = true;
					cell.callback();
				}
	
				EditorGUI.EndDisabledGroup();
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndVertical();

			return pressed;
		}

		int DrawColumn(PresetCellList presetList, int selectedIndex)
		{
			int newSelectedIndex = selectedIndex;
			EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			{
				int i = 0;
				foreach (var preset in presetList)
				{
					if (onDrawCell(preset, i == selectedIndex))
						newSelectedIndex = i;
					i++;
				}
			}
			EditorGUILayout.EndVertical();

			return newSelectedIndex;
		}
	
		void DrawBoard(PWGraph graph)
		{
			var currentList = presetList;
			int i = 0;

			while (currentList != null)
			{
				selectedIndices[i] = DrawColumn(currentList, selectedIndices[i]);
				
				currentList = currentList[selectedIndices[i]].childs;
				i++;

				if (i > selectedIndices.Count)
					break ;
			}

			//Build button:
			EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Build", GUILayout.ExpandWidth(true)))
				OnBuildPressed();
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndVertical();
		}
	
		public PWGraph Draw(Rect window, PWGraph graph)
		{
			EditorGUI.DrawRect(new Rect(0, 0, window.width, window.height), PWColorTheme.defaultBackgroundColor);

			currentGraph = graph;
	
			presetScrollPos = EditorGUILayout.BeginScrollView(presetScrollPos, GUILayout.ExpandHeight(true));
			{
				// EditorGUILayout.LabelField("Procedural Worlds");
				
				EditorGUILayout.BeginHorizontal();
				{
					DrawBoard(graph);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
			
			return graph;
		}
	
		public abstract void OnBuildPressed();
	}
}