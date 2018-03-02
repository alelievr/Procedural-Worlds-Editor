using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Editor
{
	public class ResizablePanelSeparator : PWLayoutSeparator
	{
		bool				debug = false;
	
		bool				resize = false;
		Rect				availableRect;
		int					internHandlerPosition;
		float				lastMouseX = -1;
		bool				vertical;

		[SerializeField]
		Rect				savedRect;
	
		[System.NonSerialized]
		bool				first = true;
	
		Event				e { get { return Event.current; } }

		public ResizablePanelSeparator(PWLayoutOrientation orientation)
		{
			this.vertical = orientation == PWLayoutOrientation.Vertical;
		}
	
		public override Rect Begin()
		{
			Rect tmpRect = EditorGUILayout.BeginHorizontal();
			
			internHandlerPosition = (int)layoutSetting.separatorPosition;

			//if we are in the first frame, provide the true width value for layout (the only information we have currently)
			if (first)
			{
				availableRect = new Rect(0, 0, internHandlerPosition, 0);
				savedRect = new Rect(0, 0, internHandlerPosition, 0);
			}
	
			if (e.type == EventType.Repaint)
				availableRect = tmpRect;
	
			float w = internHandlerPosition - layoutSetting.minWidth;
			Rect beginRect = EditorGUILayout.BeginVertical(GUILayout.Width(w), GUILayout.MaxWidth(w));
			if (e.type == EventType.Repaint)
				savedRect = beginRect;

			savedRect.width = internHandlerPosition;

			if (debug)
				EditorGUI.DrawRect(savedRect, Color.blue);
	
			first = false;
	
			return savedRect;
		}

		public override Rect Split()
		{
			EditorGUILayout.EndVertical();
			
			//separator rect:
			EditorGUILayout.BeginVertical(GUILayout.Width(layoutSetting.separatorWidth));
			GUILayout.Space(layoutSetting.separatorWidth);
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(internHandlerPosition - availableRect.x - layoutSetting.separatorWidth), GUILayout.ExpandHeight(true));

			//left bar separation and resize:
			Rect handleRect = new Rect(internHandlerPosition - 1, availableRect.y, layoutSetting.separatorWidth, availableRect.height);
			Rect handleCatchRect = new Rect(internHandlerPosition - 1, availableRect.y, 6f, availableRect.height);
			EditorGUI.DrawRect(handleRect, Color.white);
			EditorGUIUtility.AddCursorRect(handleCatchRect, MouseCursor.ResizeHorizontal);
	
			if (Event.current.type == EventType.MouseDown && handleCatchRect.Contains(Event.current.mousePosition))
				resize = true;
			if (lastMouseX != -1 && resize)
				layoutSetting.separatorPosition += Event.current.mousePosition.x - lastMouseX;
			if (Event.current.rawType == EventType.MouseUp)
				resize = false;
			lastMouseX = Event.current.mousePosition.x;
			internHandlerPosition = (int)Mathf.Clamp(layoutSetting.separatorPosition, layoutSetting.minWidth, layoutSetting.maxWidth);
			layoutSetting.separatorPosition = Mathf.Clamp(layoutSetting.separatorPosition, layoutSetting.minWidth, layoutSetting.maxWidth);
	
			if (resize && Event.current.isMouse)
				Event.current.Use();
	
			float w = availableRect.width - internHandlerPosition;
			Rect r = new Rect(internHandlerPosition + layoutSetting.separatorWidth, availableRect.y, w, availableRect.height);

			if (debug)
				EditorGUI.DrawRect(r, Color.green);

			return r;
		}

		public override void End()
		{
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}

		public override PWLayoutSetting UpdateLayoutSetting(PWLayoutSetting ls)
		{
			PWLayoutSetting ret;

			ret = base.UpdateLayoutSetting(ls);

			if (ret == null && ls != null)
				ls.vertical = vertical;
			
			return ret;
		}
	}
}