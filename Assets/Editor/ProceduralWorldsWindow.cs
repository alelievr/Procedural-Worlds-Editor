// #define		DEBUG_GRAPH

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;

public class ProceduralWorldsWindow : EditorWindow {

    private static Texture2D	backgroundTex;
	private static Texture2D	resizeHandleTex;
	private static Texture2D	selectorBackgroundTex;
	private static Texture2D	debugTexture1;
	private static Texture2D	selectorCaseBackgroundTex;
	private static Texture2D	selectorCaseTitleBackgroundTex;

	[SerializeField]
	List< PWNode >				nodes = new List< PWNode >();
	
	static GUIStyle	whiteText;
	static GUIStyle	whiteBoldText;
	static GUIStyle	splittedPanel;

	[SerializeField]
	HorizontalSplitView			h1;
	[SerializeField]
	HorizontalSplitView			h2;

	[SerializeField]
	Vector2			leftBarScrollPosition;
	[SerializeField]
	Vector2			selectorScrollPosition;

	[SerializeField]
	Vector2			graphDecalPosition;
	[SerializeField]
	Vector2			lastMousePosition;
	[SerializeField]
	bool			dragginGraph = false;
	[SerializeField]
	bool			mouseAboveNodeAnchor = false;
	[SerializeField]
	int				localWindowIdCount;
	[SerializeField]
	string			firstInitialization;
	
	[SerializeField]
	PWAnchorInfo	startDragAnchor;
	[SerializeField]
	bool			draggingLink = false;
	
	[SerializeField]
	string			searchString = "";
	
	[System.SerializableAttribute]
	private class PWNodeStorage
	{
		public string		name;
		public System.Type	nodeType;
		
		public PWNodeStorage(string n, System.Type type)
		{
			name = n;
			nodeType = type;
		}
	}

	Dictionary< string, List< PWNodeStorage > > nodeSelectorList = new Dictionary< string, List< PWNodeStorage > >()
	{
		{"Simple values", new List< PWNodeStorage >()},
		{"Operations", new List< PWNodeStorage >()},
		{"Noises", new List< PWNodeStorage >()},
		{"Noise masks", new List< PWNodeStorage >()},
		{"Storage", new List< PWNodeStorage >()},
		{"Visual", new List< PWNodeStorage >()},
		{"Debug", new List< PWNodeStorage >()},
		{"Custom", new List< PWNodeStorage >()},
	};

	[MenuItem("Window/Procedural Worlds")]
	static void Init()
	{
		ProceduralWorldsWindow window = (ProceduralWorldsWindow)EditorWindow.GetWindow (typeof (ProceduralWorldsWindow));

		window.graphDecalPosition = Vector2.zero;

		window.Show();
	}
	
	void AddToSelector(string key, params object[] objs)
	{
		if (nodeSelectorList.ContainsKey(key))
		{
			for (int i = 0; i < objs.Length; i += 2)
			nodeSelectorList[key].Add(new PWNodeStorage((string)objs[i], (Type)objs[i + 1]));
		}
	}

	void OnEnable()
	{
		CreateBackgroundTexture();
		
		splittedPanel = new GUIStyle();
		splittedPanel.margin = new RectOffset(5, 0, 0, 0);

		//setup splitted panels:
		h1 = new HorizontalSplitView(resizeHandleTex, position.width - 250, position.width / 2, position.width - 4);
		h2 = new HorizontalSplitView(resizeHandleTex, 300, 0, position.width / 2);

		//setup nodeList:
		foreach (var n in nodeSelectorList)
			n.Value.Clear();
		AddToSelector("Simple values", "Slider", typeof(PWNodeSlider));
		AddToSelector("Operations", "Add", typeof(PWNodeAdd));
		AddToSelector("Debug", "DebugLog", typeof(PWNodeDebugLog));
		AddToSelector("Noise masks", "Circle Noise Mask", typeof(PWNodeCircleNoiseMask));

		if (firstInitialization == null)
		{
			localWindowIdCount = 0;
			firstInitialization = "initialized";
		}
		
		EvaluateComputeOrder();
	}

    void OnGUI()
    {
		//text colors:
		whiteText = new GUIStyle();
		whiteText.normal.textColor = Color.white;
		whiteBoldText = new GUIStyle();
		whiteBoldText.fontStyle = FontStyle.Bold;
		whiteBoldText.normal.textColor = Color.white;

		//esc key event:
		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
		{
			if (draggingLink)
				draggingLink = false;
		}

        //background color:
		if (backgroundTex == null || h1 == null)
			OnEnable();
		GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), backgroundTex, ScaleMode.StretchToFill);

		DrawNodeGraphCore();

		h1.UpdateMinMax(position.width / 2, position.width - 4);
		h2.UpdateMinMax(0, position.width / 2);

		h1.Begin();
		Rect p1 = h2.Begin(backgroundTex);
		DrawLeftBar(p1);
		Rect g = h2.Split();
		DrawNodeGraphHeader(g);
		h2.End();
		Rect p2 = h1.Split(backgroundTex);
		DrawSelector(p2);
		h1.End();

		//if event, repaint
		if (Event.current.type == EventType.mouseDown
			|| Event.current.type == EventType.mouseDrag
			|| Event.current.type == EventType.mouseUp
			|| Event.current.type == EventType.scrollWheel
			|| Event.current.type == EventType.KeyDown
			|| Event.current.type == EventType.KeyUp)
			;
			Repaint();
    }

	void DrawLeftBar(Rect currentRect)
	{
		GUI.DrawTexture(currentRect, backgroundTex);
		leftBarScrollPosition = EditorGUILayout.BeginScrollView(leftBarScrollPosition, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginVertical(splittedPanel);
			{
				EditorGUILayout.LabelField("Procedural Worlds Editor", whiteText);
		
				//TODO: draw preview view.
		
				//TODO: draw infos / debug / global settings view
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndScrollView();
	}

	Rect DrawSelectorCase(ref Rect r, string name, bool title = false)
	{
		//text box
		Rect boxRect = new Rect(r);
		boxRect.y += 2;
		boxRect.height += 10;

		if (title)
			GUI.DrawTexture(boxRect, selectorCaseTitleBackgroundTex);
		else
			GUI.DrawTexture(boxRect, selectorCaseBackgroundTex);

		boxRect.y += 6;
		boxRect.x += 10;

		EditorGUI.LabelField(boxRect, name, (title) ? whiteBoldText : whiteText);

		r.y += 30;

		return boxRect;
	}

	void DrawSelector(Rect currentRect)
	{
		GUI.DrawTexture(currentRect, selectorBackgroundTex);
		selectorScrollPosition = EditorGUILayout.BeginScrollView(selectorScrollPosition, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginVertical(splittedPanel);
			{
				EditorGUIUtility.labelWidth = 0;
				EditorGUIUtility.fieldWidth = 0;
				GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
				{
					searchString = GUILayout.TextField(searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
					if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
					{
						// Remove focus if cleared
						searchString = "";
						GUI.FocusControl(null);
					}
				}
				GUILayout.EndHorizontal();
				
				Rect r = EditorGUILayout.GetControlRect();
				foreach (var nodeCategory in nodeSelectorList)
				{
					DrawSelectorCase(ref r, nodeCategory.Key, true);
					foreach (var nodeCase in nodeCategory.Value.Where(n => n.name.IndexOf(searchString, System.StringComparison.OrdinalIgnoreCase) >= 0))
					{
						Rect clickableRect = DrawSelectorCase(ref r, nodeCase.name);
	
						if (Event.current.type == EventType.MouseDown && clickableRect.Contains(Event.current.mousePosition))
						{
							PWNode newNode = ScriptableObject.CreateInstance(nodeCase.nodeType) as PWNode;
							//center to the middle of the screen:
							newNode.windowRect.position = -graphDecalPosition + new Vector2((int)(position.width / 2), (int)(position.height / 2));
							newNode.SetWindowId(localWindowIdCount++);
							newNode.nodeTypeName = nodeCase.name;
							nodes.Add(newNode);
							Debug.Log("added node of type: " + nodeCase.nodeType);
						}
					}
				}
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndScrollView();
	}
	
	void DrawNodeGraphHeader(Rect graphRect)
	{
		EditorGUILayout.BeginVertical(splittedPanel);

		#if (DEBUG_GRAPH)
		foreach (var node in nodes)
			GUI.DrawTexture(PWUtils.DecalRect(node.rect, graphDecalPosition), debugTexture1);
		#endif

		if (Event.current.type == EventType.MouseDown //if event is mouse down
			&& !mouseAboveNodeAnchor //if mouse is not above a node anchor
			&& graphRect.Contains(Event.current.mousePosition) //and mouse position is in graph
			&& !nodes.Any(n => PWUtils.DecalRect(n.windowRect, graphDecalPosition, true).Contains(Event.current.mousePosition))) //and mouse is not above a window
			dragginGraph = true;
		if (dragginGraph)
			graphDecalPosition += Event.current.mousePosition - lastMousePosition;
		if (Event.current.type == EventType.MouseUp)
			dragginGraph = false;
		lastMousePosition = Event.current.mousePosition;
		EditorGUILayout.EndVertical();
	}

	string GetUniqueName(string name)
	{
		while (true)
		{
			if (!nodes.Any(p => p.name == name))
				return name;
			name += "*";
		}
	}

	void DrawNodeGraphCore()
	{
		Event e = Event.current;

		Rect graphRect = EditorGUILayout.BeginHorizontal();
		{
			bool	mouseAboveAnchorLocal = false;
			PWNode.windowRenderOrder = 0;
			BeginWindows();
			for (int i = 0; i < nodes.Count; i++)
			{
				//window:
				GUI.depth = nodes[i].computeOrder;
				nodes[i].UpdateGraphDecal(graphDecalPosition);
				nodes[i].windowRect = PWUtils.DecalRect(nodes[i].windowRect, graphDecalPosition);
				Rect decaledRect = GUILayout.Window(i, nodes[i].windowRect, nodes[i].OnWindowGUI, nodes[i].nodeTypeName);
				nodes[i].windowRect = PWUtils.DecalRect(decaledRect, -graphDecalPosition);

				//highlight, hide, add all linkable anchors:
				if (draggingLink)
					nodes[i].HighlightLinkableAnchorsTo(startDragAnchor);
				nodes[i].DisplayHiddenMultipleAnchors(draggingLink);

				//process envent, state and position for node anchors:
				var mouseAboveAnchor = nodes[i].ProcessAnchors();
				if (mouseAboveAnchor.mouseAbove)
					mouseAboveAnchorLocal = true;

				//if you press the mouse above an anchor, start the link drag
				if (mouseAboveAnchorLocal && mouseAboveAnchor.mouseAbove && e.type == EventType.MouseDown)
				{
					startDragAnchor = mouseAboveAnchor;
					draggingLink = true;
				}

				//render node anchors:
				nodes[i].RenderAnchors();
				
				//we render the window (it will also compute the result)
	
				//end dragging:
				if (e.type == EventType.mouseUp && draggingLink == true)
					if (mouseAboveAnchor.mouseAbove)
					{
						//attach link to the node:
						nodes[i].AttachLink(mouseAboveAnchor, startDragAnchor);
						var win = nodes.FirstOrDefault(n => n.windowId == startDragAnchor.windowId);
						if (win != null)
							win.AttachLink(startDragAnchor, mouseAboveAnchor);
						else
							Debug.LogWarning("window id not found: " + startDragAnchor.windowId);
						
						//Recalcul the compute order:
						EvaluateComputeOrder();

						draggingLink = false;
					}

				//draw links:
				var links = nodes[i].GetLinks();
				foreach (var link in links)
				{
					// Debug.Log("link: " + link.localWindowId + ":" + link.localAnchorId + " to " + link.distantWindowId + ":" + link.distantAnchorId);
					var fromWindow = nodes.FirstOrDefault(n => n.windowId == link.localWindowId);
					var toWindow = nodes.FirstOrDefault(n => n.windowId == link.distantWindowId);

					if (fromWindow == null || toWindow == null) //invalid window ids
					{
						Debug.LogWarning("window not found: " + link.localWindowId + ", " + link.distantWindowId);
						continue ;
					}

					Rect? fromAnchor = fromWindow.GetAnchorRect(link.localAnchorId);
					Rect? toAnchor = toWindow.GetAnchorRect(link.distantAnchorId);
					if (fromAnchor != null && toAnchor != null)
						DrawNodeCurve(fromAnchor.Value, toAnchor.Value, Color.black);
				}

				//check if user have pressed the close button of this window:
				if (nodes[i].WindowShouldClose())
					nodes.RemoveAt(i);
			}
			EndWindows();
			
			if (e.type == EventType.Repaint)
				foreach (var node in nodes)
				{
					node.OnNodeProcess();
		
					var links = node.GetLinks();

					foreach (var link in links)
					{
						var target = nodes.FirstOrDefault(n => n.windowId == link.distantWindowId);

						Debug.Log("target distant name: " + link.distantName);
						if (target == null)
							continue ;

						//simple value set
						//TODO: PWValues support AND OPTIMIZATION !!!!
						var f = node.GetType().GetField(link.localName).GetValue(node);
						var prop = target.GetType().GetField(link.distantName);
						prop.SetValue(target, f);
					}
				}

			//click up outside of an anchor, stop dragging
			if (e.type == EventType.mouseUp && draggingLink == true)
				draggingLink = false;

			if (draggingLink)
				DrawNodeCurve(
					new Rect((int)startDragAnchor.anchorRect.center.x, (int)startDragAnchor.anchorRect.center.y, 0, 0),
					new Rect((int)e.mousePosition.x, (int)e.mousePosition.y, 0, 0),
					startDragAnchor.anchorColor
				);
			mouseAboveNodeAnchor = mouseAboveAnchorLocal;
		}
		EditorGUILayout.EndHorizontal();
	}

	//Dictionary< windowId, dependencyWeight >
	Dictionary< int, int > nodeComputeOrderCount = new Dictionary< int, int >();
	int EvaluateComputeOrder(bool first = true, int depth = 0, int windowId = -1)
	{
		//Recursively evaluate compute order for each nodes:
		if (first)
		{
			nodeComputeOrderCount.Clear();
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i].computeOrder = EvaluateComputeOrder(false, 1, nodes[i].windowId);
				Debug.Log("computed order for node " + nodes[i].windowId + ": " + nodes[i].computeOrder);
			}
			//sort nodes for compute order:
			nodes.Sort((n1, n2) => { return n1.computeOrder.CompareTo(n2.computeOrder); });
		}

		//check if we the node have already been computed:
		if (nodeComputeOrderCount.ContainsKey(windowId))
			return nodeComputeOrderCount[windowId];

		var node = nodes.FirstOrDefault(n => n.windowId == windowId);
		if (node == null)
			return 0;

		//compute dependency weight:
		int	ret = 1;
		foreach (var dep in node.GetDependencies())
		{
			Debug.Log("dependency for window " + windowId + ": " + dep);
			ret += EvaluateComputeOrder(false, depth + 1, dep);
		}

		nodeComputeOrderCount[windowId] = ret;
		return ret;

		/*int i = 0;
		var	dependenciesPerNodes =	(from node in nodes
									select new { deps = node.GetDependencies(), windowId = node.windowId, index = i++})
									.ToDictionary(x => x.windowId, x => x);
		
		foreach (var depKP in dependenciesPerNodes)
		{
			var dep = depKP.Value;
			nodes[dep.index].computeOrder = dep.deps.Count;
		}

		for (i = 0; i < nodes.Count; i++)
		{
			foreach (var dep in dependenciesPerNodes[nodes[i].windowId].deps)
			{
				//add the compute order of the dependencies.
				nodes[i].computeOrder += nodes[i].computeOrder;
			}
		}*/
	}

	static void CreateBackgroundTexture()
	{
        Color backgroundColor = new Color32(56, 56, 56, 255);
		Color resizeHandleColor = EditorGUIUtility.isProSkin
			? new Color32(56, 56, 56, 255)
            : new Color32(130, 130, 130, 255);
		Color selectorBackgroundColor = new Color32(80, 80, 80, 255);
		Color selectorCaseBackgroundColor = new Color32(110, 110, 110, 255);
		Color selectorCaseTitleBackgroundColor = new Color32(50, 50, 50, 255);
		
		backgroundTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		backgroundTex.SetPixel(0, 0, backgroundColor);
		backgroundTex.Apply();

		resizeHandleTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		resizeHandleTex.SetPixel(0, 0, resizeHandleColor);
		resizeHandleTex.Apply();

		selectorBackgroundTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		selectorBackgroundTex.SetPixel(0, 0, selectorBackgroundColor);
		selectorBackgroundTex.Apply();

		debugTexture1 = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		debugTexture1.SetPixel(0, 0, new Color(1f, 0f, 0f, .3f));
		debugTexture1.Apply();
		
		selectorCaseBackgroundTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		selectorCaseBackgroundTex.SetPixel(0, 0, selectorCaseBackgroundColor);
		selectorCaseBackgroundTex.Apply();
		
		selectorCaseTitleBackgroundTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		selectorCaseTitleBackgroundTex.SetPixel(0, 0, selectorCaseTitleBackgroundColor);
		selectorCaseTitleBackgroundTex.Apply();
	}

    void DrawNodeCurve(Rect start, Rect end, Color c)
    {
		//swap start and end if they are inverted
		if (start.xMax > end.xMax)
			PWUtils.Swap< Rect >(ref start, ref end);

        Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + Vector3.right * 100;
        Vector3 endTan = endPos + Vector3.left * 100;
        Color shadowCol = c;
		shadowCol.a = 0.04f;

        for (int i = 0; i < 3; i++)
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);

        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
    }
}