using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;
using PW.Core;

//Top option bar
public partial class PWMainGraphEditor {

	void DrawNodeGraphHeader(Rect graphRect)
	{
		Event	e = Event.current;
		EditorGUILayout.BeginVertical(navBarBackgroundStyle);
		{
			//Icon bar:
			EditorGUILayout.BeginHorizontal(navBarBackgroundStyle, GUILayout.MaxHeight(40), GUILayout.ExpandWidth(true));
			{
				//recenter the graph
				if (GUILayout.Button(rencenterIconTexture, GUILayout.Width(30), GUILayout.Height(30)))
					currentGraph.graphDecalPosition = Vector2.zero;
				//ping the current PW object in the project window
				if (GUILayout.Button(fileIconTexture, GUILayout.Width(30), GUILayout.Height(30)))
					EditorGUIUtility.PingObject(currentGraph);
				//Show every hidden object in your hierarchy
				if (GUILayout.Button(eyeIconTexture, GUILayout.Width(30), GUILayout.Height(30)))
				{
					var objs = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];

					foreach (var obj in objs)
					{
						if (obj.hideFlags == HideFlags.HideAndDontSave)
							obj.hideFlags = HideFlags.DontSave;
					}
				}
			}
			EditorGUILayout.EndHorizontal();
	
			//remove 4 pixels for the separation bar
			graphRect.size -= Vector2.right * 4;
	
			#if (DEBUG_GRAPH)
			foreach (var node in nodes)
				GUI.DrawTexture(PWUtils.DecalRect(node.rect, graphDecalPosition), debugTexture1);
			#endif
	
			if (e.type == EventType.MouseDown) //if event is mouse down
			{
				//TODO: remove the graph header height
				if (graphRect.Contains(e.mousePosition))
				{
					if (e.button == 2 || (e.command && e.button == 0))
						draggingGraph = true;
				}
				if (!mouseAboveNodeAnchor //if mouse is not above a node anchor
					&& mouseAboveNodeIndex == -1 //and mouse is notabove a node
					&& e.button == 0
					&& !e.command
					&& !e.control)
				{
					selecting = true;
					selectionRect.position = e.mousePosition;
					selectionRect.size = Vector2.zero;
				}
			}
			if (e.type == EventType.MouseUp)
			{
				selecting = false;
				draggingGraph = false;
				previewMouseDrag = false;
				draggingSelectedNodes = false;
				draggingSelectedNodesFromContextMenu = false;
			}
			if (e.type == EventType.Layout)
			{
				if (draggingGraph)
					currentGraph.graphDecalPosition += e.mousePosition - lastMousePosition;
				lastMousePosition = e.mousePosition;
			}
		}
		EditorGUILayout.EndVertical();
	}


}
