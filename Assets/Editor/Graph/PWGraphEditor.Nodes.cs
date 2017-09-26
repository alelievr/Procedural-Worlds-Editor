using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Node;
using PW;

//Nodes rendering
public partial class PWGraphEditor {

	void DisplayDecaledNode(int id, PWNode node, string name)
	{
		var		e = Event.current;
		
		//if you are editing the node name, hide the current name.
		if (node.windowNameEdit)
			name = "";

		//node grid snapping when pressing cmd/crtl 
		if (node.isDragged && ((!MacOS && e.control) || (MacOS && e.command)))
		{
			Vector2 pos = node.windowRect.position;
			//aproximative grid cell size
			float	snapPixels = 25.6f;

			pos.x = Mathf.RoundToInt(Mathf.RoundToInt(pos.x / snapPixels) * snapPixels);
			pos.y = Mathf.RoundToInt(Mathf.RoundToInt(pos.y / snapPixels) * snapPixels);
			node.windowRect.position = pos;
		}

		//move the node if panPosition changed:
		node.windowRect = PWUtils.DecalRect(node.windowRect, graph.panPosition);
		Rect decaledRect = GUILayout.Window(id, node.windowRect, node.OnWindowGUI, name, (node.selected) ? node.windowSelectedStyle : node.windowStyle, GUILayout.Height(node.viewHeight));
		node.windowRect = PWUtils.DecalRect(decaledRect, -graph.panPosition);
	}

	void RenderNode(int id, PWNode node)
	{
		Event	e = Event.current;

		DisplayDecaledNode(id, node, name);

		//check if the mouse is over this node
		if (node.windowRect.Contains(e.mousePosition - graph.panPosition))
			editorEvents.mouseOverNode = node;

		//managed somewhere else
		/*//end dragging:
		if ((e.type == EventType.mouseUp && draggingLink == true) //standard drag start
				|| (e.type == EventType.MouseDown && draggingLink == true)) //drag started with context menu
			if (mouseAboveAnchor.mouseAbove && PWNode.AnchorAreAssignable(startDragAnchor, mouseAboveAnchor))
			{
				StopDragLink(true);

				//TODO: manage the AttachLink return values, if one of them is false, delete the link.

				//attach link to the node:
				graph.CreateLink();
				bool linkNotRevoked = node.AttachLink(mouseAboveAnchor, startDragAnchor);

				if (linkNotRevoked)
				{
					var win = FindNodeById(startDragAnchor.nodeId);
					if (win != null)
					{
						//remove link if it was revoked.
						if (!win.AttachLink(startDragAnchor, mouseAboveAnchor))
							node.DeleteLink(mouseAboveAnchor.anchorId, win, startDragAnchor.anchorId);
						
						graphNeedReload = true;
					}
					else
						Debug.LogWarning("window id not found: " + startDragAnchor.nodeId);
					
					//Recalcul the compute order:
					EvaluateComputeOrder();
				}
			}

		//if you press the mouse above an anchor, start the link drag
		if (mouseAboveAnchor.mouseAbove && e.type == EventType.MouseDown && e.button == 0)
			BeginDragLink();
		*/

		//display the process time of the window (if not 0)
		if (node.processTime > Mathf.Epsilon)
		{
			GUIStyle gs = new GUIStyle();
			Rect msRect = PWUtils.DecalRect(node.windowRect, graph.panPosition);
			msRect.position += new Vector2(msRect.size.x / 2 - 10, msRect.size.y + 5);
			gs.normal.textColor = greenRedGradient.Evaluate(node.processTime / 20); //20ms ok, after is red
			GUI.Label(msRect, node.processTime + " ms", gs);
		}
	}
	
	void DeleteSelectedNodes()
	{
		List< PWNode > nodeToRemove = new List< PWNode >();

		foreach (var node in graph.nodes)
			if (node.selected)
				nodeToRemove.Add(node);

		foreach (var node in nodeToRemove)
			graph.RemoveNode(node);
	}

	void MoveSelectedNodes()
	{
		Debug.Log("moving from context menu");
	}

}
