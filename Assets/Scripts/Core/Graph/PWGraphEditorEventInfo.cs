using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW;
using PW.Core;
using PW.Node;

public class PWGraphEditorEventInfo {

	//graph infos:
	public bool				isMouseClickOutside;
	public bool				isSelecting;
	public Vector2			selectionStartPoint;
	public bool				isDraggingSelectedNodes;
	public int				selectedNodeCount;

	//node infos:
	public bool				isMouseOverNode;
	public bool				isDraggingNode;
	public bool				isMouseClickOnNode;
	public PWNode			mouseOverNode;

	//link infos:
	public bool				isMouseOverLink;
	public bool				isMouseClickOnLink;
	public bool				isDraggingLink;
	public PWNodeLink		mouseOverLink;

	//anchor infos:
	public bool				isMouseOverAnchor;
	public bool				isMouseClickOnAnchor;
	public PWAnchor			mouseOverAnchor;

	//ordering group infos:
	public bool				isMouseOverOrderingGroup;
	public bool				isMouseClickOnOrderingGroup;	//not implemented
	public bool				isDraggingOrderingGroup;		//not implemented
	public bool				isResizingOrderingGroup;		//not implemented
	public PWOrderingGroup	mouseOverOrderingGroup;

	public PWGraphEditorEventInfo() { Reset(); }

	//init defaut values
	public void Init()
	{
		isMouseClickOutside = false;
		isSelecting = false;
		selectionStartPoint = Vector2.zero;
		isDraggingSelectedNodes = false;
		selectedNodeCount = 0;

		isMouseOverNode = false;
		isMouseClickOnNode = false;
		isDraggingNode = false;
		mouseOverNode = null;

		isMouseOverLink = false;
		isMouseClickOnLink = false;
		isDraggingLink = false;
		mouseOverLink = null;

		isMouseOverAnchor = false;
		isMouseClickOnAnchor = false;
		mouseOverAnchor = null;

		isMouseOverOrderingGroup = false;
		isMouseClickOnOrderingGroup = false;
		isDraggingOrderingGroup = false;
		isResizingOrderingGroup = false;
		mouseOverOrderingGroup = null;
	}

	//events that will be reset each frames
	public void Reset()
	{
		isMouseClickOutside = false;
		
		isMouseOverNode = false;
		isMouseClickOnNode = false;
		mouseOverNode = null;
		
		isMouseOverLink = false;
		isMouseClickOnLink = false;
		mouseOverLink = null;

		isMouseOverAnchor = false;
		isMouseClickOnAnchor = false;
		mouseOverAnchor = null;
		
		isMouseOverOrderingGroup = false;
		isMouseClickOnOrderingGroup = false;
		mouseOverOrderingGroup = null;
	}
}
