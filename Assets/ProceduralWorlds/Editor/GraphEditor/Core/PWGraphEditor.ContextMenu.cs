using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;
using PW.Core;
using PW;
using PW.Node;
using System.IO;
using System;

using Debug = UnityEngine.Debug;

//Utils for graph editor
public partial class PWGraphEditor
{

	readonly GUIContent	newOrderingGroupContent = new GUIContent("New Ordering group");
	readonly GUIContent	deleteOrderingGroupContent = new GUIContent("Delete Ordering group");

	readonly GUIContent	newLinkContent = new GUIContent("New Link");
	readonly GUIContent	deleteAllLinksContent = new GUIContent("Delete all links");
	readonly GUIContent	deleteLinkContent = new GUIContent("Delete link");
	
	readonly GUIContent	deleteNodeContent = new GUIContent("Delete node");
	readonly GUIContent	openNodeScriptContent = new GUIContent("Open C# Script");

	readonly GUIContent	debugNodeContent = new GUIContent("Debug/Node");
	readonly GUIContent	debugAnchorContent = new GUIContent("Debug/Anchor");

	readonly GUIContent	recenterGraphContent = new GUIContent("Recenter the graph");

	protected Event e { get { return Event.current; } }

	void ContextMenu()
	{
        if (e.type == EventType.ContextClick)
        {
			Profiler.BeginSample("[PW] render context menu");

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
			menu.AddItemState(deleteOrderingGroupContent, editorEvents.isMouseOverOrderingGroup, DeleteOrderingGroup);

			menu.AddSeparator("");

			if (editorEvents.mouseOverAnchor != null)
			{
				menu.AddItem(newLinkContent, false, StartDragLink);
				menu.AddItem(deleteAllLinksContent, false, DeleteAllAnchorLinks);
			}

			var hoveredLink = editorEvents.mouseOverLink;
			menu.AddItemState(deleteLinkContent, hoveredLink != null, () => { graph.RemoveLink(hoveredLink); });

			menu.AddSeparator("");
			menu.AddItemState(deleteNodeContent, editorEvents.isMouseOverNode, () => { graph.RemoveNode(editorEvents.mouseOverNode); });
			
			if (editorEvents.selectedNodeCount != 0)
			{
				string deleteNodeString = (editorEvents.selectedNodeCount == 1) ? "delete selected node" : "delete selected nodes";
				menu.AddItem(new GUIContent(deleteNodeString), false, DeleteSelectedNodes);

				string moveNodeString = (editorEvents.selectedNodeCount == 1) ? "move selected node" : "move selected nodes";
				menu.AddItem(new GUIContent(moveNodeString), false, MoveSelectedNodes);
			}

			menu.AddSeparator("");

			var hoveredNode = editorEvents.mouseOverNode;
			menu.AddItemState(openNodeScriptContent, hoveredNode != null, () => { OpenNodeScript(hoveredNode); });
			menu.AddItemState(debugNodeContent, hoveredNode != null, () => { hoveredNode.debug = !hoveredNode.debug; }, (hoveredNode != null) ? hoveredNode.debug : false);
			
			var hoveredAnchor = editorEvents.mouseOverAnchor;
			menu.AddItemState(debugAnchorContent, hoveredAnchor != null, () => { hoveredAnchor.debug = !hoveredAnchor.debug; }, (hoveredAnchor != null) ? hoveredAnchor.debug : false);

			menu.AddSeparator("");
			menu.AddItem(recenterGraphContent, false, () => { graph.scale = 1; graph.panPosition = Vector2.zero; });

			menu.ShowAsContext();
			e.Use();

			Profiler.EndSample();
        } 
	}

	public void OpenNodeScript(PWNode node)
	{
		var monoScript = MonoScript.FromScriptableObject(node);

		string filePath = AssetDatabase.GetAssetPath(monoScript);

		if (File.Exists(filePath))
			UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(filePath, 21);
	}
}
