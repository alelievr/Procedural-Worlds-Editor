using System.Collections;
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
			foreach (var nodeCat in PWNodeTypeProvider.GetAllowedNodesForGraph(graph.GetType()))
			{
				string menuString = "Create new/" + nodeCat.title + "/";
				foreach (var nodeClass in nodeCat.typeInfos)
					menu.AddItem(new GUIContent(menuString + nodeClass.name), false, () => { graph.CreateNewNode(nodeClass.type, -graph.panPosition + e.mousePosition); });
			}
			menu.AddItem(new GUIContent("New Ordering group"), false, CreateNewOrderingGroup, e.mousePosition - graph.panPosition);
			if (editorEvents.mouseOverOrderingGroup != null)
				menu.AddItem(new GUIContent("Delete ordering group"), false, DeleteOrderingGroup);
			else
				menu.AddDisabledItem(new GUIContent("Delete ordering group"));

			menu.AddSeparator("");
			if (editorEvents.mouseOverAnchor != null)
			{
				menu.AddItem(new GUIContent("New Link"), false, BeginDragLink);
				menu.AddItem(new GUIContent("Delete all links"), false, DeleteAllAnchorLinks);
			}

			var hoveredLink = editorEvents.mouseOverLink;
			if (hoveredLink != null)
			{
				menu.AddItem(new GUIContent("Delete link"), false, () => { graph.RemoveLink(hoveredLink); });
			}
			else
				menu.AddDisabledItem(new GUIContent("Link"));

			menu.AddSeparator("");
			if (editorEvents.mouseOverNode != null)
				menu.AddItem(new GUIContent("Delete node"), false, () => { graph.RemoveNode(editorEvents.mouseOverNode); });
			else
				menu.AddDisabledItem(new GUIContent("Delete node"));
				
			if (editorEvents.selectedNodeCount != 0)
			{
				string deleteNodeString = (editorEvents.selectedNodeCount == 1) ? "delete selected node" : "delete selected nodes";
				menu.AddItem(new GUIContent(deleteNodeString), false, DeleteSelectedNodes);

				string moveNodeString = (editorEvents.selectedNodeCount == 1) ? "move selected node" : "move selected nodes";
				menu.AddItem(new GUIContent(moveNodeString), false, MoveSelectedNodes);
			}

			menu.ShowAsContext();
			e.Use();
        }
	}
}
