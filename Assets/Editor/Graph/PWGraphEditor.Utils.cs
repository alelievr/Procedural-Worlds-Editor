using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using PW.Core;
using PW.Node;

using Debug = UnityEngine.Debug;

//TODO: remove this file

//Utils for graph editor
public partial class PWGraphEditor {
	
	void BeginDragLink()
	{
		editorEvents.startedLinkAnchor = editorEvents.mouseOverAnchor;
		editorEvents.isDraggingLink = true;
	}

	void StopDragLink(bool linked)
	{
		//TODO: maybe fusion this two structure (the one in the graph must not exist)
		editorEvents.isDraggingLink = false;
	}

	void DeleteAllAnchorLinks()
	{
		editorEvents.mouseOverNode.RemoveAllLinksFromAnchor(editorEvents.mouseOverAnchor);
	}
}
