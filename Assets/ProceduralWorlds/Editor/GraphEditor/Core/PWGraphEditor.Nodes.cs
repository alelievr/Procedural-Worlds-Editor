using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;
using System.Linq;
using PW.Core;
using PW.Node;
using PW.Editor;
using PW;

//Nodes rendering
public partial class PWGraphEditor
{
	[System.NonSerialized]
	Dictionary< PWNode, PWNodeEditor > nodeEditors = new Dictionary< PWNode, PWNodeEditor >();

	void RenderDecaledNode(int id, PWNode node)
	{
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
		Rect decaledRect;

		if (!nodeEditors.ContainsKey(node))
		{
			nodeEditors[node] = Editor.CreateEditor(node) as PWNodeEditor;
			nodeEditors[node].Initialize(this);
		}
		
		decaledRect = GUILayout.Window(id, node.rect, (i) => {
			nodeEditors[node].OnInspectorGUI();
		}, node.name, (node.isSelected) ? nodeSelectedStyle : nodeStyle, GUILayout.Height(node.viewHeight));

		node.visualRect = decaledRect;
		node.rect = PWUtils.DecalRect(decaledRect, -graph.panPosition);

		//draw node header:
		//Draw the node header using the color scheme:
		if (e.type == EventType.Repaint)
		{
			float h = nodeStyle.border.top;
			float w = decaledRect.width - nodeStyle.border.right - nodeStyle.border.left;
			GUI.color = PWColorTheme.GetNodeColor(node.colorSchemeName);
			nodeHeaderStyle.Draw(new Rect(decaledRect.x, decaledRect.y, w, h), false, false, false, false);
			GUI.color = Color.white;
		}

		if (node.debug)
		{
			Rect debugRect = decaledRect;
			debugRect.y -= 20;
			EditorGUI.LabelField(debugRect, "id: " + node.id);
			debugRect.y -= 20;
			EditorGUI.LabelField(debugRect, "comp order: " + node.computeOrder + " | can work: " + node.canWork);
		}
	}

	void RenderNode(int id, PWNode node)
	{
		RenderDecaledNode(id, node);

		//check if the mouse is over this node
		if (node.rect.Contains(e.mousePosition - graph.panPosition))
		{
			graph.editorEvents.mouseOverNode = node;
			graph.editorEvents.isMouseOverNodeFrame = true;
		}

		//display the process time of the window (if > .1ms)
		if (node.processTime > .1f)
		{
			GUIStyle gs = new GUIStyle();
			Rect msRect = PWUtils.DecalRect(node.rect, graph.panPosition);
			msRect.position += new Vector2(msRect.size.x / 2 - 10, msRect.size.y + 5);
			gs.normal.textColor = greenRedGradient.Evaluate(node.processTime / 20); //20ms ok, after is red
			GUI.Label(msRect, node.processTime.ToString("F1") + " ms", gs);
		}
	}
	
	void RenderNodes()
	{
		int		nodeId = 0;

		Profiler.BeginSample("[PW] rendering nodes");
		
		BeginWindows();
		{
			foreach (var node in graph.nodes)
				RenderNode(nodeId++, node);
	
			//display the graph input and output:
			RenderNode(nodeId++, graph.outputNode);
			RenderNode(nodeId++, graph.inputNode);
		}
		EndWindows();

		Profiler.EndSample();

		//if we have a context click, editorEvents was not properly executed (cauz ContextClick is not sent to nested windows -___-)
		if (e.type == EventType.ContextClick)
			return ;

		//if mouse was not over a node this frame, unset mouseOver
		if (!editorEvents.isMouseOverNodeFrame)
			editorEvents.mouseOverNode = null;
		
		//if mouse was not over an anchor this frame, unset mouseOver
		if (!editorEvents.isMouseOverAnchorFrame)
			editorEvents.mouseOverAnchor = null;
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

	void NodeAddedCallback(PWNode node)
	{
		AssetDatabase.AddObjectToAsset(node, graph);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	void NodeRemovedCallback(PWNode node)
	{
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	void UnselectAllNodes()
	{
		foreach (var node in graph.nodes)
			node.isSelected = false;
	}

}
