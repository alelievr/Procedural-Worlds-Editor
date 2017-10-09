using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using PW.Core;
using PW.Node;

using Debug = UnityEngine.Debug;

//TODO: remove this file and create another for events and editor states

//Utils for graph editor
public partial class PWGraphEditor {
	
	void StartDragLink()
	{
		Debug.Log("Start dragging link");
		graph.editorEvents.startedLinkAnchor = editorEvents.mouseOverAnchor;
		graph.editorEvents.isDraggingLink = true;
	}

	void StopDragLink(bool linked)
	{
		//TODO: maybe fusion this two structure (the one in the graph must not exist)
		graph.editorEvents.isDraggingLink = false;
	}

	void DeleteAllAnchorLinks()
	{
		editorEvents.mouseOverNode.RemoveAllLinksFromAnchor(editorEvents.mouseOverAnchor);
	}
}
