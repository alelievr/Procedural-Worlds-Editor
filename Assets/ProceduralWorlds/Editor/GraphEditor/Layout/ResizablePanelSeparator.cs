using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Editor
{
	public class ResizablePanelSeparator : PWLayoutSeparator
	{
	
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
			Rect tmpRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			
			internHandlerPosition = (int)layoutSetting.separatorPosition;

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

			EditorGUI.DrawRect(savedRect, Color.red);
	
			return savedRect;
		}

		public override Rect Split()
		{
			EditorGUILayout.EndVertical();

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
	
			Rect r = new Rect(internHandlerPosition + 3, availableRect.y, availableRect.width - internHandlerPosition, availableRect.height);

			return r;
		}

		public override Rect End()
		{
			EditorGUILayout.EndHorizontal();
			
			//TODO: rect
			return new Rect();
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