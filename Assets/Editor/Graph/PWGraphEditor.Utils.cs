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

	//TODO: remove and create new window style on GIMP
	void GetWindowStyleFromType(Type t, out GUIStyle windowStyle, out GUIStyle windowSelectedStyle)
	{
		if (t == typeof(PWNodeGraphExternal) || t == typeof(PWNodeGraphInput) || t == typeof(PWNodeGraphOutput))
		{
			windowStyle = whiteNodeWindow;
			windowSelectedStyle = whiteNodeWindowSelected;
			return ;
		}
		foreach (var nodeCat in nodeSelectorList)
		{
			foreach (var nodeInfo in nodeCat.Value.nodes)
			{
				if (t == nodeInfo.nodeType)
				{
					windowStyle = nodeInfo.windowStyle;
					windowSelectedStyle = nodeInfo.windowSelectedStyle;
					return ;
				}
			}
		}
		windowStyle = greyNodeWindow;
		windowSelectedStyle = greyNodeWindowSelected;
	}
}
