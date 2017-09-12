﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Node;

using Debug = UnityEngine.Debug;

//Utils for graph editor
public partial class PWGraphEditor {

	void ContextMenu()
	{
		Event	e = Event.current;
        if (e.type == EventType.ContextClick)
        {
            Vector2 mousePos = e.mousePosition;

			// Now create the menu, add items and show it
			GenericMenu menu = new GenericMenu();
			foreach (var nodeCat in nodeSelectorList)
			{
				string menuString = "Create new/" + nodeCat.Key + "/";
				foreach (var nodeClass in nodeCat.Value.nodes)
					menu.AddItem(new GUIContent(menuString + nodeClass.name), false, CreateNewNode, nodeClass.nodeType);
			}
			menu.AddItem(new GUIContent("New Ordering group"), false, CreateNewOrderingGroup, e.mousePosition - graph.graphDecalPosition);
			if (mouseAboveOrderingGroup != null)
				menu.AddItem(new GUIContent("Delete ordering group"), false, DeleteOrderingGroup);
			else
				menu.AddDisabledItem(new GUIContent("Delete ordering group"));

			menu.AddSeparator("");
			if (mouseAboveNodeAnchor)
			{
				menu.AddItem(new GUIContent("New Link"), false, BeginDragLink);
				menu.AddItem(new GUIContent("Delete all links"), false, DeleteAllAnchorLinks);
			}

			var hoveredLink = currentLinks.FirstOrDefault(l => l.hover == true);
			if (hoveredLink != null)
			{
				menu.AddItem(new GUIContent("Delete link"), false, DeleteLink, hoveredLink);
			}
			else
				menu.AddDisabledItem(new GUIContent("Link"));

			menu.AddSeparator("");
			if (mouseAboveNodeIndex != -1)
				menu.AddItem(new GUIContent("Delete node"), false, DeleteNode, mouseAboveNodeIndex);
			else
				menu.AddDisabledItem(new GUIContent("Delete node"));
				
			if (selectedNodeCount != 0)
			{
				string deleteNodeString = (selectedNodeCount == 1) ? "delete selected node" : "delete selected nodes";
				menu.AddItem(new GUIContent(deleteNodeString), false, DeleteSelectedNodes);

				string moveNodeString = (selectedNodeCount == 1) ? "move selected node" : "move selected nodes";
				menu.AddItem(new GUIContent(moveNodeString), false, MoveSelectedNodes);
			}

			menu.ShowAsContext();
			e.Use();
        }
	}
}