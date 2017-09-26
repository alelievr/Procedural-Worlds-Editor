using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

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

			//update the PWGUI window rect with this window rect:
			PWGUI.currentWindowRect = windowRect;
			PWGUI.StartFrame();

			// set the header of the window as draggable:
			int width = (int) windowRect.width;
			Rect dragRect = new Rect(30, 0, width, 20);
			if (e.type == EventType.MouseDown && dragRect.Contains(e.mousePosition))
				isDragged = true;
			if (e.type == EventType.MouseUp)
				isDragged = false;
			if (id != -1 && e.button == 0 && !windowNameEdit)
				GUI.DragWindow(dragRect);

			int	debugViewH = 0;
			#if DEBUG_NODE
				EditorGUILayout.BeginVertical(debugStyle);
				EditorGUILayout.LabelField("Id: " + nodeId + " | Compute order: " + computeOrder);
				EditorGUILayout.LabelField("type: " + GetType());
				EditorGUILayout.LabelField("isDependent: " + isDependent);
				EditorGUILayout.LabelField("Dependencies:");
				foreach (var dep in depencendies)
					EditorGUILayout.LabelField("    " + dep.nodeId + " : " + dep.anchorId);
				EditorGUILayout.LabelField("Links:");
				foreach (var l in links)
					EditorGUILayout.LabelField("    " + l.distantNodeId + " : " + l.distantAnchorId);
				EditorGUILayout.EndVertical();
				debugViewH = (int)GUILayoutUtility.GetLastRect().height + 6; //add the padding and margin
			#endif

			GUILayout.BeginVertical(innerNodePaddingStyle);
			{
				OnNodeGUI();

				EditorGUIUtility.labelWidth = 0;
			}
			GUILayout.EndVertical();

			int viewH = (int)GUILayoutUtility.GetLastRect().height;
			if (e.type == EventType.Repaint)
				viewHeight = viewH + debugViewH;

			viewHeight = Mathf.Max(viewHeight, maxAnchorRenderHeight);
				
			if (e.type == EventType.Repaint)
				viewHeight += 24;
			
			RenderAnchors();
			ProcessAnchorEvents();

			delayedChanges.Update();

			Rect selectRect = new Rect(10, 18, windowRect.width - 20, windowRect.height - 18);
			if (e.type == EventType.MouseDown && e.button == 0 && selectRect.Contains(e.mousePosition))
				selected = !selected;

			
			//fill the graph event infos:
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