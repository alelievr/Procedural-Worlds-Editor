using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Node;

namespace PW
{
	//Node's window rendering, event processing and Process callback()
	public partial class PWNode
	{
		public int		viewHeight = 0; //to keep ???

		#if UNITY_EDITOR
		public void OnWindowGUI(int id)
		{
			var e = Event.current;
			
			if (!styleLoaded)
			{
				LoadStyles();
				foreach (var anchor in anchorFields)
					anchor.LoadStylesAndAssets();
			}
			
			//update the PWGUI window rect with this window rect:
			PWGUI.StartFrame(rect);

			// set the header of the window as draggable:
			int width = (int) rect.width;
			Rect dragRect = new Rect(30, 0, width, 20);
			
			Rect debugIconRect = dragRect;
			int	debugIconSize = 16;
			debugIconRect.position += new Vector2(width - debugIconSize, 0);
			GUI.DrawTexture(debugIconRect, debugIcon);

			if (e.type == EventType.MouseDown && dragRect.Contains(e.mousePosition))
			{
				isDragged = true;
				editorEvents.isDraggingNode = true;
			}
			if (e.rawType == EventType.MouseUp)
			{
				isDragged = false;
				editorEvents.isDraggingNode = false;
			}
			
			if (id != -1 && e.button == 0 && !windowNameEdit)
				GUI.DragWindow(dragRect);
			
			GUILayout.BeginVertical(innerNodePaddingStyle);
			{
				OnNodeGUI();

				EditorGUIUtility.labelWidth = 0;
			}
			GUILayout.EndVertical();

			int viewH = (int)GUILayoutUtility.GetLastRect().height;
			if (e.type == EventType.Repaint)
				viewHeight = viewH;

			viewHeight = Mathf.Max(viewHeight, maxAnchorRenderHeight);
				
			if (e.type == EventType.Repaint)
				viewHeight += 24;
			
			RenderAnchors();
			ProcessAnchorEvents();

			delayedChanges.Update();

			Rect selectRect = new Rect(10, 18, rect.width - 20, rect.height - 18);
			if (e.type == EventType.MouseDown && e.button == 0 && selectRect.Contains(e.mousePosition))
				isSelected = !isSelected;
		}
		#endif

		public virtual void	OnNodeGUI()
		{
			EditorGUILayout.LabelField("empty node");
		}

		public void Process()
		{
			OnNodeProcess();
			if (!realMode)
				OnPostProcess();
		}
	}
}