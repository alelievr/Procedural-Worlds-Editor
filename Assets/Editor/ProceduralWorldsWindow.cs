// #define		DEBUG_GRAPH

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
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
	
	static GUIStyle	whiteText;
	static GUIStyle	whiteBoldText;
	static GUIStyle	splittedPanel;

	int					currentPickerWindow;

	[SerializeField]
	public PWNodeGraph	currentGraph;

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

	[System.NonSerializedAttribute]
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

	[System.NonSerializedAttribute]
	Dictionary< string, Dictionary< string, FieldInfo > > bakedNodeFields = new Dictionary< string, Dictionary< string, FieldInfo > >();

	[MenuItem("Window/Procedural Worlds")]
	static void Init()
	{
		ProceduralWorldsWindow window = (ProceduralWorldsWindow)EditorWindow.GetWindow (typeof (ProceduralWorldsWindow));

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

		//setup nodeList:
		foreach (var n in nodeSelectorList)
			n.Value.Clear();
		AddToSelector("Simple values", "Slider", typeof(PWNodeSlider));
		AddToSelector("Operations", "Add", typeof(PWNodeAdd));
		AddToSelector("Debug", "DebugLog", typeof(PWNodeDebugLog));
		AddToSelector("Noise masks", "Circle Noise Mask", typeof(PWNodeCircleNoiseMask));

		//bake the fieldInfo types:
		bakedNodeFields.Clear();
		foreach (var nodeCat in nodeSelectorList)
			foreach (var nodeClass in nodeCat.Value)
			{
				var dico = new Dictionary< string, FieldInfo >();
				bakedNodeFields[nodeClass.nodeType.AssemblyQualifiedName] = dico;

				foreach (var field in nodeClass.nodeType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
					dico[field.Name] = field;
			}
		
		if (currentGraph == null)
			currentGraph = new PWNodeGraph();
			
		//initialize graph the first time he was created
		if (currentGraph.firstInitialization == null)
		{
			//setup splitted panels:
			currentGraph.h1 = new HorizontalSplitView(resizeHandleTex, position.width - 250, position.width / 2, position.width - 4);
			currentGraph.h2 = new HorizontalSplitView(resizeHandleTex, 300, 0, position.width / 2);

			currentGraph.firstInitialization = "initialized";
			currentGraph.localWindowIdCount = 0;

			currentGraph.name = "New ProceduralWorld";
		}
		
		//clear the corrupted node:
		for (int i = 0; i < currentGraph.nodes.Count; i++)
			if (currentGraph.nodes[i] == null)
				currentGraph.nodes.RemoveAt(i--);

		EvaluateComputeOrder();
	}

    void OnGUI()
    {
		EditorUtility.SetDirty(this);

		//text colors:
		whiteText = new GUIStyle();
		whiteText.normal.textColor = Color.white;
		whiteBoldText = new GUIStyle();
		whiteBoldText.fontStyle = FontStyle.Bold;
		whiteBoldText.normal.textColor = Color.white;

		//esc key event:
		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
		{
			if (currentGraph.draggingLink)
				currentGraph.draggingLink = false;
		}

        //background color:
		if (backgroundTex == null || currentGraph.h1 == null || resizeHandleTex == null)
			OnEnable();
		GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), backgroundTex, ScaleMode.StretchToFill);

		DrawNodeGraphCore();

		currentGraph.h1.UpdateMinMax(position.width / 2, position.width - 4);
		currentGraph.h2.UpdateMinMax(0, position.width / 2);

		currentGraph.h1.Begin();
		Rect p1 = currentGraph.h2.Begin(backgroundTex);
		DrawLeftBar(p1);
		Rect g = currentGraph.h2.Split(resizeHandleTex);
		DrawNodeGraphHeader(g);
		currentGraph.h2.End();
		Rect p2 = currentGraph.h1.Split(resizeHandleTex);
		DrawSelector(p2);
		currentGraph.h1.End();

		//if event, repaint
		if (Event.current.type == EventType.mouseDown
			|| Event.current.type == EventType.mouseDrag
			|| Event.current.type == EventType.mouseUp
			|| Event.current.type == EventType.scrollWheel
			|| Event.current.type == EventType.KeyDown
			|| Event.current.type == EventType.KeyUp)
			Repaint();
    }

	void DrawLeftBar(Rect currentRect)
	{
		GUI.DrawTexture(currentRect, backgroundTex);
		currentGraph.leftBarScrollPosition = EditorGUILayout.BeginScrollView(currentGraph.leftBarScrollPosition, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginVertical(splittedPanel);
			{
				EditorGUILayout.LabelField("Procedural Worlds Editor", whiteText);

				EditorGUILayout.TextField("ProceduralWorld name: ", currentGraph.name);
		
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Load graph"))
				{
					UnityEngine.Object selected = null;
					currentPickerWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
					EditorGUIUtility.ShowObjectPicker< PWNodeGraph >(null, false, "", currentPickerWindow);
                    if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow)
                    {
                    	selected = EditorGUIUtility.GetObjectPickerObject();
						if (selected != null)
							currentGraph = (PWNodeGraph)selected;
                    }
				}
				else if (GUILayout.Button("Save this graph"))
				{
                    string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                    if (path == "")
                    {
                        path = "Assets";
                    }
                    else if (Path.GetExtension(path) != "")
                    {
                        path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
                    }

                    string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + currentGraph.name + ".asset");

                    AssetDatabase.CreateAsset(currentGraph, assetPathAndName);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = currentGraph;
                }
				EditorGUILayout.EndHorizontal();

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
		currentGraph.selectorScrollPosition = EditorGUILayout.BeginScrollView(currentGraph.selectorScrollPosition, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginVertical(splittedPanel);
			{
				EditorGUIUtility.labelWidth = 0;
				EditorGUIUtility.fieldWidth = 0;
				GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
				{
					currentGraph.searchString = GUILayout.TextField(currentGraph.searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
					if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
					{
						// Remove focus if cleared
						currentGraph.searchString = "";
						GUI.FocusControl(null);
					}
				}
				GUILayout.EndHorizontal();
				
				Rect r = EditorGUILayout.GetControlRect();
				foreach (var nodeCategory in nodeSelectorList)
				{
					DrawSelectorCase(ref r, nodeCategory.Key, true);
					foreach (var nodeCase in nodeCategory.Value.Where(n => n.name.IndexOf(currentGraph.searchString, System.StringComparison.OrdinalIgnoreCase) >= 0))
					{
						Rect clickableRect = DrawSelectorCase(ref r, nodeCase.name);
	
						if (Event.current.type == EventType.MouseDown && clickableRect.Contains(Event.current.mousePosition))
						{
							PWNode newNode = ScriptableObject.CreateInstance(nodeCase.nodeType) as PWNode;
							//center to the middle of the screen:
							newNode.windowRect.position = -currentGraph.graphDecalPosition + new Vector2((int)(position.width / 2), (int)(position.height / 2));
							newNode.SetWindowId(currentGraph.localWindowIdCount++);
							newNode.nodeTypeName = nodeCase.name;
							currentGraph.nodes.Add(newNode);
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

		//remove 4 pixels for the separation bar
		graphRect.size -= Vector2.right * 4;

		#if (DEBUG_GRAPH)
		foreach (var node in nodes)
			GUI.DrawTexture(PWUtils.DecalRect(node.rect, graphDecalPosition), debugTexture1);
		#endif

		if (Event.current.type == EventType.MouseDown //if event is mouse down
			&& !currentGraph.mouseAboveNodeAnchor //if mouse is not above a node anchor
			&& graphRect.Contains(Event.current.mousePosition) //and mouse position is in graph
			&& !currentGraph.nodes.Any(n => PWUtils.DecalRect(n.windowRect,currentGraph. graphDecalPosition, true).Contains(Event.current.mousePosition))) //and mouse is not above a window
			currentGraph.dragginGraph = true;
		if (currentGraph.dragginGraph)
			currentGraph.graphDecalPosition += Event.current.mousePosition - currentGraph.lastMousePosition;
		if (Event.current.type == EventType.MouseUp)
			currentGraph.dragginGraph = false;
		currentGraph.lastMousePosition = Event.current.mousePosition;
		EditorGUILayout.EndVertical();
	}

	string GetUniqueName(string name)
	{
		while (true)
		{
			if (!currentGraph.nodes.Any(p => p.name == name))
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
			for (int i = 0; i < currentGraph.nodes.Count; i++)
			{
				//window:
				GUI.depth =currentGraph. nodes[i].computeOrder;
				currentGraph.nodes[i].UpdateGraphDecal(currentGraph.graphDecalPosition);
				currentGraph.nodes[i].windowRect = PWUtils.DecalRect(currentGraph.nodes[i].windowRect, currentGraph.graphDecalPosition);
				Rect decaledRect = GUILayout.Window(i, currentGraph.nodes[i].windowRect, currentGraph.nodes[i].OnWindowGUI, currentGraph.nodes[i].nodeTypeName);
				currentGraph.nodes[i].windowRect = PWUtils.DecalRect(decaledRect, -currentGraph.graphDecalPosition);

				//highlight, hide, add all linkable anchors:
				if (currentGraph.draggingLink)
					currentGraph.nodes[i].HighlightLinkableAnchorsTo(currentGraph.startDragAnchor);
				currentGraph.nodes[i].DisplayHiddenMultipleAnchors(currentGraph.draggingLink);

				//process envent, state and position for node anchors:
				var mouseAboveAnchor = currentGraph.nodes[i].ProcessAnchors();
				if (mouseAboveAnchor.mouseAbove)
					mouseAboveAnchorLocal = true;

				//if you press the mouse above an anchor, start the link drag
				if (mouseAboveAnchorLocal && mouseAboveAnchor.mouseAbove && e.type == EventType.MouseDown)
				{
					currentGraph.startDragAnchor = mouseAboveAnchor;
					currentGraph.draggingLink = true;
				}

				//render node anchors:
				currentGraph.nodes[i].RenderAnchors();
				
				//we render the window (it will also compute the result)
	
				//end dragging:
				if (e.type == EventType.mouseUp && currentGraph.draggingLink == true)
					if (mouseAboveAnchor.mouseAbove)
					{
						//attach link to the node:
						currentGraph.nodes[i].AttachLink(mouseAboveAnchor, currentGraph.startDragAnchor);
						var win = currentGraph.nodes.FirstOrDefault(n => n.windowId == currentGraph.startDragAnchor.windowId);
						if (win != null)
							win.AttachLink(currentGraph.startDragAnchor, mouseAboveAnchor);
						else
							Debug.LogWarning("window id not found: " + currentGraph.startDragAnchor.windowId);
						
						//Recalcul the compute order:
						EvaluateComputeOrder();

						currentGraph.draggingLink = false;
					}

				//draw links:
				var links = currentGraph.nodes[i].GetLinks();
				foreach (var link in links)
				{
					// Debug.Log("link: " + link.localWindowId + ":" + link.localAnchorId + " to " + link.distantWindowId + ":" + link.distantAnchorId);
					var fromWindow = currentGraph.nodes.FirstOrDefault(n => n.windowId == link.localWindowId);
					var toWindow = currentGraph.nodes.FirstOrDefault(n => n.windowId == link.distantWindowId);

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
				if (currentGraph.nodes[i].WindowShouldClose())
					currentGraph.nodes.RemoveAt(i);
			}
			EndWindows();
			
			if (e.type == EventType.Repaint)
				foreach (var node in currentGraph.nodes)
				{
					node.OnNodeProcess();
		
					var links = node.GetLinks();

					foreach (var link in links)
					{
						var target = currentGraph.nodes.FirstOrDefault(n => n.windowId == link.distantWindowId);

						if (target == null)
							continue ;

						var val = bakedNodeFields[link.localClassAQName][link.localName].GetValue(node);
						var prop = bakedNodeFields[link.distantClassAQName][link.distantName];
						if (link.distantIndex == -1)
							prop.SetValue(target, val);
						else //multiple object data:
						{
							PWValues values = (PWValues)prop.GetValue(target);

							if (values != null)
								if (!values.AssignAt(link.distantIndex, val))
									Debug.Log("failed to set distant indexed field value: " + link.distantName);
						}
					}
				}

			//click up outside of an anchor, stop dragging
			if (e.type == EventType.mouseUp && currentGraph.draggingLink == true)
				currentGraph.draggingLink = false;

			if (currentGraph.draggingLink)
				DrawNodeCurve(
					new Rect((int)currentGraph.startDragAnchor.anchorRect.center.x, (int)currentGraph.startDragAnchor.anchorRect.center.y, 0, 0),
					new Rect((int)e.mousePosition.x, (int)e.mousePosition.y, 0, 0),
					currentGraph.startDragAnchor.anchorColor
				);
			currentGraph.mouseAboveNodeAnchor = mouseAboveAnchorLocal;
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
			for (int i = 0; i < currentGraph.nodes.Count; i++)
			{
				currentGraph.nodes[i].computeOrder = EvaluateComputeOrder(false, 1, currentGraph.nodes[i].windowId);
				// Debug.Log("computed order for node " + nodes[i].windowId + ": " + nodes[i].computeOrder);
			}
			//sort nodes for compute order:
			currentGraph.nodes.Sort((n1, n2) => { return n1.computeOrder.CompareTo(n2.computeOrder); });
		}

		//check if we the node have already been computed:
		if (nodeComputeOrderCount.ContainsKey(windowId))
			return nodeComputeOrderCount[windowId];

		var node = currentGraph.nodes.FirstOrDefault(n => n.windowId == windowId);
		if (node == null)
			return 0;

		//compute dependency weight:
		int	ret = 1;
		foreach (var dep in node.GetDependencies())
			ret += EvaluateComputeOrder(false, depth + 1, dep);

		nodeComputeOrderCount[windowId] = ret;
		return ret;
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