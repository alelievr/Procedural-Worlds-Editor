using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;
using System;
using System.Reflection.Emit;

namespace ProceduralWorlds.Editor
{
	//Node core rendering
	public abstract partial class NodeEditor
	{
		static GUIStyle 			renameNodeTextFieldStyle;
		static GUIStyle				innerNodePaddingStyle;
		static GUIStyle				nodeStyle;
		static bool					styleLoadedStatic;
		[System.NonSerialized]
		bool						styleLoaded;

		List< object >				propertiesBeforeGUI;
		List< object >				propertiesAfterGUI;

		Dictionary< string, ReflectionUtils.GenericCaller > bakedChildFieldGetters = new Dictionary< string, ReflectionUtils.GenericCaller >();

		void LoadCoreResources()
		{
			if (!styleLoaded)
				styleLoaded = true;
			
			if (!styleLoadedStatic)
			{
				//check if style was already initialized:
				if (innerNodePaddingStyle != null)
					return ;
	
				renameNodeTextFieldStyle = GUI.skin.FindStyle("RenameNodetextField");
				innerNodePaddingStyle = GUI.skin.FindStyle("WindowInnerPadding");
				nodeStyle = GUI.skin.FindStyle("Node");
	
				styleLoadedStatic = true;
			}
		}

		void RenderNode()
		{
			var e = Event.current;

			Profiler.BeginSample("[PW] " + nodeRef.GetType() + " rendering");
			
			//update the PWGUI window rect with this window rect:
			PWGUI.StartFrame(nodeRef.rect);

			// set the header of the window as draggable:
			int width = (int) nodeRef.rect.width;
			int padding = 8;
			Rect dragRect = new Rect(padding, 0, width - padding * 2, 20);
			
			Rect debugIconRect = dragRect;
			int	debugIconSize = 16;
			debugIconRect.position += new Vector2(width - debugIconSize, 0);
			GUI.DrawTexture(debugIconRect, debugIcon);

			if (e.type == EventType.MouseDown && e.button == 0 && dragRect.Contains(e.mousePosition))
			{
				nodeRef.isDragged = true;
				editorEvents.isDraggingNode = true;
			}
			if (e.rawType == EventType.MouseUp)
			{
				nodeRef.isDragged = false;
				editorEvents.isDraggingNode = false;
			}

			if (nodeRef.isDragged)
				Undo.RecordObject(this, "Node " + nodeRef.name + " dragged");
			
			//Drag window
			if (e.button == 0 && !windowNameEdit)
				GUI.DragWindow(dragRect);
			
			//Undo/Redo handling:
			propertiesBeforeGUI = TakeUndoablePropertiesSnapshot(propertiesBeforeGUI);

			GUILayout.BeginVertical(innerNodePaddingStyle);
			{
				DrawNullInputGUI();

				OnNodeGUI();

				EditorGUIUtility.labelWidth = 0;
			}
			GUILayout.EndVertical();
			
			propertiesAfterGUI = TakeUndoablePropertiesSnapshot(propertiesAfterGUI);

			if (PropertiesSnapshotDiffers(propertiesBeforeGUI, propertiesAfterGUI))
			{
				//Set back the nodeRef values to what they was before the modification
				RestorePropertiesSnapshot(propertiesBeforeGUI);

				//Then record the object
				Undo.RecordObject(this, "Property updated in " + nodeRef.name);

				//And set back the modified values
				RestorePropertiesSnapshot(propertiesAfterGUI);
				
				// Debug.Log("Undo recorded: in " + nodeRef.GetType());
			}

			int viewH = (int)GUILayoutUtility.GetLastRect().height;
			if (e.type == EventType.Repaint)
				nodeRef.viewHeight = viewH;

			nodeRef.viewHeight = Mathf.Max(nodeRef.viewHeight, maxAnchorRenderHeight);
				
			if (e.type == EventType.Repaint)
				nodeRef.viewHeight += 24;
			
			RenderAnchors();
			ProcessAnchorEvents();

			Profiler.EndSample();

			delayedChanges.Update();

			Rect selectRect = new Rect(10, 18, rect.width - 20, rect.height - 18);
			if (e.type == EventType.MouseDown && e.button == 0 && selectRect.Contains(e.mousePosition))
			{
				nodeRef.isSelected = !nodeRef.isSelected;
				if (nodeRef.isSelected)
				{
					graphEditor.RaiseNodeSelected(nodeRef);
					if (nodeInspectorGUIOverloaded)
						Selection.activeObject = nodeRef;
				}
				else
					graphEditor.RaiseNodeUnselected(nodeRef);
			}
		}

		List< object > TakeUndoablePropertiesSnapshot(List< object > buffer = null)
		{
			if (buffer == null)
				buffer = new List< object >(new object[nodeRef.undoableFields.Count]);
			
			for (int i = 0; i < nodeRef.undoableFields.Count; i++)
				buffer[i] = nodeRef.undoableFields[i].GetValue(nodeRef);
			
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
			for (int i = 0; i < nodeRef.undoableFields.Count; i++)
				nodeRef.undoableFields[i].SetValue(nodeRef, properties[i]);
		}

		void RenderInspector()
		{
			OnNodeInspectorGUI();
		}

		void DrawNullInputGUI()
		{
			foreach (var inputAnchor in nodeRef.inputAnchors)
			{
				if (!inputAnchor.required)
					continue ;
				
				if (!bakedChildFieldGetters.ContainsKey(inputAnchor.fieldName))
					bakedChildFieldGetters[inputAnchor.fieldName] = CreateGenericGetter(inputAnchor.fieldName);

				if (bakedChildFieldGetters[inputAnchor.fieldName].Call(nodeRef) == null)
					EditorGUILayout.HelpBox("Null parameter: " + inputAnchor.fieldName, MessageType.Error);
			}
		}

		ReflectionUtils.GenericCaller CreateGenericGetter(string fieldName)
		{
			//Create the delegate type that takes our node type in parameter
			var delegateType = typeof(ReflectionUtils.ChildFieldGetter<>).MakeGenericType(new Type[] { nodeRef.GetType() });

			//Get the child field from base class
			FieldInfo fi = nodeRef.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);

			//Create a new method which return the field fi
			DynamicMethod dm = new DynamicMethod("Get" + fi.Name, typeof(object), new Type[] { nodeRef.GetType() }, nodeRef.GetType());
			ILGenerator il = dm.GetILGenerator();
			// Load the instance of the object (argument 0) onto the stack
			il.Emit(OpCodes.Ldarg_0);
			// Load the value of the object's field (fi) onto the stack
			il.Emit(OpCodes.Ldfld, fi);
			// return the value on the top of the stack
			il.Emit(OpCodes.Ret);

			//Create a specific type from Caller which will cast the generic type to a specific one to call the generated delegate
			var callerType = typeof(ReflectionUtils.Caller<>).MakeGenericType(new Type[] { nodeRef.GetType() });

			//Instantiate this type and bind the delegate
			var genericCaller = Activator.CreateInstance(callerType) as ReflectionUtils.GenericCaller;
			genericCaller.SetDelegate(dm.CreateDelegate(delegateType));

			return genericCaller;
		}
	}

}