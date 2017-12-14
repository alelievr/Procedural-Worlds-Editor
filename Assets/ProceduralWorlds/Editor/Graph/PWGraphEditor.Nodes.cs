using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using PW.Core;
using PW.Node;
using PW;

//Nodes rendering
public partial class PWGraphEditor
{

	void RenderDecaledNode(int id, PWNode node)
	{
		var		e = Event.current;

		//node grid snapping when pressing cmd/crtl 
		if (node.isDragged && e.command)
		{
			Vector2 pos = node.rect.position;
			//aproximative grid cell size
			float	snapPixels = 25.6f;

			pos.x = Mathf.RoundToInt(Mathf.RoundToInt(pos.x / snapPixels) * snapPixels);
			pos.y = Mathf.RoundToInt(Mathf.RoundToInt(pos.y / snapPixels) * snapPixels);
			node.rect.position = pos;
		}

		//move the node if panPosition changed:
		node.rect = PWUtils.DecalRect(node.rect, graph.panPosition);
		Rect decaledRect = GUILayout.Window(id, node.rect, node.OnWindowGUI, node.name, (node.isSelected) ? nodeSelectedStyle : nodeStyle, GUILayout.Height(node.viewHeight));
		node.rect = PWUtils.DecalRect(decaledRect, -graph.panPosition);

		if (node.debug)
		{
			Rect debugRect = decaledRect;
			debugRect.y -= 20;
			EditorGUI.LabelField(debugRect, "comp order: " + node.computeOrder + " | can work: " + node.canWork);

			//more debug here ?
		}
	}

	void RenderNode(int id, PWNode node)
	{
		Event	e = Event.current;

		RenderDecaledNode(id, node);

		//check if the mouse is over this node
		if (node.rect.Contains(e.mousePosition - graph.panPosition))
		{
			graph.editorEvents.mouseOverNode = node;
			graph.editorEvents.isMouseOverNodeFrame = true;
		}

		//display the process time of the window (if not 0)
		if (node.processTime > Mathf.Epsilon)
		{
			GUIStyle gs = new GUIStyle();
			Rect msRect = PWUtils.DecalRect(node.rect, graph.panPosition);
			msRect.position += new Vector2(msRect.size.x / 2 - 10, msRect.size.y + 5);
			gs.normal.textColor = greenRedGradient.Evaluate(node.processTime / 20); //20ms ok, after is red
			GUI.Label(msRect, node.processTime + " ms", gs);
		}
	}
	
	void DeleteSelectedNodes()
	{
		List< PWNode > nodeToRemove = new List< PWNode >();

		foreach (var node in graph.nodes)
			if (node.isSelected)
				nodeToRemove.Add(node);

		foreach (var node in nodeToRemove)
			graph.RemoveNode(node);
	}

	void MoveSelectedNodes()
	{
		Debug.Log("moving from context menu");
	}

	void OnNodeAddedCallback(PWNode node)
	{
		AssetDatabase.AddObjectToAsset(node, graph);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Debug.Log("Added node to asset !");
	}

	void OnNodeRemovedCallback(PWNode node)
	{
		DestroyImmediate(node, true);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

}
