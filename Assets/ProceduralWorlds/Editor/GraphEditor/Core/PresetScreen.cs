using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	public abstract class PresetScreen 
	{
		public class PresetCell
		{
			public Texture2D		texture;
			public string			name;
			public string			graphPartFile;
			public Action			callback;
			public bool				enabled;

			public PresetCellList	childs;
		}
	
		public class PresetCellList : List< PresetCell >
		{
			public string	header;

			public void Add(string name, Texture2D texture, string graphPartFile, bool enabled = true, PresetCellList childList = null)
			{
				PresetCell	pc = new PresetCell
				{
					texture = texture,
					name = name,
					graphPartFile = graphPartFile,
					enabled = enabled,
					childs = childList
				};
	
				this.Add(pc);
			}

			public void Add(string header)
			{
				this.header = header;
			}
		}
		
		public readonly int				maxColumnCells = 3;
		
		protected BaseGraph				currentGraph;
		protected List< int >			selectedIndices = new List< int >();
		protected List< string >		graphPartFiles = new List< string >();
		protected Vector2[]				scrollBars;
	
		[System.NonSerialized]
		PresetCellList					presetList = null;
		
		//scroll position on the preset screen
		Vector2							presetScrollPos;
		
		int								columns;
	
		GUIStyle						buttonStyle;
		GUIStyle						selectedButtonStyle;
	
		protected void LoadPresetList(PresetCellList presets)
		{
			presetList = presets;

			UpdateColumnCount();
			UpdateSelectedList();
			UpdateGraphPartFiles();
		}

		protected void LoadStyle()
		{
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

			scrollBars = new Vector2[columns + 1];
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

		void UpdateGraphPartFiles()
		{
			int i = 0;

			PresetCellList currentList = presetList;
			graphPartFiles.Clear();
			while (currentList != null)
			{
				int index = selectedIndices[i];
				var cell = currentList[index];

				if (!String.IsNullOrEmpty(cell.graphPartFile))
					graphPartFiles.Add(cell.graphPartFile);
				
				currentList = cell.childs;
				
				selectedIndices.Add(index);
				i++;
			}
		}
	
		void DefaultDrawHeader(string header)
		{
			EditorGUILayout.BeginVertical();
	
			var headerSize = Styles.header.CalcSize(new GUIContent(header));
	
			Rect headerRect = EditorGUILayout.GetControlRect(false, headerSize.y);
	
			EditorGUI.LabelField(headerRect, header, Styles.header);
	
			EditorGUILayout.EndVertical();
		}
	
		bool DefaultDrawCell(PresetCell cell, bool selected, int columnIndex)
		{
			bool pressed = false;

			EditorGUILayout.BeginVertical();
			{
				EditorGUI.BeginDisabledGroup(!cell.enabled);
	
				GUIContent content = new GUIContent(cell.name, cell.texture);
				if (GUILayout.Button(content, (selected) ? selectedButtonStyle : buttonStyle, GUILayout.Width(200), GUILayout.Height(200)))
				{
					pressed = true;
					if (cell.callback != null)
						cell.callback();
				}
	
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndVertical();

			return pressed;
		}

		int DrawColumn(PresetCellList presetList, int columnIndex)
		{
			int newSelectedIndex = selectedIndices[columnIndex];

			EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			scrollBars[columnIndex] = EditorGUILayout.BeginScrollView(scrollBars[columnIndex], GUILayout.ExpandWidth(true));
			{
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.FlexibleSpace();
					EditorGUILayout.LabelField(presetList.header, EditorStyles.whiteBoldLabel);
					GUILayout.FlexibleSpace();
				}
				EditorGUILayout.EndHorizontal();

				int i = 0;
				foreach (var preset in presetList)
				{
					if (DefaultDrawCell(preset, i == newSelectedIndex, columnIndex))
						newSelectedIndex = i;
					i++;
				}
			}
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();

			return newSelectedIndex;
		}
	
		void DrawBoard()
		{
			var currentList = presetList;
			int i = 0;

			while (currentList != null)
			{
				int newIndex = DrawColumn(currentList, i);
				if (newIndex != selectedIndices[i])
				{
					selectedIndices[i] = newIndex;
					UpdateGraphPartFiles();
				}
				
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
	
		public BaseGraph Draw(Rect window, BaseGraph graph)
		{
			EditorGUI.DrawRect(new Rect(0, 0, window.width, window.height), ColorTheme.defaultBackgroundColor);

			currentGraph = graph;
	
			presetScrollPos = EditorGUILayout.BeginScrollView(presetScrollPos, GUILayout.ExpandHeight(true));
			{
				// EditorGUILayout.LabelField("Procedural Worlds");
				
				EditorGUILayout.BeginHorizontal();
				{
					DrawBoard();
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
			
			return graph;
		}
	
		public abstract void OnBuildPressed();
	}
}