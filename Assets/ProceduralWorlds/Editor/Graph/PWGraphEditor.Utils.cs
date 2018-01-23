using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Node;

using Debug = UnityEngine.Debug;

//TODO: remove this file and create another for events and editor states

//Utils for graph editor
public partial class PWGraphEditor
{
	
	void StartDragLink()
	{
		Undo.RecordObject(graph, "Link started");
		graph.editorEvents.startedLinkAnchor = editorEvents.mouseOverAnchor;
		graph.editorEvents.isDraggingLink = true;
		graph.RaiseOnLinkStartDragged(graph.editorEvents.startedLinkAnchor);
	}

	void StopDragLink(bool linked)
	{
		Debug.Log("linked: " + linked);
		graph.editorEvents.isDraggingLink = false;

		if (!linked)
			graph.RaiseOnLinkCancenled();
		
		graph.RaiseOnLinkStopDragged();
	}

	void DeleteAllAnchorLinks()
	{
		editorEvents.mouseOverAnchor.RemoveAllLinks();
	}
}
