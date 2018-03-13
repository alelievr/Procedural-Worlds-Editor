using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	public class ResizablePanelSeparator : PWLayoutSeparator
	{
		Rect				availableRect;
		int					internHandlerPosition;
		readonly bool		vertical;

		[SerializeField]
		Rect				lastRect;

		Rect				separatorRect;

		public bool			draggingHandler { get; private set; }

		public ResizablePanelSeparator(PWLayoutOrientation orientation)
		{
			this.vertical = orientation == PWLayoutOrientation.Vertical;
		}
	
		public override Rect Begin()
		{
			internHandlerPosition = (int)layoutSetting.separatorPosition;
			if (vertical)
			{
			}
			else
			{
				if (layoutSetting.leftBar)
					DrawHandleBar();
				
				Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Width(internHandlerPosition), GUILayout.ExpandHeight(true));
				if (e.type == EventType.Repaint)
					lastRect = r;
			}

			lastRect.width = internHandlerPosition;

			return lastRect;
		}

		public override void End()
		{
			if (vertical)
			{
			}
			else
			{
				EditorGUILayout.EndHorizontal();
	
				if (!layoutSetting.leftBar)
					DrawHandleBar();
			}
		}

		public override Rect GetSeparatorRect()
		{
			return separatorRect;
		}

		void DrawHandleBar()
		{
			Rect separatorRect = EditorGUILayout.BeginHorizontal(GUILayout.Width(layoutSetting.separatorWidth), GUILayout.ExpandHeight(true));
			GUILayout.Space(layoutSetting.separatorWidth);
			EditorGUI.DrawRect(separatorRect, Color.white);
			EditorGUILayout.EndHorizontal();

			if (e.type == EventType.Repaint)
				this.separatorRect = separatorRect;

			EditorGUIUtility.AddCursorRect(separatorRect, MouseCursor.ResizeHorizontal);

			if (e.type == EventType.MouseDown && e.button == 0)
				if (separatorRect.Contains(e.mousePosition))
					draggingHandler = true;
				
			if (e.type == EventType.MouseDrag && e.button == 0 && draggingHandler)
				layoutSetting.separatorPosition += (layoutSetting.leftBar) ? -e.delta.x : e.delta.x;
			
			float p = layoutSetting.separatorPosition - lastRect.x;
			layoutSetting.separatorPosition = Mathf.Clamp(p, layoutSetting.minWidth - lastRect.x, layoutSetting.maxWidth - lastRect.x) + lastRect.x;
			
			if (e.rawType == EventType.MouseUp)
				draggingHandler = false;
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