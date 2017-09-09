using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using PW.Core;
using PW.Node;

using Debug = UnityEngine.Debug;

//Utils for graph editor
public partial class PWGraphEditor {
	
	void HighlightDeleteAnchor(PWAnchorInfo anchor)
	{
		//anchor is input type.
		PWLink link = FindLinkFromAnchor(anchor);

		if (link != null)
			link.linkHighlight = PWLinkHighlight.DeleteAndReset;
	}

	void BeginDragLink()
	{
		startDragAnchor = mouseAboveAnchorInfo;
		draggingLink = true;
		if (startDragAnchor.anchorType == PWAnchorType.Input)
		{
			var links = FindLinksFromAnchor(startDragAnchor);

			if (links != null)
				foreach (var link in links)
					link.linkHighlight = PWLinkHighlight.Delete;
		}
	}

	void StopDragLink(bool linked)
	{
		draggingLink = false;

		if (linked)
		{
			//if we are linking to an input:
			if (mouseAboveAnchorInfo.anchorType == PWAnchorType.Input && mouseAboveAnchorInfo.linkCount != 0)
			{
				PWLink link = FindLinkFromAnchor(mouseAboveAnchorInfo);

				if (link == null) //link was not created / canceled by the node
					return ;

				var from = FindNodeById(link.localNodeId);
				var to = FindNodeById(link.distantNodeId);
				
				from.DeleteLink(link.localAnchorId, to, link.distantAnchorId);
				to.DeleteLink(link.distantAnchorId, from, link.localAnchorId);
			}
			else if (mouseAboveAnchorInfo.anchorType == PWAnchorType.Output && startDragAnchor.linkCount != 0)
			{
				var inputNode = FindNodeById(startDragAnchor.nodeId);

				//find the link with inputNode:
				var toRemoveLink = FindLinkFromAnchor(startDragAnchor);

				var outputNode = FindNodeById(toRemoveLink.localNodeId);

				//delete links:
				outputNode.DeleteLink(mouseAboveAnchorInfo.anchorId, inputNode, startDragAnchor.anchorId);

				//delete dependencies:
				inputNode.DeleteDependency(toRemoveLink.localNodeId, toRemoveLink.localAnchorId);
			}
		}
		else if (startDragAnchor.linkCount != 0)
		{
			PWLink link = FindLinkFromAnchor(startDragAnchor);

			//disable delete highlight for link
			if (link != null)
				link.linkHighlight = PWLinkHighlight.None;
		}
	}

	IEnumerable< PWLink > FindLinksFromAnchor(PWAnchorInfo anchor)
	{
		if (anchor.anchorType == PWAnchorType.Input)
		{
			//find the anchor node
			var node = FindNodeById(anchor.nodeId);
			if (node == null)
				return null;

			//get dependencies of this anchor
			var deps = node.GetDependencies(anchor.anchorId);
			if (deps.Count() == 0)
				return null;

			//get the linked window from the dependency
			var linkNode = FindNodeById(deps.First().nodeId);
			if (linkNode == null)
				return null;

			//find the link of each dependency
			List< PWLink > links = new List< PWLink >();
			foreach (var dep in deps)
				links.Add(linkNode.GetLink(dep.anchorId, node.nodeId, dep.connectedAnchorId));
			return links;
		}
		else
			return null;
	}

	PWLink FindLinkFromAnchor(PWAnchorInfo anchor)
	{
		var links = FindLinksFromAnchor(anchor);

		if (links == null || links.Count() == 0)
			return null;
		return links.First();
	}

	void DeleteAllAnchorLinks()
	{
		var node = FindNodeById(mouseAboveAnchorInfo.nodeId);
		if (node == null)
			return ;
		var anchorConnections = node.GetAnchorConnections(mouseAboveAnchorInfo.anchorId);
		foreach (var ac in anchorConnections)
		{
			var n = FindNodeById(ac.first);
			if (n != null)
			{
				if (mouseAboveAnchorInfo.anchorType == PWAnchorType.Output)
					n.DeleteDependency(mouseAboveAnchorInfo.nodeId, mouseAboveAnchorInfo.anchorId);
				else
					n.DeleteLink(ac.second, node, mouseAboveAnchorInfo.anchorId);
			}
		}
		node.DeleteAllLinkOnAnchor(mouseAboveAnchorInfo.anchorId);
		
		EvaluateComputeOrder();
	}

	void DeleteLink(object l)
	{
		PWLink	link = l  as PWLink;

		var from = FindNodeById(link.localNodeId);
		var to = FindNodeById(link.distantNodeId);

		from.DeleteLink(link.localAnchorId, to, link.distantAnchorId);
		to.DeleteLink(link.distantAnchorId, from, link.localAnchorId);
		
		EvaluateComputeOrder();
	}

	void UpdateLinkMode(PWLink link, PWNodeProcessMode newMode)
	{
		link.mode = newMode;

        var node = FindNodeById(link.distantNodeId);
		var dep = node.GetDependency(link.distantAnchorId, link.localNodeId, link.localAnchorId);
        dep.mode = newMode;
		
		currentGraph.RebakeGraphParts();
	}
	
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
