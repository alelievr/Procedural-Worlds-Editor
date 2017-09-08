// #define		DEBUG_GRAPH

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;
using PW.Core;
using PW.Node;
using Object = UnityEngine.Object;

[System.Serializable]
public class ProceduralWorldsWindow : PWGraphEditor {
	
	[SerializeField]
	public HorizontalSplitView			h1;
	[SerializeField]
	public HorizontalSplitView			h2;
	[SerializeField]
	public string						searchString = "";
	
	//graph, node, anchors and links control and 
	int					mouseAboveNodeIndex;
	bool				mouseAboveNodeAnchor;
	PWOrderingGroup		mouseAboveOrderingGroup;
	bool				draggingGraph = false;
	bool				draggingLink = false;
	bool				draggingNode = false;
	bool				draggingSelectedNodes = false;
	bool				draggingSelectedNodesFromContextMenu = false;
	public bool			graphNeedReload = false;
	public bool			graphNeedReloadOnce = false;
	bool				previewMouseDrag = false;
	bool				editorNeedRepaint = false;
	PWAnchorInfo		startDragAnchor;
	PWAnchorInfo		mouseAboveAnchorInfo;
	[System.NonSerializedAttribute]
	PWNode				mouseAboveNode;
	
	//list of all links
	int					linkIndex;
	List< PWLink >		currentLinks = new List< PWLink >();

	//events fields
	Vector2				lastMousePosition;
	Vector2				windowSize;
	[System.NonSerializedAttribute]
	Vector2				currentMousePosition;

	//terrain materializer
	PWTerrainBase		terrainMaterializer;
	int					chunkRenderDistance = 4; //chunk render distance

	//multi-node selection
	[System.NonSerializedAttribute]
	Rect				selectionRect;
	[System.NonSerializedAttribute]
	bool				selecting = false;

	//current and parent graph
	[SerializeField]
	public PWMainGraph	currentGraph;

	//node selector and his subclasses
	[System.NonSerializedAttribute]
	Dictionary< string, PWNodeStorageCategory > nodeSelectorList = new Dictionary< string, PWNodeStorageCategory >();

	[System.SerializableAttribute]
	private class PWNodeStorageCategory
	{
		public string					color;
		public List< PWNodeStorage >	nodes;

		public PWNodeStorageCategory(string color) : this()
		{
			this.color = color;
		}

		public PWNodeStorageCategory()
		{
			nodes = new List< PWNodeStorage >();
		}
	}

	[System.SerializableAttribute]
	private class PWNodeStorage
	{
		public string		name;
		public System.Type	nodeType;
		public GUIStyle		windowStyle;
		public GUIStyle		windowSelectedStyle;
		
		public PWNodeStorage(string n, System.Type type, GUIStyle ws, GUIStyle wss)
		{
			name = n;
			nodeType = type;
			windowStyle = ws;
			windowSelectedStyle = wss;
		}
	}

#region Internal editor styles and textures

	private static Texture2D	resizeHandleTexture;
	private static Texture2D	nodeEditorBackgroundTexture;
	private static Texture2D	defaultBackgroundTexture;

	private static Texture2D	preset2DSideViewTexture;
	private static Texture2D	preset2DTopDownViewTexture;
	private static Texture2D	preset3DPlaneTexture;
	private static Texture2D	preset3DSphericalTexture;
	private static Texture2D	preset3DCubicTexture;
	private static Texture2D	preset1DDensityFieldTexture;
	private static Texture2D	preset2DDensityFieldTexture;
	private static Texture2D	preset3DDensityFieldTexture;
	private static Texture2D	presetMeshTetxure;

	private static Texture2D	rencenterIconTexture;
	private static Texture2D	fileIconTexture;
	private static Texture2D	pauseIconTexture;
	private static Texture2D	eyeIconTexture;

	private static Gradient		greenRedGradient;
	
	static GUIStyle		whiteText;
	static GUIStyle		whiteBoldText;
	static GUIStyle		navBarBackgroundStyle;
	static GUIStyle		panelBackgroundStyle;
	static GUIStyle		nodeGraphWidowStyle;

	static GUISkin		PWGUISkin;

	public GUIStyle breadcrumbsButtonStyle;
	public GUIStyle	breadcrumbsButtonLeftStyle;
	public GUIStyle toolbarSearchCancelButtonStyle;
	public GUIStyle toolbarSearchTextStyle;
	public GUIStyle toolbarStyle;
	public GUIStyle nodeSelectorTitleStyle;
	public GUIStyle	nodeSelectorCaseStyle;
	public GUIStyle	selectionStyle;
	public GUIStyle	prefixLabelStyle;

	public GUIStyle	testNodeWinow;
	public GUIStyle blueNodeWindow;
	public GUIStyle blueNodeWindowSelected;
	public GUIStyle greenNodeWindow;
	public GUIStyle greenNodeWindowSelected;
	public GUIStyle yellowNodeWindow;
	public GUIStyle yellowNodeWindowSelected;
	public GUIStyle orangeNodeWindow;
	public GUIStyle orangeNodeWindowSelected;
	public GUIStyle redNodeWindow;
	public GUIStyle redNodeWindowSelected;
	public GUIStyle cyanNodeWindow;
	public GUIStyle cyanNodeWindowSelected;
	public GUIStyle purpleNodeWindow;
	public GUIStyle purpleNodeWindowSelected;
	public GUIStyle pinkNodeWindow;
	public GUIStyle pinkNodeWindowSelected;
	public GUIStyle greyNodeWindow;
	public GUIStyle greyNodeWindowSelected;
	public GUIStyle whiteNodeWindow;
	public GUIStyle whiteNodeWindowSelected;

#endregion

#region Initialization and data baking

	[MenuItem("Window/Procedural Worlds")]
	static void Init()
	{
		ProceduralWorldsWindow window = (ProceduralWorldsWindow)EditorWindow.GetWindow (typeof (ProceduralWorldsWindow));

		window.SaveNewGraph();

		window.Show();
	}

/*	void InitializeNewGraph(PWNodeGraph graph)
	{
		//setup splitted panels:
		graph.h1 = new HorizontalSplitView(resizeHandleTexture, position.width * 0.85f, position.width / 2, position.width - 4);
		graph.h2 = new HorizontalSplitView(resizeHandleTexture, position.width * 0.25f, 4, position.width / 2);

		graph.graphDecalPosition = Vector2.zero;

		graph.realMode = false;
		graph.presetChoosed = false;
		
		graph.localNodeIdCount = 0;
		graph.chunkSize = 16;
		graph.step = 1;
		graph.maxStep = 4;
		graph.geologicTerrainStep = 8;
		graph.geologicDistanceCheck = 2;
		
		graph.outputNode = CreateNewNode(typeof(PWNodeGraphOutput), new Vector2(position.width - 100, (int)(position.height / 2)));
		graph.inputNode = CreateNewNode(typeof(PWNodeGraphInput), new Vector2(50, (int)(position.height / 2)));

		graph.firstInitialization = "initialized";
		graph.PWGUI = new PWGUIManager();

		graph.saveName = null;
		graph.externalName = "New ProceduralWorld";

		graph.processMode = PWGraphProcessMode.Normal;
	}*/

	public override void OnEnable()
	{
		base.OnEnable();

		GeneratePWAssets();

		InitializeNodeSelector();

		//force graph to reload all chunks (just after compiled)
		graphNeedReload = true;
	}

#endregion

#region Global GUI rendering

	//call all rendering methods:
    public override void OnGUI()
    {
		var e = Event.current;
		LoadCustomStyles();

		//prevent popup events to influence the rest of the GUI
		PWPopup.eventType = e.type;
		PWGUIManager.editorWindowRect = position;
		if (PWPopup.mouseAbove && e.type != EventType.Repaint && e.type != EventType.Layout)
			e.type = EventType.Ignore;

		//update the current GUI settings storage and clear drawed popup list:
		currentGraph.PWGUI.StartFrame();
		if (e.type == EventType.Layout)
			PWPopup.ClearAll();

		//text colors:
		whiteText = new GUIStyle();
		whiteText.normal.textColor = Color.white;
		whiteBoldText = new GUIStyle();
		whiteBoldText.fontStyle = FontStyle.Bold;
		whiteBoldText.normal.textColor = Color.white;

		if (windowSize != Vector2.zero && windowSize != position.size)
			OnWindowResize();
		
		windowSize = position.size;
		
		if (!currentGraph.presetChoosed)
		{
			DrawPresetPanel();
			return ;
		}
		
		//esc key event:
		if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
		{
			if (draggingLink)
				StopDragLink(false);
			selecting = false;
			draggingSelectedNodes = false;
			draggingSelectedNodesFromContextMenu = false;
			draggingNode = false;
		}

		if (e.type == EventType.Layout)
			ProcessPreviewScene(currentGraph.outputType);

		if (terrainMaterializer == null)
		{
			GameObject gtm = GameObject.Find("PWPreviewTerrain");
			if (gtm != null)
				terrainMaterializer = gtm.GetComponent< PWTerrainBase >();
		}
	
		DrawNodeGraphCore();

		currentGraph.h1.UpdateMinMax(position.width / 2, position.width - 3);
		currentGraph.h2.UpdateMinMax(50, position.width / 2);

		currentGraph.h1.Begin();
		Rect p1 = currentGraph.h2.Begin(defaultBackgroundTexture);
		DrawLeftBar(p1);
		Rect g = currentGraph.h2.Split(resizeHandleTexture);
		DrawNodeGraphHeader(g);
		currentGraph.h2.End();
		Rect p2 = currentGraph.h1.Split(resizeHandleTexture);
		DrawSelector(p2);
		currentGraph.h1.End();

		DrawContextualMenu(g);

		//FIXME
		if (!editorNeedRepaint)
			editorNeedRepaint = e.isMouse || e.type == EventType.ScrollWheel;

		//if event, repaint
		if ((editorNeedRepaint || draggingGraph || draggingLink || draggingNode))
		{
			Repaint();
			editorNeedRepaint = false;
		}

		currentGraph.assetPath = AssetDatabase.GetAssetPath(currentGraph);
		currentMousePosition = e.mousePosition;

		//render all opened popups (at the end cause the have to be above other infos)
		PWPopup.RenderAll(ref editorNeedRepaint);
		
		if (GUI.changed && e.type == EventType.Layout)
		{
			EditorUtility.SetDirty(this);
			EditorUtility.SetDirty(currentGraph);
			AssetDatabase.SaveAssets();
			Debug.Log("saved all assets !");
		}
    }

#endregion

#region Node and OrderingGroup Utils

	void OnWindowResize()
	{
		//calcul the ratio for the window move:
		float r = position.size.x / windowSize.x;

		h1.handlerPosition *= r;
		h2.handlerPosition *= r;
	}

	void DeleteNode(object oNodeIndex)
	{
		int	id = (int)oNodeIndex;
		if (id < 0 || id > currentGraph.nodes.Count)
		{
			Debug.LogWarning("cant remove this node !");
			return ;
		}

		node.Remove();
		currentGraph.RemoveNode(id);
	}

	void CreateNewNode(object type)
	{
		Vector2 pos = -currentGraph.graphDecalPosition + currentMousePosition;
		CreateNewNode((Type)type, pos, true);
	}

	PWNode	CreateNewNode(PWNode newNode, Vector2 position, string name, bool addToNodeList = false)
	{
		position.x = Mathf.RoundToInt(position.x);
		position.y = Mathf.RoundToInt(position.y);

		//center to the middle of the screen:
		newNode.windowRect.position = position;
		newNode.SetNodeId(currentGraph.localNodeIdCount++);
		newNode.nodeTypeName = name;
		newNode.chunkSize = currentGraph.chunkSize;
		newNode.seed = currentGraph.seed;
		newNode.computeOrder = newNode.isDependent ? -1 : 0;
		GetWindowStyleFromType(newNode.GetType(), out newNode.windowStyle, out newNode.windowSelectedStyle);
		newNode.UpdateCurrentGraph(currentGraph);
		newNode.RunNodeAwake();

		if (String.IsNullOrEmpty(newNode.externalName))
		{
			newNode.externalName = newNode.GetType().Name;
			newNode.name = newNode.GetType().Name;
		}

		AssetDatabase.AddObjectToAsset(newNode, currentGraph);
		
		if (addToNodeList)
			currentGraph.nodes.Add(newNode);
		currentGraph.nodesDictionary[newNode.nodeId] = newNode;

		graphNeedReload = true;
		
		return newNode;
	}

	PWNode	CreateNewNode(Type t, Vector2 position, bool addToNodeList = false)
	{
		PWNode newNode = ScriptableObject.CreateInstance(t) as PWNode;

		return CreateNewNode(newNode, position, t.ToString(), addToNodeList);
	}

	void DeleteSelectedNodes()
	{
		List< PWNode > nodeToRemove = new List< PWNode >();
		List< PWNodeGraph > graphToRemove = new List< PWNodeGraph >();

		currentGraph.ForeachAllNodes(n => {
			if (n.selected)
			{
				if (n.GetType() == typeof(PWNodeGraphExternal))
				{
					//TODO: find graph and remove it
				}
				else
					nodeToRemove.Add(n);
			}
		});

		foreach (var n in nodeToRemove)
		{
			currentGraph.nodes.Remove(n);
			DeleteNode(n);
		}
	}

	void MoveSelectedNodes()
	{
		draggingSelectedNodesFromContextMenu = true;
		draggingSelectedNodes = true;
	}

	void CreateNewOrderingGroup(object pos)
	{
		currentGraph.orderingGroups.Add(new PWOrderingGroup((Vector2)pos));
	}

	void DeleteOrderingGroup()
	{
		if (mouseAboveOrderingGroup != null)
			currentGraph.orderingGroups.Remove(mouseAboveOrderingGroup);
	}

#endregion

#region Anchor and Links utils

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

#endregion

#region Contextual menu rendering

	void DrawContextualMenu(Rect graphNodeRect)
	{
		Event	e = Event.current;
        if (e.type == EventType.ContextClick)
        {
            Vector2 mousePos = e.mousePosition;
            EditorGUI.DrawRect(graphNodeRect, Color.green);

            if (graphNodeRect.Contains(mousePos))
            {
                // Now create the menu, add items and show it
                GenericMenu menu = new GenericMenu();
				foreach (var nodeCat in nodeSelectorList)
				{
					string menuString = "Create new/" + nodeCat.Key + "/";
					foreach (var nodeClass in nodeCat.Value.nodes)
						menu.AddItem(new GUIContent(menuString + nodeClass.name), false, CreateNewNode, nodeClass.nodeType);
				}
				menu.AddItem(new GUIContent("New Ordering group"), false, CreateNewOrderingGroup, e.mousePosition - currentGraph.graphDecalPosition);
				if (mouseAboveOrderingGroup != null)
					menu.AddItem(new GUIContent("Delete ordering group"), false, DeleteOrderingGroup);
				else
					menu.AddDisabledItem(new GUIContent("Delete ordering group"));

                menu.AddSeparator("");
				if (mouseAboveNodeAnchor)
				{
					menu.AddItem(new GUIContent("New Link"), false, BeginDragLink);
					menu.AddItem(new GUIContent("Delete all links"), false, DeleteAllAnchorLinks);
				}

				var hoveredLink = currentLinks.FirstOrDefault(l => l.hover == true);
				if (hoveredLink != null)
				{
					menu.AddItem(new GUIContent("Delete link"), false, DeleteLink, hoveredLink);
				}
				else
					menu.AddDisabledItem(new GUIContent("Link"));

                menu.AddSeparator("");
				if (mouseAboveNodeIndex != -1)
					menu.AddItem(new GUIContent("Delete node"), false, DeleteNode, mouseAboveNodeIndex);
				else
					menu.AddDisabledItem(new GUIContent("Delete node"));
					
				int selectedNodeCount = 0;
				currentGraph.ForeachAllNodes(n => { if (n.selected) selectedNodeCount++; });

				if (selectedNodeCount != 0)
				{
					string deleteNodeString = (selectedNodeCount == 1) ? "delete selected node" : "delete selected nodes";
					menu.AddItem(new GUIContent(deleteNodeString), false, DeleteSelectedNodes);

					string moveNodeString = (selectedNodeCount == 1) ? "move selected node" : "move selected nodes";
					menu.AddItem(new GUIContent(moveNodeString), false, MoveSelectedNodes);
				}

                menu.ShowAsContext();
                e.Use();
            }
        }
	}

#endregion

#region Utils and miscellaneous
	
	string GetUniqueName(string name)
	{
		while (true)
		{
			if (!currentGraph.nodes.Any(p => p.name == name))
				return name;
			name += "*";
		}
	}

	PWNode FindNodeById(int id)
	{
		return currentGraph.FindNodebyId(id);
	}

	void OnDestroy()
	{
		AssetDatabase.SaveAssets();
		currentGraph.isVisibleInEditor = false;
	}

	//If this function is called, it means there is not yet a saved instance of currentGraph
	void SaveNewGraph()
	{
		if (currentGraph.saveName != null)
			return ;
		
		if (!Directory.Exists(PWConstants.resourcePath))
			Directory.CreateDirectory(PWConstants.resourcePath);

		currentGraph.saveName = "New ProceduralWorld";
		currentGraph.name = currentGraph.saveName;
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(PWConstants.resourcePath, currentGraph.saveName + ".asset"));

		AssetDatabase.CreateAsset(currentGraph, assetPathAndName);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		EditorGUIUtility.PingObject(currentGraph);
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

	PWNodeGraph CloneGraph(PWNodeGraph graph)
	{
		PWNodeGraph	newGraph = Object.Instantiate< PWNodeGraph >(graph);

		newGraph.nodes.Clear();
		foreach (var node in graph.nodes)
		{
			var n = Object.Instantiate< PWNode >(node);
			n.UpdateCurrentGraph(newGraph);
			newGraph.nodes.Add(n);
		}
		newGraph.OnEnable();
		return newGraph;
	}

#endregion

#region Draw utils functions and Ressource generation

	static void GeneratePWAssets()
	{
		Func< Color, Texture2D > CreateTexture2DColor = (Color c) => {
			Texture2D	ret;
			ret = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			ret.wrapMode = TextureWrapMode.Repeat;
			ret.SetPixel(0, 0, c);
			ret.Apply();
			return ret;
		};

		Func< string, Texture2D > CreateTexture2DFromFile = (string ressourcePath) => {
			return Resources.Load< Texture2D >(ressourcePath);
        };

		//generate background colors:
        Color defaultBackgroundColor = new Color32(57, 57, 57, 255);
		Color resizeHandleColor = EditorGUIUtility.isProSkin
			? new Color32(56, 56, 56, 255)
            : new Color32(130, 130, 130, 255);
		
		//load backgrounds and colors as texture
		resizeHandleTexture = CreateTexture2DColor(resizeHandleColor);
		defaultBackgroundTexture = CreateTexture2DColor(defaultBackgroundColor);
		nodeEditorBackgroundTexture = CreateTexture2DFromFile("nodeEditorBackground");

		//loading preset panel images
		preset2DSideViewTexture = CreateTexture2DFromFile("preview2DSideView");
		preset2DTopDownViewTexture = CreateTexture2DFromFile("preview2DTopDownView");
		preset3DPlaneTexture = CreateTexture2DFromFile("preview3DPlane");
		preset3DSphericalTexture = CreateTexture2DFromFile("preview3DSpherical");
		preset3DCubicTexture = CreateTexture2DFromFile("preview3DCubic");
		presetMeshTetxure = CreateTexture2DFromFile("previewMesh");
		preset1DDensityFieldTexture= CreateTexture2DFromFile("preview1DDensityField");
		preset2DDensityFieldTexture = CreateTexture2DFromFile("preview2DDensityField");
		preset3DDensityFieldTexture = CreateTexture2DFromFile("preview3DDensityField");

		//icons and utils
		rencenterIconTexture = CreateTexture2DFromFile("ic_recenter");
		fileIconTexture = CreateTexture2DFromFile("ic_file");
		pauseIconTexture = CreateTexture2DFromFile("ic_pause");
		eyeIconTexture = CreateTexture2DFromFile("ic_eye");
		
		//style
		nodeGraphWidowStyle = new GUIStyle();
		nodeGraphWidowStyle.normal.background = defaultBackgroundTexture;

		//generating green-red gradient
        GradientColorKey[] gck;
        GradientAlphaKey[] gak;
        greenRedGradient = new Gradient();
        gck = new GradientColorKey[2];
        gck[0].color = Color.green;
        gck[0].time = 0.0F;
        gck[1].color = Color.red;
        gck[1].time = 1.0F;
        gak = new GradientAlphaKey[2];
        gak[0].alpha = 1.0F;
        gak[0].time = 0.0F;
        gak[1].alpha = 1.0F;
        gak[1].time = 1.0F;
        greenRedGradient.SetKeys(gck, gak);
	}

	void LoadCustomStyles()
	{
		PWGUISkin = Resources.Load("PWEditorSkin") as GUISkin;

		//initialize if null
		if (navBarBackgroundStyle == null || breadcrumbsButtonStyle == null || blueNodeWindow == null)
		{
			breadcrumbsButtonStyle = new GUIStyle("GUIEditor.BreadcrumbMid");
			breadcrumbsButtonLeftStyle = new GUIStyle("GUIEditor.BreadcrumbLeft");
	
			toolbarStyle = new GUIStyle("Toolbar");
			toolbarSearchTextStyle = new GUIStyle("ToolbarSeachTextField");
			toolbarSearchCancelButtonStyle = new GUIStyle("ToolbarSeachCancelButton");

			nodeSelectorTitleStyle = PWGUISkin.FindStyle("NodeSelectorTitle");
			nodeSelectorCaseStyle = PWGUISkin.FindStyle("NodeSelectorCase");

			selectionStyle = PWGUISkin.FindStyle("Selection");

			navBarBackgroundStyle = PWGUISkin.FindStyle("NavBarBackground");
			panelBackgroundStyle = PWGUISkin.FindStyle("PanelBackground");
	
			testNodeWinow = PWGUISkin.FindStyle("TestNodeWindow");

			prefixLabelStyle = PWGUISkin.FindStyle("PrefixLabel");
	
			blueNodeWindow = PWGUISkin.FindStyle("BlueNodeWindow");
			blueNodeWindowSelected = PWGUISkin.FindStyle("BlueNodeWindowSelected");
			greenNodeWindow = PWGUISkin.FindStyle("GreenNodeWindow");
			greenNodeWindowSelected = PWGUISkin.FindStyle("GreenNodeWindowSelected");
			yellowNodeWindow = PWGUISkin.FindStyle("YellowNodeWindow");
			yellowNodeWindowSelected = PWGUISkin.FindStyle("YellowNodeWindowSelected");
			orangeNodeWindow = PWGUISkin.FindStyle("OrangeNodeWindow");
			orangeNodeWindowSelected = PWGUISkin.FindStyle("OrangeNodeWindowSelected");
			redNodeWindow = PWGUISkin.FindStyle("RedNodeWindow");
			redNodeWindowSelected = PWGUISkin.FindStyle("RedNodeWindowSelected");
			cyanNodeWindow = PWGUISkin.FindStyle("CyanNodeWindow");
			cyanNodeWindowSelected = PWGUISkin.FindStyle("CyanNodeWindowSelected");
			purpleNodeWindow = PWGUISkin.FindStyle("PurpleNodeWindow");
			purpleNodeWindowSelected = PWGUISkin.FindStyle("PurpleNodeWindowSelected");
			pinkNodeWindow = PWGUISkin.FindStyle("PinkNodeWindow");
			pinkNodeWindowSelected = PWGUISkin.FindStyle("PinkNodeWindowSelected");
			greyNodeWindow = PWGUISkin.FindStyle("GreyNodeWindow");
			greyNodeWindowSelected = PWGUISkin.FindStyle("GreyNodeWindowSelected");
			whiteNodeWindow = PWGUISkin.FindStyle("WhiteNodeWindow");
			whiteNodeWindowSelected = PWGUISkin.FindStyle("WhiteNodeWindowSelected");
			
			//copy all custom styles to the new style
			string[] stylesToCopy = {"RL"};
			PWGUISkin.customStyles = PWGUISkin.customStyles.Concat(
				GUI.skin.customStyles.Where(
					style => stylesToCopy.Any(
						styleName => style.name.Contains(styleName) && !PWGUISkin.customStyles.Any(
							s => s.name.Contains(styleName)
						)
					)
				)
			).ToArray();
		}
			
		//set the custom style for the editor
		GUI.skin = PWGUISkin;
	}

    void DrawNodeCurve(Rect start, Rect end, int index, PWLink link)
    {
		Event e = Event.current;

		int		id;
		if (link == null)
			id = -1;
		else
			id = GUIUtility.GetControlID((link.localName + link.distantName + index).GetHashCode(), FocusType.Passive);

        Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
		
		Vector3 startDir = Vector3.right;;
		Vector3 endDir = Vector3.left;;

		float	tanPower = 50;

        Vector3 startTan = startPos + startDir * tanPower;
        Vector3 endTan = endPos + endDir * tanPower;

		if (link != null && !draggingNode && String.IsNullOrEmpty(GUI.GetNameOfFocusedControl()))
		{
			switch (e.GetTypeForControl(id))
			{
				case EventType.MouseDown:
					if (link.linkHighlight == PWLinkHighlight.Delete)
						break ;
					if (!draggingLink && HandleUtility.nearestControl == id && (e.button == 0 || e.button == 1))
					{
						GUIUtility.hotControl = id;
						//unselect all others links:
						foreach (var l in currentLinks)
							l.selected = false;
						link.selected = true;
						link.linkHighlight = PWLinkHighlight.Selected;
					}
					break ;
			}
			if (HandleUtility.nearestControl == id)
			{
				Debug.Log("bezier curve take the control !");
				GUIUtility.hotControl = id;
				link.hover = true;
			}
		}

		HandleUtility.AddControl(id, HandleUtility.DistancePointBezier(e.mousePosition, startPos, endPos, startTan, endTan) / 1.5f);
		if (e.type == EventType.Repaint)
		{
			PWLinkHighlight s = (link != null) ? (link.linkHighlight) : PWLinkHighlight.None;
			PWNodeProcessMode m = (link != null) ? link.mode : PWNodeProcessMode.AutoProcess;
			switch ((link != null) ? link.linkType : PWLinkType.BasicData)
			{
				case PWLinkType.Sampler3D:
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(.1f, .1f, .1f), 8, s, m);
					break ;
				case PWLinkType.ThreeChannel:
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(0f, 0f, 1f), 12, s, m);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(0f, 1f, 0f), 8, s, m);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(1f, 0f, 0f), 4, s, m);
					break ;
				case PWLinkType.FourChannel:
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(.1f, .1f, .1f), 16, s, m);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(0f, 0f, 1f), 12, s, m);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(0f, 1f, 0f), 8, s, m);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(1f, 0f, 0f), 4, s, m);
					break ;
				default:
					DrawSelectedBezier(startPos, endPos, startTan, endTan, (link == null) ? startDragAnchor.anchorColor : link.color, 4, s, m);
					break ;
			}
			if (link != null && link.linkHighlight == PWLinkHighlight.DeleteAndReset)
				link.linkHighlight = PWLinkHighlight.None;
			if (link != null && !link.selected && link.linkHighlight == PWLinkHighlight.Selected)
				link.linkHighlight = PWLinkHighlight.None;
		}
    }

	void	DrawSelectedBezier(Vector3 startPos, Vector3 endPos, Vector3 startTan, Vector3 endTan, Color c, int width, PWLinkHighlight linkHighlight, PWNodeProcessMode linkMode)
	{
		switch (linkHighlight)
		{
			case PWLinkHighlight.Selected:
				Handles.DrawBezier(startPos, endPos, startTan, endTan, PWColorPalette.GetColor("selectedNode"), null, width + 3);
				break;
			case PWLinkHighlight.Delete:
			case PWLinkHighlight.DeleteAndReset:
				Handles.DrawBezier(startPos, endPos, startTan, endTan, new Color(1f, .0f, .0f, 1), null, width + 2);
				break ;
		}
		Handles.DrawBezier(startPos, endPos, startTan, endTan, c, null, width);

		if (linkMode == PWNodeProcessMode.RequestForProcess)
		{
			Vector3[] points = Handles.MakeBezierPoints(startPos, endPos, startTan, endTan, 4);
			Vector2 pauseSize = new Vector2(20, 20);
			Matrix4x4 savedGUIMatrix = GUI.matrix;
			Rect pauseRect = new Rect((Vector2)points[2] - pauseSize / 2, pauseSize);
			float angle = Vector2.Angle((startPos.y > endPos.y) ? startPos - endPos : endPos - startPos, Vector2.right);
			GUIUtility.RotateAroundPivot(angle, points[2]);
			GUI.color = c;
            GUI.DrawTexture(pauseRect, pauseIconTexture);
			GUI.color = Color.white;
			GUI.matrix = savedGUIMatrix;
        }
	}
#endregion
}