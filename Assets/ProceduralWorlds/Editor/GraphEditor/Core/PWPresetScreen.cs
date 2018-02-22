using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using PW.Core;

namespace PW.Editor
{
	public class PWPresetScreen 
	{
		public class PresetCell
		{
			public Texture2D	texture;
			public string		name;
			public string		header;
			public Action		callback;
			public bool			enabled;
		}
	
		public class PresetCellList : List< PresetCell >
		{
			public void Add(string header, Texture2D texture, string name, Action callback, bool enabled = true)
			{
				PresetCell	pc = new PresetCell{texture = texture, name = name, header = header, callback = callback, enabled = enabled};
	
				this.Add(pc);
			}
		}
		
		public Action< PresetCell >		onDrawCell;
		public Action< string >			onDrawHeader;
	
		public readonly int				maxColumnCells = 3;
	
		[System.NonSerialized]
		PresetCellList					presetCellList = null;
		
		//scroll position on the preset screen
		Vector2							presetScrollPos;
		//id for the object picker
		int								currentPickerWindow;
		PWGraph							currentGraph;
	
		GUIStyle						buttonStyle;
	
		public PWPresetScreen()
		{
			onDrawHeader = DefaultDrawHeader;
			onDrawCell = DefaultDrawCell;
		}
	
		protected void LoadPresetList(PresetCellList presets)
		{
			presetCellList = presets;
	
			buttonStyle = new GUIStyle("button");
			buttonStyle.imagePosition = ImagePosition.ImageAbove;
			buttonStyle.alignment = TextAnchor.MiddleCenter;
			buttonStyle.fontSize = 12;
		}
	
		void DefaultDrawHeader(string header)
		{
			EditorGUILayout.BeginVertical();
	
			var headerSize = PWStyles.header.CalcSize(new GUIContent(header));
	
			Rect headerRect = EditorGUILayout.GetControlRect(false, headerSize.y);
	
			EditorGUI.LabelField(headerRect, header, PWStyles.header);
	
			EditorGUILayout.EndVertical();
		}
	
		void DefaultDrawCell(PresetCell cell)
		{
			EditorGUILayout.BeginVertical();
			{
				GUILayout.FlexibleSpace();
				EditorGUI.BeginDisabledGroup(!cell.enabled);
	
				GUIContent content = new GUIContent(cell.name, cell.texture);
				if (GUILayout.Button(content, buttonStyle, GUILayout.Width(200), GUILayout.Height(200)))
				{
					currentGraph.presetChoosed = true;
					cell.callback();
					currentGraph.UpdateComputeOrder();
				}
	
				EditorGUI.EndDisabledGroup();
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndVertical();
		}
	
		PWGraph DrawGraphInput(PWGraph graph)
		{
			currentGraph = graph;
	
			if (GUILayout.Button("Load graph"))
			{
				currentPickerWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
				EditorGUIUtility.ShowObjectPicker< PWGraph >(graph, false, "", currentPickerWindow);
			}
			
			if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow)
			{
				UnityEngine.Object selected = null;
				selected = EditorGUIUtility.GetObjectPickerObject();
				if (selected != null)
					graph = (PWGraph)selected;
			}
	
			return graph;
		}
	
		void DrawSelector(PWGraph graph)
		{
			GUILayout.FlexibleSpace();
			EditorGUILayout.BeginVertical();
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical();
			{
				GUILayout.FlexibleSpace();
	
				int i = 0;
				string lastheader = null;
	
				EditorGUILayout.BeginHorizontal();
				
				foreach (var presetCell in presetCellList)
				{
					if (lastheader != presetCell.header)
					{
						EditorGUILayout.EndHorizontal();
						onDrawHeader(presetCell.header);
						EditorGUILayout.BeginHorizontal();
						i = 0;
					}
					
					if (i != 0 && (i % maxColumnCells) == 0)
					{
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginHorizontal();
					}
	
					onDrawCell(presetCell);
	
					lastheader = presetCell.header;
					i++;
				}
				
				EditorGUILayout.EndHorizontal();
	
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical();
			EditorGUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		}
	
		public PWGraph Draw(Rect window, PWGraph graph)
		{
			EditorGUI.DrawRect(new Rect(0, 0, window.width, window.height), PWColorTheme.defaultBackgroundColor);
	
			presetScrollPos = EditorGUILayout.BeginScrollView(presetScrollPos);
			{
				EditorGUILayout.LabelField("Procedural Worlds");
				
				//Load graph button:
				EditorGUILayout.BeginHorizontal();
				{
					var newGraph = DrawGraphInput(graph);
	
					if (newGraph.GetType() == graph.GetType())
						graph = newGraph;
				}
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				{
					DrawSelector(graph);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
			
			return graph;
		}
	
	}
}