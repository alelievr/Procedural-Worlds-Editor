﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;
using System;
using Random = UnityEngine.Random;

using System.Linq;
namespace ProceduralWorlds.Editor
{
	public class PWLayout
	{
		static Event					e { get { return Event.current; } }

		BaseGraphEditor					graphEditor;

		BaseGraph							oldGraph;

		Stack< PWLayoutOrientation >	currentOrientation = new Stack< PWLayoutOrientation >();

		List< PWLayoutSeparator >		loadedSeparators = new List< PWLayoutSeparator >();
		List< PWLayoutPanel >			loadedPanels = new List< PWLayoutPanel >();

		List< Action >					layoutActions = new List< Action >();
		List< Rect >					layoutRects = new List< Rect >();

		List< PWLayoutSetting >			savedDefaultSettings = new List< PWLayoutSetting >();

		//Private constructor so the only way to create an instance of this class is PWLayoutFactory
		public PWLayout(BaseGraphEditor graphEditor)
		{
			this.graphEditor = graphEditor;
		}

		public void DrawLayout()
		{
			if (oldGraph != null && oldGraph != graphEditor.graph && graphEditor.graph != null)
				UpdateLayoutSettings(graphEditor.graph.layoutSettings);

			layoutRects.Clear();
			foreach (var layoutAction in layoutActions)
				layoutAction();

			oldGraph = graphEditor.graph;
		}

		public void BeginHorizontal()
		{
			currentOrientation.Push(PWLayoutOrientation.Horizontal);
			layoutActions.Add(() => EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true)));
		}
		
		public void EndHorizontal()
		{
			layoutActions.Add(() => EditorGUILayout.EndHorizontal());
			currentOrientation.Pop();
		}

		public void BeginVertical()
		{
			currentOrientation.Push(PWLayoutOrientation.Vertical);
			layoutActions.Add(() => EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)));
		}
		
		public void EndVertical()
		{
			layoutActions.Add(() => EditorGUILayout.EndVertical());
			currentOrientation.Pop();
		}

		void AddPanel(PWLayoutSeparator sep, PWLayoutPanel panel)
		{
			layoutActions.Add(() => {
				Rect r = sep.Begin();
				layoutRects.Add(r);
				panel.Draw(r);
				sep.End();
				layoutRects.Add(sep.GetSeparatorRect());
			});
			loadedSeparators.Add(sep);
			loadedPanels.Add(panel);
		}

		public void ResizablePanel(PWLayoutSetting defaultSetting, PWLayoutPanel panel)
		{
			var sep = new ResizablePanelSeparator(currentOrientation.Peek());
			sep.UpdateLayoutSetting(defaultSetting);
			AddPanel(sep, panel);

			savedDefaultSettings.Add(defaultSetting.Clone(null));
		}

		public void AutoSizePanel(PWLayoutSetting defaultSetting, PWLayoutPanel panel)
		{
			var sep = new StaticPanelSeparator(currentOrientation.Peek());
			sep.UpdateLayoutSetting(defaultSetting);
			AddPanel(sep, panel);
		}

		public void UpdateLayoutSettings(PWLayoutSettings layoutSettings)
		{
			int		index = 0;

			foreach (var sep in loadedSeparators)
			{
				if (layoutSettings.settings.Count <= index)
					layoutSettings.settings.Add(new PWLayoutSetting());

				var layoutSetting = layoutSettings.settings[index];
				var newLayout = sep.UpdateLayoutSetting(layoutSetting);
				if (newLayout != null)
					layoutSettings.settings[index] = newLayout;
				
				index++;
			}
		}

		public T GetPanel< T >() where T : PWLayoutPanel
		{
			return loadedPanels.FirstOrDefault(p => p is T) as T;
		}

		public List< Rect> GetRects()
		{
			return layoutRects;
		}

	}
}