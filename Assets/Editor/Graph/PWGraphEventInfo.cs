using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW;
using PW.Core;
using PW.Node;

public class PWGraphEventInfo {

	//graph infos:
	public bool				isMouseClickOutside;
	public bool				isSelecting;
	public bool				isDraggingSelected;
	public int				selectedNodeCount;

	//node infos:
	public bool				isMouseOverNode;
	public bool				isDraggingNode;
	public bool				isMouseClickOnNode;
	public PWNode			mouseOverNode;

	//link infos:
	public bool				isMouseOverLink;
	public bool				isMouseClickOnLink;
	public PWNodeConnection	mouseOverNodeConnection;

	//anchor infos:
	public bool				isMouseOverAnchor;
	public bool				isMouseClickOnAnchor;
	public PWNodeAnchor		mouseOverNodeAnchor;

	//ordering group infos:
	public bool				isMouseInsideOrderingGroup;
	public bool				isMouseClickOnOrderingGroup;
	public bool				isDraggingOrderingGroup;
	public bool				isResizingOrderingGroup;
	public PWOrderingGroup	mouseOverOrderingGroup;

	public PWGraphEventInfo() { Reset(); }

	public void Reset()
	{
		isMouseClickOutside = false;
		isSelecting = false;
		isDraggingSelected = false;
		selectedNodeCount = 0;

		isMouseOverNode = false;
		isMouseClickOnNode = false;
		isDraggingNode = false;
		mouseOverNode = null;

		isMouseOverLink = false;
		isMouseClickOnLink = false;
		mouseOverNodeConnection = null;

		isMouseOverAnchor = false;
		isMouseClickOnAnchor = false;
		mouseOverNodeAnchor = null;

		isMouseInsideOrderingGroup = false;
		isMouseClickOnOrderingGroup = false;
		isDraggingOrderingGroup = false;
		isResizingOrderingGroup = false;
		mouseOverOrderingGroup = null;
	}
}
