using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Editor
{
	public class ResizableSplitView : IPWLayoutSeparator
	{
	
		bool				resize = false;
		Rect				availableRect;
		int					internHandlerPosition;
		float				lastMouseX = -1;
		bool				vertical;

		PWGraphEditor		graphEditor;

		PWLayoutSettings	layoutSettings;
	
		[SerializeField]
		Rect				savedRect;
	
		[System.NonSerialized]
		bool				first = true;
	
		Event				e { get { return Event.current; } }

		public ResizableSplitView(bool vertical)
		{
			this.vertical = vertical;
		}
	
		public Rect Begin()
		{
			Rect tmpRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			
			internHandlerPosition = (int)layoutSettings.separatorPosition;
	
			//if we are in the first frame, provide the true width value for layout (the only information we have currently)
			if (first)
			{
				availableRect = new Rect(0, 0, internHandlerPosition, 0);
				savedRect = new Rect(0, 0, internHandlerPosition, 0);
			}
	
			if (e.type == EventType.Repaint)
				availableRect = tmpRect;
	
			Rect beginRect = EditorGUILayout.BeginVertical(GUILayout.Width(internHandlerPosition), GUILayout.ExpandHeight(true));
			if (e.type == EventType.Repaint)
				savedRect = beginRect;
	
			first = false;
	
			return savedRect;
		}

		public void Initialize(PWGraphEditor graphEditor)
		{
			this.graphEditor = graphEditor;
		}

		public Rect Split()
		{
			EditorGUILayout.EndVertical();
			
			//left bar separation and resize:
			Rect handleRect = new Rect(internHandlerPosition - 1, availableRect.y, layoutSettings.separatorWidth, availableRect.height);
			Rect handleCatchRect = new Rect(internHandlerPosition - 1, availableRect.y, 6f, availableRect.height);
			EditorGUI.DrawRect(handleRect, Color.white);
			EditorGUIUtility.AddCursorRect(handleCatchRect, MouseCursor.ResizeHorizontal);
	
			if (Event.current.type == EventType.MouseDown && handleCatchRect.Contains(Event.current.mousePosition))
				resize = true;
			if (lastMouseX != -1 && resize)
				layoutSettings.separatorPosition += Event.current.mousePosition.x - lastMouseX;
			if (Event.current.rawType == EventType.MouseUp)
				resize = false;
			lastMouseX = Event.current.mousePosition.x;
			internHandlerPosition = (int)Mathf.Clamp(layoutSettings.separatorPosition, layoutSettings.minWidth, layoutSettings.maxWidth);
			layoutSettings.separatorPosition = Mathf.Clamp(layoutSettings.separatorPosition, layoutSettings.minWidth, layoutSettings.maxWidth);
	
			if (resize && Event.current.isMouse)
				Event.current.Use();
	
			return new Rect(internHandlerPosition + 3, availableRect.y, availableRect.width - internHandlerPosition, availableRect.height);
		}

		public Rect End()
		{
			EditorGUILayout.EndHorizontal();
			
			//TODO: rect
			return new Rect();
		}

		public void UpdateLayoutSettings(PWLayoutSettings layoutSettings)
		{
			this.layoutSettings = layoutSettings;
			layoutSettings.vertical = vertical;
		}
	}
}