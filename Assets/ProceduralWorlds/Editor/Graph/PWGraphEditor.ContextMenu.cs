using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Node;

using Debug = UnityEngine.Debug;

//Utils for graph editor
public partial class PWGraphEditor
{

	GUIContent	newOrderingGroupContent = new GUIContent("New Ordering group");
	GUIContent	deleteOrderingGroupContent = new GUIContent("Delete Ordering group");

	GUIContent	newLinkContent = new GUIContent("New Link");
	GUIContent	deleteAllLinksContent = new GUIContent("Delete all links");
	GUIContent	deleteLinkContent = new GUIContent("Delete link");
	
	GUIContent	deleteNodeContent = new GUIContent("Delete node");

	GUIContent	debugNodeContent = new GUIContent("Debug/Node");
	GUIContent	debugAnchorContent = new GUIContent("Debug/Anchor");

	GUIContent	recenterGraphContent = new GUIContent("Recenter the graph");


	void ContextMenu()
	{
		Event	e = Event.current;

        if (e.type == EventType.ContextClick)
        {
            Vector2 mousePos = e.mousePosition;

			// Now create the menu, add items and show it
			GenericMenu menu = new GenericMenu();
			foreach (var nodeCat in PWNodeTypeProvider.GetAllowedNodesForGraph(graph.graphType))
			{
				string menuString = "Create new/" + nodeCat.title + "/";
				foreach (var nodeClass in nodeCat.typeInfos)
					menu.AddItem(new GUIContent(menuString + nodeClass.name), false, () => { graph.CreateNewNode(nodeClass.type, -graph.panPosition + e.mousePosition); Debug.Log("pos: " + -graph.panPosition + e.mousePosition); });
			}
			menu.AddItem(newOrderingGroupContent, false, CreateNewOrderingGroup, e.mousePosition - graph.panPosition);
			if (editorEvents.mouseOverOrderingGroup != null)
				menu.AddItem(deleteOrderingGroupContent, false, DeleteOrderingGroup);
			else
				menu.AddDisabledItem(deleteOrderingGroupContent);

			menu.AddSeparator("");

			if (editorEvents.mouseOverAnchor != null)
			{
				menu.AddItem(newLinkContent, false, StartDragLink);
				menu.AddItem(deleteAllLinksContent, false, DeleteAllAnchorLinks);
			}

			var hoveredLink = editorEvents.mouseOverLink;
			if (hoveredLink != null)
				menu.AddItem(deleteLinkContent, false, () => { graph.RemoveLink(hoveredLink); });
			else
				menu.AddDisabledItem(deleteLinkContent);

			menu.AddSeparator("");
			if (editorEvents.mouseOverNode != null)
				menu.AddItem(deleteNodeContent, false, () => { graph.RemoveNode(editorEvents.mouseOverNode); });
			else
				menu.AddDisabledItem(deleteNodeContent);
				
			if (editorEvents.selectedNodeCount != 0)
			{
				string deleteNodeString = (editorEvents.selectedNodeCount == 1) ? "delete selected node" : "delete selected nodes";
				menu.AddItem(new GUIContent(deleteNodeString), false, DeleteSelectedNodes);

				string moveNodeString = (editorEvents.selectedNodeCount == 1) ? "move selected node" : "move selected nodes";
				menu.AddItem(new GUIContent(moveNodeString), false, MoveSelectedNodes);
			}

			menu.AddSeparator("");

			var hoveredNode = editorEvents.mouseOverNode;
			if (hoveredNode != null)
				menu.AddItem(debugNodeContent, hoveredNode.debug, () => { hoveredNode.debug = !hoveredNode.debug; });
			else
				menu.AddDisabledItem(debugNodeContent);
			
			var hoveredAnchor = editorEvents.mouseOverAnchor;
			if (hoveredAnchor != null)
				menu.AddItem(debugAnchorContent, hoveredAnchor.debug, () => { hoveredAnchor.debug = !hoveredAnchor.debug; });
			else
				menu.AddDisabledItem(debugAnchorContent);

			menu.AddSeparator("");
			menu.AddItem(recenterGraphContent, false, () => { graph.scale = 1; graph.panPosition = Vector2.zero; });

			menu.ShowAsContext();
			e.Use();
        }
	}
}
