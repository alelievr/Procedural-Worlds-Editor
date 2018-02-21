using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;
using UnityEngine;
using UnityEditor;
using System.Linq;
using PW.Core;
using PW.Node;

namespace PW.Editor
{
	//Node core rendering
	public abstract partial class PWNodeEditor
	{
		static GUIStyle 			renameNodeTextFieldStyle = null;
		static GUIStyle				innerNodePaddingStyle = null;
		static GUIStyle				nodeStyle = null;
		static bool					styleLoadedStatic = false;
		[System.NonSerialized]
		bool						styleLoaded = false;

		public int		viewHeight = 0; //to keep ???

		List< object >	propertiesBeforeGUI = null;
		List< object >	propertiesAfterGUI = null;

		void LoadCoreStyle()
		{
			if (!styleLoaded)
			{
				//TODO: this will not works
				foreach (var anchor in node.anchorFields)
					anchor.LoadStylesAndAssets();
				styleLoaded = true;
			}
			if (!styleLoadedStatic)
				LoadStyles();
		}

		void RenderNode()
		{
			var e = Event.current;

			Profiler.BeginSample("[PW] " + GetType() + " rendering");
			
			//update the PWGUI window rect with this window rect:
			PWGUI.StartFrame(node.rect);

			// set the header of the window as draggable:
			int width = (int) node.rect.width;
			int padding = 8;
			Rect dragRect = new Rect(padding, 0, width - padding * 2, 20);
			
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

			if (isDragged)
				Undo.RecordObject(this, "Node " + name + " dragged");
			
			//Drag window
			if (e.button == 0 && !windowNameEdit)
				GUI.DragWindow(dragRect);
			
			//Undo/Redo handling:
			propertiesBeforeGUI = TakeUndoablePropertiesSnapshot(propertiesBeforeGUI);

			GUILayout.BeginVertical(innerNodePaddingStyle);
			{
				OnNodeGUI();

				EditorGUIUtility.labelWidth = 0;
			}
			GUILayout.EndVertical();
			
			propertiesAfterGUI = TakeUndoablePropertiesSnapshot(propertiesAfterGUI);

			if (PropertiesSnapshotDiffers(propertiesBeforeGUI, propertiesAfterGUI))
			{
				//Set back the node values to what they was before the modification
				RestorePropertiesSnapshot(propertiesBeforeGUI);

				//Then record the object
				Undo.RecordObject(this, "Property updated in " + name);

				//And set back the modified values
				RestorePropertiesSnapshot(propertiesAfterGUI);
				
				// Debug.Log("Undo recorded: in " + GetType());
			}

			int viewH = (int)GUILayoutUtility.GetLastRect().height;
			if (e.type == EventType.Repaint)
				viewHeight = viewH;

			viewHeight = Mathf.Max(viewHeight, maxAnchorRenderHeight);
				
			if (e.type == EventType.Repaint)
				viewHeight += 24;
			
			RenderAnchors();
			ProcessAnchorEvents();

			Profiler.EndSample();

			delayedChanges.Update();

			Rect selectRect = new Rect(10, 18, rect.width - 20, rect.height - 18);
			if (e.type == EventType.MouseDown && e.button == 0 && selectRect.Contains(e.mousePosition))
				isSelected = !isSelected;
		}
		#endif

		List< object > TakeUndoablePropertiesSnapshot(List< object > buffer = null)
		{
			if (buffer == null)
				buffer = new List< object >(new object[undoableFields.Count]);
			
			for (int i = 0; i < undoableFields.Count; i++)
				buffer[i] = undoableFields[i].GetValue(this);
			
			return buffer;
		}

		bool PropertiesSnapshotDiffers(List< object > propertiesList1, List< object > propertiesList2)
		{
			if (propertiesList1.Count != propertiesList2.Count)
				return true;
			
			for (int i = 0; i < propertiesList1.Count; i++)
			{
				var p1 = propertiesList1[i];
				var p2 = propertiesList2[i];

				if (p1 == null)
					continue ;

				if (!p1.Equals(p2))
					return true;
			}

			return false;
		}

		void RestorePropertiesSnapshot(List< object > properties)
		{
			for (int i = 0; i < undoableFields.Count; i++)
				undoableFields[i].SetValue(this, properties[i]);
		}
	}
}