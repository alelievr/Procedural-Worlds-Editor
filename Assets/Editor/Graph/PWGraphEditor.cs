using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;
using PW.Core;
using PW.Node;

[System.Serializable]
public class PWGraphEditor : EditorWindow {

	public virtual void OnEnable()
	{
	}

	public virtual void OnDisable()
	{
	}

	//draw the default node graph:
	public virtual void OnGUI()
	{

	}
	
#region Node rendering

	void DisplayDecaledNode(int id, PWNode node, string name)
	{
		var		e = Event.current;
		bool 	Mac = SystemInfo.operatingSystem.Contains("Mac");
		
		//if you are editing the node name, hide the current name.
		if (node.windowNameEdit)
			name = "";

		if (node.isDragged && ((!Mac && e.control) || (Mac && e.command)))
		{
			Vector2 pos = node.windowRect.position;
			float	snapPixels = 25.6f;

			pos.x = Mathf.RoundToInt(Mathf.RoundToInt(pos.x / snapPixels) * snapPixels);
			pos.y = Mathf.RoundToInt(Mathf.RoundToInt(pos.y / snapPixels) * snapPixels);
			node.windowRect.position = pos;
		}
		node.UpdateGraphDecal(currentGraph.graphDecalPosition);
		node.windowRect = PWUtils.DecalRect(node.windowRect, currentGraph.graphDecalPosition);
		Rect decaledRect = GUILayout.Window(id, node.windowRect, node.OnWindowGUI, name, (node.selected) ? node.windowSelectedStyle : node.windowStyle, GUILayout.Height(node.viewHeight));
		if (node.windowRect.Contains(e.mousePosition))
			mouseAboveNode = node;
		else if (e.type == EventType.MouseDown)
			node.OnClickedOutside();
		node.windowRect = PWUtils.DecalRect(decaledRect, -currentGraph.graphDecalPosition);
	}

	void RenderNode(int id, PWNode node, string name, int index, ref bool mouseAboveAnchorLocal, ref bool mouseDraggingWindowLocal)
	{
		Event	e = Event.current;

		DisplayDecaledNode(id, node, name);

		if (node.windowRect.Contains(e.mousePosition - currentGraph.graphDecalPosition))
		{
			mouseAboveNodeIndex = index;
		}

		//highlight, hide, add all linkable anchors:
		if (draggingLink)
			node.HighlightLinkableAnchorsTo(startDragAnchor);
		node.DisplayHiddenMultipleAnchors(draggingLink);

		//process envent, state and position for node anchors:
		var mouseAboveAnchor = node.GetAnchorUnderMouse();
		if (mouseAboveAnchor.mouseAbove)
			mouseAboveAnchorLocal = true;

		if (!mouseDraggingWindowLocal)
			if (node.isDragged)
			{
				if (node.selected)
				{
					int	selectedNodeCount = 0;
	
					currentGraph.ForeachAllNodes(n => { if (n.selected) selectedNodeCount++; }, false, true);
					if (selectedNodeCount != 0)
						draggingSelectedNodes = true;
				}
				mouseDraggingWindowLocal = true;
			}

		//end dragging:
		if ((e.type == EventType.mouseUp && draggingLink == true) //standard drag start
				|| (e.type == EventType.MouseDown && draggingLink == true)) //drag started with context menu
			if (mouseAboveAnchor.mouseAbove && PWNode.AnchorAreAssignable(startDragAnchor, mouseAboveAnchor))
			{
				StopDragLink(true);

				//TODO: manage the AttachLink return values, if one of them is false, delete the link.

				//attach link to the node:
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

		if (mouseAboveAnchor.mouseAbove)
			mouseAboveAnchorInfo = mouseAboveAnchor;
			
		//if you press the mouse above an anchor, start the link drag
		if (mouseAboveAnchor.mouseAbove && e.type == EventType.MouseDown && e.button == 0)
			BeginDragLink();
		
		if (mouseAboveAnchor.mouseAbove
				&& draggingLink
				&& startDragAnchor.anchorId != mouseAboveAnchorInfo.anchorId
				&& PWNode.AnchorAreAssignable(mouseAboveAnchor, startDragAnchor))
			HighlightDeleteAnchor(mouseAboveAnchor);

		//draw links:
		var links = node.GetLinks();
		int		i = 0;
		Handles.BeginGUI();
		foreach (var link in links)
		{
			// Debug.Log("link: " + link.localNodeId + ":" + link.localAnchorId + " to " + link.distantNodeId + ":" + link.distantAnchorId);
			var fromWindow = FindNodeById(link.localNodeId);
			var toWindow = FindNodeById(link.distantNodeId);

			if (toWindow == null) //invalid window ids
			{
				node.DeleteLinkByWindowTarget(link.distantNodeId);
				Debug.LogWarning("window not found: " + link.distantNodeId);
				continue ;
			}
			Rect? fromAnchor = fromWindow.GetAnchorRect(link.localAnchorId);
			Rect? toAnchor = toWindow.GetAnchorRect(link.distantAnchorId);
			if (fromAnchor != null && toAnchor != null)
			{
				DrawNodeCurve(fromAnchor.Value, toAnchor.Value, i++, link);
				if (currentLinks.Count <= linkIndex)
					currentLinks.Add(link);
				else
					currentLinks[linkIndex] = link;
				linkIndex++;
			}
		}
		Handles.EndGUI();

		//display the process time of the window (if not 0)
		if (node.processTime > Mathf.Epsilon)
		{
			GUIStyle gs = new GUIStyle();
			Rect msRect = PWUtils.DecalRect(node.windowRect, currentGraph.graphDecalPosition);
			msRect.position += new Vector2(msRect.size.x / 2 - 10, msRect.size.y + 5);
			gs.normal.textColor = greenRedGradient.Evaluate(node.processTime / 20); //20ms ok, after is red
			GUI.Label(msRect, node.processTime + " ms", gs);
		}

		//check if user have pressed the close button of this window:
		if (node.WindowShouldClose())
			DeleteNode(index);
	}

#endregion

#region Graph core rendering

	void DrawNodeGraphCore()
	{
		Event		e = Event.current;
		EventType	savedEventType;
		bool		savedIsMouse;
		int			i;
		
		//check if we have an event outside of the graph core area
		savedEventType = e.type;
		savedIsMouse = e.isMouse;
		if ((e.isMouse) && currentGraph.h2.mousePanel != 2)
			e.type = EventType.Ignore;
		
		float	scale = 2f;

		//background grid
		GUI.DrawTextureWithTexCoords(
			new Rect(currentGraph.graphDecalPosition.x % 128 - 128, currentGraph.graphDecalPosition.y % 128 - 128, maxSize.x, maxSize.y),
			nodeEditorBackgroundTexture, new Rect(0, 0, (maxSize.x / nodeEditorBackgroundTexture.width) * scale,
			(maxSize.y / nodeEditorBackgroundTexture.height) * scale)
		);

		//rendering the selection rect
		if (e.type == EventType.mouseDrag && e.button == 0 && selecting)
			selectionRect.size = e.mousePosition - selectionRect.position;
		if (selecting)
		{
			Rect posiviteSelectionRect = PWUtils.CreateRect(selectionRect.min, selectionRect.max);
			Rect decaledSelectionRect = PWUtils.DecalRect(posiviteSelectionRect, -currentGraph.graphDecalPosition);
			GUI.Label(selectionRect, "", selectionStyle);
			currentGraph.ForeachAllNodes(n => n.selected = decaledSelectionRect.Overlaps(n.windowRect), false, true);
		}

		//multiple window drag:
		if (draggingSelectedNodes)
		{
				currentGraph.ForeachAllNodes(n => {
				if (n.selected)
					n.windowRect.position += e.mousePosition - lastMousePosition;
				}, false, true);
		}

		//ordering group rendering
		mouseAboveOrderingGroup = null;
		foreach (var orderingGroup in currentGraph.orderingGroups)
		{
			if (orderingGroup.Render(currentGraph.graphDecalPosition, position.size))
				mouseAboveOrderingGroup = orderingGroup;
		}

		//node rendering
		EditorGUILayout.BeginHorizontal();
		{
			//We run the calcul the nodes:
			if (e.type == EventType.Layout)
			{
				currentGraph.ForeachAllNodes(n => n.BeginFrameUpdate(), true, true);

				if (graphNeedReload)
				{
					graphNeedReload = false;
					
					terrainMaterializer.DestroyAllChunks();

					//load another instance of the current graph to separate calls:
					if (terrainMaterializer.graph != null && terrainMaterializer.graph.GetHashCode() != currentGraph.GetHashCode())
						DestroyImmediate(terrainMaterializer.graph);
					terrainMaterializer.InitGraph(CloneGraph(currentGraph));

					Debug.Log("currentGraph: " + currentGraph.GetHashCode() + " , terrainMat: " + terrainMaterializer.graph.GetHashCode());
					//process the instance of the graph in our editor so we can see datas on chunk 0, 0, 0
					currentGraph.realMode = false;
					currentGraph.ForeachAllNodes(n => n.UpdateCurrentGraph(currentGraph));
					currentGraph.UpdateChunkPosition(Vector3.zero);

					if (graphNeedReloadOnce)
						currentGraph.ProcessGraphOnce();
					graphNeedReloadOnce = false;

					currentGraph.ProcessGraph();
				}
				//updateChunks will update and generate new chunks if needed.
				//TODOMAYBE: remove this when workers will be added to the Terrain.
				terrainMaterializer.UpdateChunks();
			}
			if (e.type == EventType.KeyDown && e.keyCode == KeyCode.S)
			{
				e.Use();
				AssetDatabase.SaveAssets();
			}

			bool	mouseAboveAnchorLocal = false;
			bool	draggingNodeLocal = false;
			int		nodeId = 0;
			linkIndex = 0;

			if (!draggingSelectedNodesFromContextMenu)
				draggingSelectedNodes = false;
			mouseAboveNodeIndex = -1;

			PWNode.windowRenderOrder = 0;

			//reset the link hover:
			foreach (var l in currentLinks)
				l.hover = false;

			BeginWindows();
			for (i = 0; i < currentGraph.nodes.Count; i++)
			{
				var node = currentGraph.nodes[i];
				if (node == null)
					continue ;
				string nodeName = (string.IsNullOrEmpty(node.externalName)) ? node.nodeTypeName : node.externalName;
				RenderNode(nodeId++, node, nodeName, i, ref mouseAboveAnchorLocal, ref draggingNodeLocal);
			}

			//display the upper graph reference:
			RenderNode(nodeId++, currentGraph.outputNode, "output", -2, ref mouseAboveAnchorLocal, ref draggingNodeLocal);

			EndWindows();

			//click up outside of an anchor, stop dragging
			if (e.type == EventType.mouseUp && draggingLink)
				StopDragLink(false);

			Rect snappedToAnchorMouseRect = new Rect((int)e.mousePosition.x, (int)e.mousePosition.y, 0, 0);

			if (mouseAboveNodeAnchor && draggingLink)
			{
				if (startDragAnchor.fieldType != null && mouseAboveAnchorInfo.fieldType != null)
					if (PWNode.AnchorAreAssignable(startDragAnchor, mouseAboveAnchorInfo))
					{
						if (mouseAboveNode != null)
							mouseAboveNode.AnchorBeingLinked(mouseAboveAnchorInfo.anchorId);
						snappedToAnchorMouseRect = mouseAboveAnchorInfo.anchorRect;
					}
			}

			//duplicate selected items if cmd+d:
			if (e.command && e.keyCode == KeyCode.D && e.type == EventType.KeyDown)
			{
				//duplicate the selected nodes
				var dupnList = new List< PWNode >();
				foreach (var node in currentGraph.nodes)
				{
					if (node.selected)
						dupnList.Add(Instantiate(node));
					node.selected = false;
				}

				foreach (var toAdd in dupnList)
				{
					CreateNewNode(toAdd, toAdd.windowRect.position + new Vector2(40, 40), toAdd.name, true);
					toAdd.nodeId = currentGraph.localNodeIdCount++;
					toAdd.DeleteAllLinks(false);
					toAdd.selected = true;
				}

				e.Use();
			}

			//draw the dragging link
			if (draggingLink)
				DrawNodeCurve(
					new Rect((int)startDragAnchor.anchorRect.center.x, (int)startDragAnchor.anchorRect.center.y, 0, 0),
					snappedToAnchorMouseRect,
					-1,
					null
				);
			mouseAboveNodeAnchor = mouseAboveAnchorLocal;
			draggingNode = draggingNodeLocal;
			
			//unselect all selected links if click beside.
			if (e.type == EventType.MouseDown && !currentLinks.Any(l => l.hover) && draggingGraph == false)
				foreach (var l in currentLinks)
					if (l.selected)
					{
						l.selected = false;
						l.linkHighlight = PWLinkHighlight.None;
					}

			//notifySetDataChanged management
			bool	reloadRequested = false;
			bool	biomeReload = false;
			PWNode	reloadRequestedNode = null;
			int		reloadWeight = 0;
			currentGraph.ForeachAllNodes(p => {
				if (e.type == EventType.Layout)
				{
					if (p.notifyDataChanged || p.notifyBiomeDataChanged)
					{
						biomeReload = p.notifyBiomeDataChanged;
						graphNeedReload = true;
						p.notifyDataChanged = false;
						p.notifyBiomeDataChanged = false;
						reloadRequested = true;
						reloadWeight = p.computeOrder;
						reloadRequestedNode = p;
					}
				}
			}, true, true);

			if (reloadRequested)
			{
				currentGraph.ForeachAllNodes(n => {
					if (n.computeOrder >= reloadWeight)
					{
						if (biomeReload)
							n.biomeReloadRequested = true;
						else
							n.reloadRequested = true;
						n.SetReloadReuqestedNode(reloadRequestedNode);
					}
				}, true, true);
			}
		}
		EditorGUILayout.EndHorizontal();
		
		if (savedIsMouse && currentGraph.h2.mousePanel != 2)
			e.type = savedEventType;
	}

#endregion

}
