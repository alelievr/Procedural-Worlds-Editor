// #define		DEBUG_GRAPH

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;

public class ProceduralWorldsWindow : EditorWindow {

    private static Texture2D	backgroundTex;
	private static Texture2D	resizeHandleTex;
	private static Texture2D	selectorBackgroundTex;
	// private static Texture2D	debugTexture1;
	private static Texture2D	selectorCaseBackgroundTex;
	private static Texture2D	selectorCaseTitleBackgroundTex;

	private static Texture2D	preset2DSideViewTexture;
	private static Texture2D	preset2DTopDownViewTexture;
	private static Texture2D	preset3DPlaneTexture;
	private static Texture2D	preset3DSphericalTexture;
	private static Texture2D	preset3DCubicTexture;
	private static Texture2D	preset1DDensityFieldTexture;
	private static Texture2D	preset2DDensityFieldTexture;
	private static Texture2D	preset3DDensityFieldTexture;
	private static Texture2D	presetMeshTetxure;
	
	static GUIStyle	whiteText;
	static GUIStyle	whiteBoldText;
	static GUIStyle	splittedPanel;
	static GUIStyle	nodeGraphWidowStyle;

	int					currentPickerWindow;
	int					mouseAboveNodeIndex;
	int					mouseAboveSubmachineIndex;
	bool				mouseAboveNodeAnchor;
	bool				draggingGraph = false;
	bool				draggingLink = false;
	bool				draggingNode = false;
	public static bool	graphNeedReload = false;
	bool				previewMouseDrag = false;
	PWAnchorInfo		startDragAnchor;
	PWAnchorInfo		mouseAboveAnchorInfo;

	Vector2				lastMousePosition;
	Vector2				presetScrollPos;
	Vector2				windowSize;

	GameObject			previewScene;
	Camera				previewCamera;
	RenderTexture		previewCameraRenderTexture;

	int					linkIndex;
	List< PWLink >		currentLinks = new List< PWLink >();

	PWTerrainBase		terrainMaterializer;

	[SerializeField]
	public PWNodeGraph	currentGraph;
	[SerializeField]
	public PWNodeGraph	parentGraph;

	[System.NonSerializedAttribute]
	Vector2				currentMousePosition;

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
	Dictionary< string, List< PWNodeStorage > > nodeSelectorList = new Dictionary< string, List< PWNodeStorage > >();

	[MenuItem("Window/Procedural Worlds")]
	static void Init()
	{
		ProceduralWorldsWindow window = (ProceduralWorldsWindow)EditorWindow.GetWindow (typeof (ProceduralWorldsWindow));

		window.SaveNewGraph();

		window.Show();
	}

	void InitializeNewGraph(PWNodeGraph graph)
	{
		//setup splitted panels:
		graph.h1 = new HorizontalSplitView(resizeHandleTex, position.width * 0.85f, position.width / 2, position.width - 4);
		graph.h2 = new HorizontalSplitView(resizeHandleTex, position.width * .25f, 0, position.width / 2);

		graph.graphDecalPosition = Vector2.zero;

		graph.realMode = false;
		graph.presetChoosed = false;
		
		graph.localWindowIdCount = 0;
		graph.chunkSize = 16;
		
		graph.parentReference = null;
		graph.outputNode = CreateNewNode(typeof(PWNodeGraphOutput), new Vector2(position.width - 100, (int)(position.height / 2)));
		graph.inputNode = CreateNewNode(typeof(PWNodeGraphInput), new Vector2(50, (int)(position.height / 2)));

		graph.firstInitialization = "initialized";

		graph.saveName = null;
		graph.externalName = "New ProceduralWorld";
	}

	void AddToSelector(string key, params object[] objs)
	{
		if (!nodeSelectorList.ContainsKey(key))
			nodeSelectorList[key] = new List< PWNodeStorage >();
		for (int i = 0; i < objs.Length; i += 2)
			nodeSelectorList[key].Add(new PWNodeStorage((string)objs[i], (Type)objs[i + 1]));
	}

	void OnEnable()
	{
		CreateBackgroundTexture();
		
		splittedPanel = new GUIStyle();
		splittedPanel.margin = new RectOffset(5, 0, 0, 0);

		nodeGraphWidowStyle = new GUIStyle();
		nodeGraphWidowStyle.normal.background = backgroundTex;

		//setup nodeList:
		foreach (var n in nodeSelectorList)
			n.Value.Clear();
		AddToSelector("Simple values", "Slider", typeof(PWNodeSlider));
		AddToSelector("Operations", "Add", typeof(PWNodeAdd));
		AddToSelector("Debug", "DebugLog", typeof(PWNodeDebugLog));
		AddToSelector("Noise masks", "Circle Noise Mask", typeof(PWNodeCircleNoiseMask));
		AddToSelector("Noises", "Perlin noise 2D", typeof(PWNodePerlinNoise2D));
		AddToSelector("Materializers", "SideView 2D terrain", typeof(PWNodeSideView2DTerrain));
		AddToSelector("Materializers", "TopDown 2D terrain", typeof(PWNodeTopDown2DTerrain));
		AddToSelector("Storages");
		AddToSelector("Custom");

		//current PWNodeGraph file have been deleted
		if (currentGraph == null)
			currentGraph = ScriptableObject.CreateInstance< PWNodeGraph >();

		//force graph to reload all chunks (just after compiled)
		graphNeedReload = true;

		currentGraph.unserializeInitialized = true;
	}

	void OnDestroy()
	{
		AssetDatabase.SaveAssets();
	}

	//If this function is called, it means there is not yet a saved instance of currentGraph
	void SaveNewGraph()
	{
		if (currentGraph.saveName != null)
			return ;

		string path = "Assets/ProceduralWorlds/Resources/";
		
		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);

		currentGraph.saveName = "New ProceduralWorld";
		currentGraph.name = currentGraph.saveName;
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + currentGraph.saveName + ".asset");

		AssetDatabase.CreateAsset(currentGraph, assetPathAndName);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		EditorGUIUtility.PingObject(currentGraph);
	}

    void OnGUI()
    {
		//text colors:
		whiteText = new GUIStyle();
		whiteText.normal.textColor = Color.white;
		whiteBoldText = new GUIStyle();
		whiteBoldText.fontStyle = FontStyle.Bold;
		whiteBoldText.normal.textColor = Color.white;

        //background color:
		if (backgroundTex == null || !currentGraph.unserializeInitialized || resizeHandleTex == null)
			OnEnable();
			
		if (currentGraph.parentReference == null || String.IsNullOrEmpty(currentGraph.parentReference))
			currentGraph.parentReference = null;
			
		//function is in OnGUI cause in OnEnable, the position values are bad.
		if (currentGraph.firstInitialization == null)
			InitializeNewGraph(currentGraph);
			
		if (parentGraph == null)
		{
			parentGraph = AssetDatabase.LoadAssetAtPath< PWNodeGraph >(AssetDatabase.GetAssetPath(currentGraph));
			Debug.Log("loaded parent graph: " + parentGraph.name);
		}
		
		if (!currentGraph.presetChoosed)
		{
			DrawPresetPanel();
			return ;
		}
		
		if (windowSize != Vector2.zero && windowSize != position.size)
			OnWindowResize();

		// initialize unserialized fields in node:
		currentGraph.ForeachAllNodes((n) => { if (n != null && !n.unserializeInitialized) n.RunNodeAwake(); }, true, true);

		//esc key event:
		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
		{
			if (draggingLink)
				StopDragLink(false);
		}

		GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), backgroundTex, ScaleMode.StretchToFill);

		if (Event.current.type == EventType.Layout)
			ProcessPreviewScene(currentGraph.outputType);

		if (terrainMaterializer == null)
		{
			GameObject gtm = GameObject.Find("PWPreviewTerrain");
			if (gtm != null)
				terrainMaterializer = gtm.GetComponent< PWTerrainBase >();
		}

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

		DrawContextualMenu(g);

		//if event, repaint
		if (Event.current.type == EventType.mouseDown
			|| Event.current.type == EventType.mouseDrag
			|| Event.current.type == EventType.mouseUp
			|| Event.current.type == EventType.scrollWheel
			|| Event.current.type == EventType.KeyDown
			|| Event.current.type == EventType.Repaint
			|| Event.current.type == EventType.KeyUp)
			Repaint();

		windowSize = position.size;

		currentGraph.assetPath = AssetDatabase.GetAssetPath(currentGraph);
		currentMousePosition = Event.current.mousePosition;
		
		if (GUI.changed && Event.current.type == EventType.Layout)
		{
			EditorUtility.SetDirty(this);
			EditorUtility.SetDirty(currentGraph);
			AssetDatabase.SaveAssets();
			Debug.Log("saved all assets !");
		}
    }

	void DrawPresetLineHeader(string header)
	{
		EditorGUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		EditorGUI.indentLevel = 5;
		EditorGUILayout.LabelField(header, whiteText);
		EditorGUI.indentLevel = 0;
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndVertical();
	}

	void DrawPresetLine(Texture2D tex, string description, Action callback, bool disabled = true)
	{
		EditorGUILayout.BeginVertical();
		{
			GUILayout.FlexibleSpace();
			EditorGUI.BeginDisabledGroup(disabled);
			if (tex != null)
				if (GUILayout.Button(tex, GUILayout.Width(100), GUILayout.Height(100)))
				{
					currentGraph.presetChoosed = true;
					graphNeedReload = true;
					callback();
					EvaluateComputeOrder();
				}
			EditorGUILayout.LabelField(description, whiteText);
			EditorGUI.EndDisabledGroup();
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndVertical();
	}

	void DrawPresetPanel()
	{
		GUI.DrawTexture(new Rect(0, 0, position.width, position.height), backgroundTex);

		presetScrollPos = EditorGUILayout.BeginScrollView(presetScrollPos);

		EditorGUILayout.LabelField("Procedural Worlds");
		
		//Load graph button:
		EditorGUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("Load graph"))
			{
				currentPickerWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
				EditorGUIUtility.ShowObjectPicker< PWNodeGraph >(null, false, "", currentPickerWindow);
			}
			
			if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow)
			{
				UnityEngine.Object selected = null;
				selected = EditorGUIUtility.GetObjectPickerObject();
				if (selected != null)
				{
					Debug.Log("graph " + selected.name + " loaded");
					currentGraph = (PWNodeGraph)selected;
				}
			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.FlexibleSpace();
			EditorGUILayout.BeginVertical();
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical();
			{
				GUILayout.FlexibleSpace();

				//3 DrawPresetLine per line + 1 header:
				EditorGUILayout.BeginHorizontal();
				DrawPresetLineHeader("2D");
				DrawPresetLine(preset2DSideViewTexture, "2D sideview procedural terrain", () => {});
				DrawPresetLine(preset2DTopDownViewTexture, "2D top down procedural terrain", () => {
					currentGraph.outputType = PWOutputType.TOPDOWNVIEW_2D;
					CreateNewNode(typeof(PWNodePerlinNoise2D));
					PWNode perlin = currentGraph.nodes.Last();
					perlin.windowRect.position += Vector2.left * 400;
					CreateNewNode(typeof(PWNodeTopDown2DTerrain));
					PWNode terrain = currentGraph.nodes.Last();

					perlin.AttachLink("output", terrain, "texture");
					terrain.AttachLink("texture", perlin, "output");
					terrain.AttachLink("terrainOutput", currentGraph.outputNode, "inputValues");
					currentGraph.outputNode.AttachLink("inputValues", terrain, "terrainOutput");
				}, false);
				DrawPresetLine(null, "", () => {});
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				DrawPresetLineHeader("3D");
				DrawPresetLine(preset3DPlaneTexture, "3D plane procedural terrain", () => {});
				DrawPresetLine(preset3DSphericalTexture, "3D spherical procedural terrain", () => {});
				DrawPresetLine(preset3DCubicTexture, "3D cubic procedural terrain", () => {});
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				DrawPresetLineHeader("Density fields");
				DrawPresetLine(preset1DDensityFieldTexture, "1D float density field", () => {});
				DrawPresetLine(preset2DDensityFieldTexture, "2D float density field", () => {});
				DrawPresetLine(preset3DDensityFieldTexture, "3D float density field", () => {});
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				DrawPresetLineHeader("Others");
				DrawPresetLine(presetMeshTetxure, "mesh", () => {});
				DrawPresetLine(null, "", () => {});
				DrawPresetLine(null, "", () => {});
				EditorGUILayout.EndHorizontal();
				
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical();
			EditorGUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndScrollView();
	}

	GameObject GetLoadedPreviewScene(params PWOutputType[] allowedTypes)
	{
		GameObject		ret;

		Func< string, PWOutputType, GameObject >	TestSceneNametype = (string name, PWOutputType type) =>
		{
			ret = GameObject.Find(name);
			if (ret == null)
				return null;
			foreach (var at in allowedTypes)
				if (type == at)
					return ret;
			return null;
		};
		ret = TestSceneNametype(PWConstants.previewSideViewSceneName, PWOutputType.SIDEVIEW_2D);
		if (ret != null)
			return ret;
		ret = TestSceneNametype(PWConstants.previewTopDownSceneName, PWOutputType.TOPDOWNVIEW_2D);
		if (ret != null)
			return ret;
		ret = TestSceneNametype(PWConstants.preview3DSceneName, PWOutputType.PLANE_3D);
		if (ret != null)
			return ret;
		return null;
	}

	void ProcessPreviewScene(PWOutputType outputType)
	{
		if (previewScene == null)
		{
			//TODO: do the preview for Density field 1D
			switch (outputType)
			{
				case PWOutputType.DENSITY_2D:
				case PWOutputType.SIDEVIEW_2D:
					previewScene = GetLoadedPreviewScene(PWOutputType.DENSITY_2D, PWOutputType.SIDEVIEW_2D);
					if (previewScene == null)
						previewScene = Instantiate(Resources.Load(PWConstants.previewSideViewSceneName, typeof(GameObject)) as GameObject);
					previewScene.name = PWConstants.previewTopDownSceneName;
					break ;
				case PWOutputType.TOPDOWNVIEW_2D:
					previewScene = GetLoadedPreviewScene(PWOutputType.TOPDOWNVIEW_2D);
					if (previewScene == null)
						previewScene = Instantiate(Resources.Load(PWConstants.previewTopDownSceneName, typeof(GameObject)) as GameObject);
					previewScene.name = PWConstants.previewTopDownSceneName;
					break ;
				default: //for 3d previewScenes:
					previewScene = GetLoadedPreviewScene(PWOutputType.CUBIC_3D, PWOutputType.DENSITY_3D, PWOutputType.PLANE_3D, PWOutputType.SPHERICAL_3D);
					if (previewScene == null)
						previewScene = Instantiate(Resources.Load(PWConstants.preview3DSceneName, typeof(GameObject)) as GameObject);
					previewScene.name = PWConstants.preview3DSceneName;
					break ;
			}
		}

		if (previewCamera == null)
			previewCamera = previewScene.GetComponentInChildren< Camera >();
		if (previewCameraRenderTexture == null)
			previewCameraRenderTexture = new RenderTexture(800, 800, 10000, RenderTextureFormat.ARGB32);
		if (previewCamera != null && previewCameraRenderTexture != null)
			previewCamera.targetTexture = previewCameraRenderTexture;
		if (terrainMaterializer == null)
			terrainMaterializer = previewScene.GetComponentInChildren< PWTerrainBase >();
		if (terrainMaterializer.initialized == false || terrainMaterializer.graph != currentGraph)
			terrainMaterializer.InitGraph(parentGraph);
	}

	void MovePreviewCamera(Vector2 move)
	{
		previewCamera.gameObject.transform.position += new Vector3(move.x, 0, move.y);
	}

	void DrawLeftBar(Rect currentRect)
	{
		Event	e = Event.current;
		GUI.DrawTexture(currentRect, backgroundTex);

		//add the texturepreviewRect size:
		Rect previewRect = new Rect(0, 0, currentRect.width, currentRect.width);
		currentGraph.leftBarScrollPosition = EditorGUILayout.BeginScrollView(currentGraph.leftBarScrollPosition, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(currentRect.width), GUILayout.ExpandHeight(true));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Height(currentRect.height - currentRect.width - 4), GUILayout.ExpandWidth(true));
			{
				EditorGUILayout.LabelField("Procedural Worlds Editor !", whiteText);

				if (currentGraph == null)
					OnEnable();
				GUI.SetNextControlName("PWName");
				currentGraph.externalName = EditorGUILayout.TextField("ProceduralWorld name: ", currentGraph.name);

				if ((e.type == EventType.MouseDown || e.type == EventType.Ignore)
					&& !GUILayoutUtility.GetLastRect().Contains(e.mousePosition)
					&& GUI.GetNameOfFocusedControl() == "PWName")
					GUI.FocusControl(null);

				//preview texture:
				GUI.DrawTexture(previewRect, previewCameraRenderTexture);

				//preview controls:
				if (e.type == EventType.MouseDown && previewRect.Contains(e.mousePosition))
					previewMouseDrag = true;

				if (e.type == EventType.Layout && previewMouseDrag)
				{
					//mouse controls:
					Vector2 move = e.mousePosition - lastMousePosition;

					MovePreviewCamera(new Vector2(-move.x / 16, move.y / 16));
				}

				EditorGUI.BeginChangeCheck();
				parentGraph.seed = EditorGUILayout.IntField("Seed", parentGraph.seed);
				if (EditorGUI.EndChangeCheck())
				{
					parentGraph.UpdateSeed(parentGraph.seed);
					graphNeedReload = true;
				}
				
				//chunk size:
				EditorGUI.BeginChangeCheck();
				parentGraph.chunkSize = EditorGUILayout.IntField("Chunk size", parentGraph.chunkSize);
				parentGraph.chunkSize = Mathf.Clamp(parentGraph.chunkSize, 1, 1024);
				if (EditorGUI.EndChangeCheck())
				{
					parentGraph.UpdateChunkSize(parentGraph.chunkSize);
					graphNeedReload = true;
				}
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
							CreateNewNode(nodeCase.nodeType);
					}
				}
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndScrollView();
	}
	
	void DrawNodeGraphHeader(Rect graphRect)
	{
		Event	e = Event.current;
		EditorGUILayout.BeginVertical(splittedPanel);
		{
			//TODO: render the breadcrumbs bar
			Rect breadcrumbsRect = EditorGUILayout.BeginHorizontal(GUILayout.MaxHeight(20), GUILayout.ExpandWidth(true));
			{
				breadcrumbsRect.yMin -= 1;
				breadcrumbsRect.xMin -= 1;
				if (e.type == EventType.Repaint)
					GUI.DrawTexture(breadcrumbsRect, selectorBackgroundTex);
				
				PWNodeGraph g = currentGraph;
				var breadcrumbsList = new List< string >();
				while (g != null)
				{
					breadcrumbsList.Add(g.name);
					g = parentGraph.FindGraphByName(g.parentReference);
				}
				breadcrumbsList.Reverse();
				foreach (var b in breadcrumbsList)
					if (GUILayout.Button(b, GUILayout.MaxWidth(150)))
						SwitchGraph(parentGraph.FindGraphByName(b));
			}
			EditorGUILayout.EndHorizontal();
	
			//remove 4 pixels for the separation bar
			graphRect.size -= Vector2.right * 4;
	
			#if (DEBUG_GRAPH)
			foreach (var node in nodes)
				GUI.DrawTexture(PWUtils.DecalRect(node.rect, graphDecalPosition), debugTexture1);
			#endif
	
			if (e.type == EventType.MouseDown //if event is mouse down
				&& (e.button == 0 || e.button == 2)
				&& !mouseAboveNodeAnchor //if mouse is not above a node anchor
				&& graphRect.Contains(e.mousePosition) //and mouse position is in graph
				&& mouseAboveNodeIndex == -1
				&& mouseAboveSubmachineIndex == -1)
				draggingGraph = true;
			if (e.type == EventType.MouseUp)
			{
				draggingGraph = false;
				previewMouseDrag = false;
			}
			if (e.type == EventType.Layout)
			{
				if (draggingGraph)
					currentGraph.graphDecalPosition += e.mousePosition - lastMousePosition;
				lastMousePosition = e.mousePosition;
			}
		}
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

	void DisplayDecaledNode(int id, PWNode node, string name)
	{
		node.UpdateGraphDecal(currentGraph.graphDecalPosition);
		node.windowRect = PWUtils.DecalRect(node.windowRect, currentGraph.graphDecalPosition);
		Rect decaledRect = GUILayout.Window(id, node.windowRect, node.OnWindowGUI, name, GUILayout.Height(node.viewHeight));
		node.windowRect = PWUtils.DecalRect(decaledRect, -currentGraph.graphDecalPosition);
	}

	PWNode FindNodeByWindowId(int id)
	{
		if (currentGraph.nodesDictionary.ContainsKey(id))
			return currentGraph.nodesDictionary[id];

		var ret = currentGraph.nodes.FirstOrDefault(n => n.windowId == id);
		if (ret != null)
			return ret;

		string gExternalNodeName = currentGraph.subgraphReferences.FirstOrDefault(gName => {
			var g = parentGraph.FindGraphByName(gName);
			return g && g.externalGraphNode.windowId == id;
		});
		if (gExternalNodeName != null)
		{
			var gExternalNode = parentGraph.FindGraphByName(gExternalNodeName);
			if(gExternalNode.externalGraphNode != null)
				return gExternalNode.externalGraphNode;
		}

		if (currentGraph.inputNode.windowId == id)
			return currentGraph.inputNode;
		if (currentGraph.outputNode.windowId == id)
			return currentGraph.outputNode;

		return null;
	}

	void RenderNode(int id, PWNode node, string name, int index, ref bool mouseAboveAnchorLocal, ref bool mouseDraggingWindowLocal, bool submachine = false)
	{
		Event	e = Event.current;

		GUI.depth = node.computeOrder;
		DisplayDecaledNode(id, node, name);

		if (node.windowRect.Contains(e.mousePosition - currentGraph.graphDecalPosition))
		{
			if (submachine)
				mouseAboveSubmachineIndex = index;
			else
				mouseAboveNodeIndex = index;
		}

		//highlight, hide, add all linkable anchors:
		if (draggingLink)
			node.HighlightLinkableAnchorsTo(startDragAnchor);
		node.DisplayHiddenMultipleAnchors(draggingLink);

		//process envent, state and position for node anchors:
		var mouseAboveAnchor = node.ProcessAnchors();
		if (mouseAboveAnchor.mouseAbove)
			mouseAboveAnchorLocal = true;

		if (!mouseDraggingWindowLocal)
			if (node.isDragged)
				mouseDraggingWindowLocal = true;

		//render node anchors:
		node.RenderAnchors();

		//end dragging:
		if ((e.type == EventType.mouseUp && draggingLink == true) //standard drag start
				|| (e.type == EventType.MouseDown && draggingLink == true)) //drag started with context menu
			if (mouseAboveAnchor.mouseAbove)
			{
				StopDragLink(true);

				//attach link to the node:
				node.AttachLink(mouseAboveAnchor, startDragAnchor);
				var win = FindNodeByWindowId(startDragAnchor.windowId);
				if (win != null)
				{
					win.AttachLink(startDragAnchor, mouseAboveAnchor);
					graphNeedReload = true;
				}
				else
					Debug.LogWarning("window id not found: " + startDragAnchor.windowId);
				
				//Recalcul the compute order:
				EvaluateComputeOrder();
			}

		if (mouseAboveAnchor.mouseAbove)
			mouseAboveAnchorInfo = mouseAboveAnchor;
			
		//if you press the mouse above an anchor, start the link drag
		if (mouseAboveAnchor.mouseAbove && e.type == EventType.MouseDown && e.button == 0)
			BeginDragLink();
		
		if (mouseAboveAnchor.mouseAbove
				&& draggingLink
				&& startDragAnchor.anchorId != mouseAboveAnchorInfo.anchorId
				&& mouseAboveAnchor.anchorType == PWAnchorType.Input)
			HighlightDeleteAnchor(mouseAboveAnchor);

		//draw links:
		var links = node.GetLinks();
		int		i = 0;
		foreach (var link in links)
		{
			// Debug.Log("link: " + link.localWindowId + ":" + link.localAnchorId + " to " + link.distantWindowId + ":" + link.distantAnchorId);
			var fromWindow = FindNodeByWindowId(link.localWindowId);
			var toWindow = FindNodeByWindowId(link.distantWindowId);

			if (toWindow == null) //invalid window ids
			{
				node.DeleteLinkByWindowTarget(link.distantWindowId);
				Debug.LogWarning("window not found: " + link.distantWindowId);
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

		//check if user have pressed the close button of this window:
		if (node.WindowShouldClose())
			DeleteNode(index);
	}

	void DrawNodeGraphCore()
	{
		Event	e = Event.current;
		int		i;

		EditorGUILayout.BeginHorizontal();
		{
			currentGraph.ForeachAllNodes(p => p.BeginFrameUpdate());
			//We run the calcul the nodes:
			//if we are on the mother graph, render the terrain
			//TODO: other preview type for graph outputs in function of graph output type.
			if (e.type == EventType.Layout)
			{
				if (graphNeedReload)
				{
					terrainMaterializer.DestroyAllChunks();
					graphNeedReload = false;
				}
				//updateChunks will update and generate new chunks if needed.
				terrainMaterializer.UpdateChunks();
			}
			if (e.type == EventType.KeyDown && e.keyCode == KeyCode.S)
				graphNeedReload = true;

			bool	mouseAboveAnchorLocal = false;
			bool	draggingNodeLocal = false;
			int		windowId = 0;
			linkIndex = 0;

			mouseAboveNodeIndex = -1;
			mouseAboveSubmachineIndex = -1;

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
				string nodeName = (string.IsNullOrEmpty(node.name)) ? node.nodeTypeName : node.name;
				RenderNode(windowId++, node, nodeName, i, ref mouseAboveAnchorLocal, ref draggingNodeLocal);
			}

			//display graph sub-PWGraphs
			i = 0;
			foreach (var graphName in currentGraph.subgraphReferences)
			{
				var graph = parentGraph.FindGraphByName(graphName);
				if (graph)
					RenderNode(windowId++, graph.externalGraphNode, graph.externalName, i, ref mouseAboveAnchorLocal, ref draggingNodeLocal, true);
				i++;
			}

			//display the upper graph reference:
			if (currentGraph.parentReference != null)
				RenderNode(windowId++, currentGraph.inputNode, "upper graph", -2, ref mouseAboveAnchorLocal, ref draggingNodeLocal);
			RenderNode(windowId++, currentGraph.outputNode, "output", -2, ref mouseAboveAnchorLocal, ref draggingNodeLocal);

			EndWindows();
			
			//submachine enter button click management:
			foreach (var graphName in currentGraph.subgraphReferences)
			{
				var graph = parentGraph.FindGraphByName(graphName);

				if (!graph || !graph.externalGraphNode)
					continue ;

				if (graph.externalGraphNode.specialButtonClick)
					SwitchGraph(graph);
			}

			//click up outside of an anchor, stop dragging
			if (e.type == EventType.mouseUp && draggingLink)
				StopDragLink(false);

			if (draggingLink)
				DrawNodeCurve(
					new Rect((int)startDragAnchor.anchorRect.center.x, (int)startDragAnchor.anchorRect.center.y, 0, 0),
					new Rect((int)e.mousePosition.x, (int)e.mousePosition.y, 0, 0),
					-1,
					null
				);
			mouseAboveNodeAnchor = mouseAboveAnchorLocal;
			draggingNode = draggingNodeLocal;
			
			if (e.type == EventType.MouseDown && !currentLinks.Any(l => l.hover) && draggingGraph == false)
				foreach (var l in currentLinks)
					if (l.selected)
					{
						l.selected = false;
						l.linkHighlight = PWLinkHighlight.None;
					}

			bool	reloadRequested = false;
			int		reloadWeight = 0;
			currentGraph.ForeachAllNodes(p => {
				p.EndFrameUpdate();
				if (e.type == EventType.Layout)
					if (p.notifyDataChanged)
					{
						graphNeedReload = true;
						p.notifyDataChanged = false;
						reloadRequested = true;
						reloadWeight = p.computeOrder;
					}
			}, true, true);

			//TODO: subgraph dependencies management.
			if (reloadRequested)
			{
				currentGraph.ForeachAllNodes(n => {
					if (n.computeOrder >= reloadWeight)
						n.reloadRequested = true;
				}, true, true);
			}
		}
		EditorGUILayout.EndHorizontal();
	}

	void OnWindowResize()
	{

	}

	void DeleteNode(object oNodeIndex)
	{
		var node = currentGraph.nodes[(int)oNodeIndex];

		if (node == null)
			return ;

		graphNeedReload = true;
		//remove all input links for each node links:
		foreach (var link in node.GetLinks())
		{
			var n = FindNodeByWindowId(link.distantWindowId);
			if (n != null)
				n.DeleteDependenciesByWindowTarget(link.localWindowId);
		}
		//remove all links for node dependencies
		foreach (var deps in node.GetDependencies())
		{
			var n = FindNodeByWindowId(deps.windowId);
			if (n != null)
				n.DeleteLinkByWindowTarget(node.windowId);
		}

		//remove the node
		currentGraph.nodes.RemoveAt((int)oNodeIndex);
		DestroyImmediate(node, true);

		EvaluateComputeOrder();
	}

	void CreateNewNode(object type)
	{
		Vector2 pos = -currentGraph.graphDecalPosition + currentMousePosition;
		PWNode newNode = CreateNewNode((Type)type, pos);
		currentGraph.nodes.Add(newNode);
	}

	PWNode	CreateNewNode(Type t, Vector2 position)
	{
		PWNode newNode = ScriptableObject.CreateInstance(t) as PWNode;

		//center to the middle of the screen:
		newNode.windowRect.position = position;
		newNode.SetWindowId(currentGraph.localWindowIdCount++);
		newNode.nodeTypeName = t.ToString();
		newNode.chunkSize = currentGraph.chunkSize;
		newNode.seed = currentGraph.seed;
		newNode.computeOrder = -1;
		newNode.RunNodeAwake();

		if (String.IsNullOrEmpty(newNode.name))
			newNode.name = t.Name;

		AssetDatabase.AddObjectToAsset(newNode, currentGraph);
		
		currentGraph.nodesDictionary[newNode.windowId] = newNode;
		
		return newNode;
	}

	void DeleteSubmachine(object oid)
	{
		int id = (int)oid;

		var subGraphName = currentGraph.subgraphReferences.ElementAt(id);

		if (subGraphName == null)
			return ;

		var subGraph = parentGraph.FindGraphByName(subGraphName);

		//TODO: remove all dependencies and links from the output and input machine.

		if (id < currentGraph.subgraphReferences.Count && id >= 0)
			currentGraph.subgraphReferences.RemoveAt(id);

		DeleteSubmachine(subGraph);

		graphNeedReload = true;

		AssetDatabase.SaveAssets();

		EvaluateComputeOrder();
	}

	void DeleteSubmachine(PWNodeGraph submachine)
	{
		//remove all its nodes:
		foreach (var subgraphNode in submachine.nodes)
			DestroyImmediate(subgraphNode, true);

		//remove input and output:
		if (submachine.inputNode != null)
			DestroyImmediate(submachine.inputNode, true);
		if (submachine.outputNode != null)
			DestroyImmediate(submachine.outputNode, true);

		//remove inner submachines:
		foreach (var subsubgraph in submachine.subgraphReferences)
			DeleteSubmachine(subsubgraph);

		//and finaly destroy the submachine:
		DestroyImmediate(submachine, true);
	}

	void CreatePWMachine(object mousePosition)
	{
		int	subgraphLocalWindowIdCount = 0;
		
		//calculate the subgraph starting window id count:
		int i = 0;
		PWNodeGraph g = currentGraph;
		while (g != null)
		{
			i++;
			g = parentGraph.FindGraphByName(g.parentReference);
		}
		subgraphLocalWindowIdCount = i * 1000000 + (currentGraph.localWindowIdCount++ * 10000);

		Vector2 pos = -currentGraph.graphDecalPosition + (Vector2)mousePosition;
		PWNodeGraph subgraph = ScriptableObject.CreateInstance< PWNodeGraph >();
		InitializeNewGraph(subgraph);
		subgraph.externalGraphNode = CreateNewNode(typeof(PWNodeGraphExternal), pos);
		//link external and internal nodes:
		(subgraph.externalGraphNode as PWNodeGraphExternal).InitGraphOut(subgraph.outputNode);
		(subgraph.inputNode as PWNodeGraphInput).externalGraphNode = subgraph.externalGraphNode as PWNodeGraphExternal;
		subgraph.localWindowIdCount = subgraphLocalWindowIdCount;
		subgraph.presetChoosed = true;
		subgraph.parentReference = currentGraph.name;
		subgraph.externalName = "PW sub-machine";

		subgraph.name = "sub" + currentGraph.localWindowIdCount + "-" + currentGraph.externalName;
		AssetDatabase.AddObjectToAsset(subgraph, currentGraph);
		AssetDatabase.SaveAssets();

		currentGraph.subgraphReferences.Add(subgraph.name);
		parentGraph.graphInstancies[subgraph.name] = subgraph;

		AssetDatabase.Refresh();

		currentGraph.nodesDictionary[subgraph.externalGraphNode.windowId] = subgraph.inputNode;
	}

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
		if (mouseAboveAnchorInfo.anchorType == PWAnchorType.Input)
		{
			if (mouseAboveAnchorInfo.linkCount != 0)
			{
				PWLink link = FindLinkFromAnchor(mouseAboveAnchorInfo);

				if (link != null)
					link.linkHighlight = PWLinkHighlight.Delete;
			}
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

				var from = FindNodeByWindowId(link.localWindowId);
				var to = FindNodeByWindowId(link.distantWindowId);
				
				from.DeleteLink(link.localAnchorId, to, link.distantAnchorId);
				to.DeleteLink(link.distantAnchorId, from, link.localAnchorId);

				//TODO: delete all previously linked to the mouseAboveAnchorInfo anchor.
			}
			else if (startDragAnchor.linkCount != 0)//or an output
			{
				//TODO: delete all previously linked to the startDragAnchor anchor.
			}
		}
		else if (startDragAnchor.anchorType == PWAnchorType.Input)
		{
			PWLink link = FindLinkFromAnchor(startDragAnchor);

			//displable delete highlight for link
			if (link != null)
				link.linkHighlight = PWLinkHighlight.None;
		}
	}

	PWLink FindLinkFromAnchor(PWAnchorInfo anchor)
	{
		if (anchor.anchorType == PWAnchorType.Input)
		{
			//find the anchor node
			var node = FindNodeByWindowId(anchor.windowId);
			if (node == null)
				return null;

			//get dependencies of this anchor
			var deps = node.GetDependencies(anchor.anchorId);
			if (deps.Count == 0)
				return null;

			//get the linked window from the dependency
			var linkNode = FindNodeByWindowId(deps[0].windowId);
			if (linkNode == null)
				return null;

			//find the link of the first dependency
			var links = linkNode.GetLinks(deps[0].anchorId, node.windowId, deps[0].connectedAnchorId);
			if (links.Count != 0)
				return links[0];
			return null;
		}
		else
			return null;
	}

	void DeleteAllAnchorLinks()
	{
		var node = FindNodeByWindowId(mouseAboveAnchorInfo.windowId);
		if (node == null)
			return ;
		var anchorConnections = node.GetAnchorConnections(mouseAboveAnchorInfo.anchorId);
		foreach (var ac in anchorConnections)
		{
			var n = FindNodeByWindowId(ac.first);
			if (n != null)
			{
				if (mouseAboveAnchorInfo.anchorType == PWAnchorType.Output)
					n.DeleteDependency(mouseAboveAnchorInfo.windowId, mouseAboveAnchorInfo.anchorId);
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

		var from = FindNodeByWindowId(link.localWindowId);
		var to = FindNodeByWindowId(link.distantWindowId);

		from.DeleteLink(link.localAnchorId, to, link.distantAnchorId);
		to.DeleteLink(link.distantAnchorId, from, link.localAnchorId);
		
		EvaluateComputeOrder();
	}

	void SwitchGraph(PWNodeGraph graph)
	{
		if (graph == null)
			return ;
		StopDragLink(false);
		currentGraph = graph;
	}

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
				menu.AddItem(new GUIContent("New PWMachine"), false, CreatePWMachine, e.mousePosition);
				foreach (var nodeCat in nodeSelectorList)
				{
					string menuString = "Create new/" + nodeCat.Key + "/";
					foreach (var nodeClass in nodeCat.Value)
						menu.AddItem(new GUIContent(menuString + nodeClass.name), false, CreateNewNode, nodeClass.nodeType);
				}
                menu.AddSeparator("");
				if (mouseAboveNodeAnchor)
				{
					menu.AddItem(new GUIContent("New Link"), false, BeginDragLink);
					menu.AddItem(new GUIContent("Delete all links"), false, DeleteAllAnchorLinks);
				}
				else
				{
					menu.AddDisabledItem(new GUIContent("New Link"));
					menu.AddDisabledItem(new GUIContent("Delete all links"));
				}
				var hoveredLink = currentLinks.FirstOrDefault(l => l.hover == true);
				if (hoveredLink != null)
					menu.AddItem(new GUIContent("Delete link"), false, DeleteLink, hoveredLink);
				else
					menu.AddDisabledItem(new GUIContent("Delete link"));
                menu.AddSeparator("");
				if (mouseAboveNodeIndex != -1)
					menu.AddItem(new GUIContent("Delete node"), false, DeleteNode, mouseAboveNodeIndex);
				else if (mouseAboveSubmachineIndex != -1)
					menu.AddItem(new GUIContent("Delete submachine"), false, DeleteSubmachine, mouseAboveSubmachineIndex);
				else
					menu.AddDisabledItem(new GUIContent("Delete node"));
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Go to parent"), false, () => SwitchGraph(parentGraph.FindGraphByName(currentGraph.parentReference)));

                menu.ShowAsContext();
                e.Use();
            }
        }
	}

	//Dictionary< windowId, dependencyWeight >
	Dictionary< int, int > nodeComputeOrderCount = new Dictionary< int, int >();
	int EvaluateComputeOrder(bool first = true, int depth = 0, int windowId = -1)
	{
		//Recursively evaluate compute order for each nodes:
		if (first)
		{
			nodeComputeOrderCount.Clear();
			currentGraph.inputNode.computeOrder = 0;

			foreach (var gNode in currentGraph.nodes)
				gNode.computeOrder = EvaluateComputeOrder(false, 1, gNode.windowId);
			foreach (var subGraphName in currentGraph.subgraphReferences)
			{
				PWNodeGraph g = parentGraph.FindGraphByName(subGraphName);
				if (g != null)
					g.externalGraphNode.computeOrder = EvaluateComputeOrder(false, 1, g.externalGraphNode.windowId);
			}

			currentGraph.outputNode.computeOrder = EvaluateComputeOrder(false, 1, currentGraph.outputNode.windowId);

			currentGraph.UpdateComputeOrder();
			
			return 0;
		}

		//check if we the node have already been computed:
		if (nodeComputeOrderCount.ContainsKey(windowId))
			return nodeComputeOrderCount[windowId];

		var node = FindNodeByWindowId(windowId);
		if (node == null)
			return 0;

		//check if the window have all these inputs to work:
		if (!node.CheckRequiredAnchorLink())
			return -1;

		//compute dependency weight:
		int	ret = 1;
		foreach (var dep in node.GetDependencies())
		{
			int d = EvaluateComputeOrder(false, depth + 1, dep.windowId);
			Debug.Log("compute order of node " + dep.windowId + ": " + d);

			//if dependency does not have enought datas to compute result, abort calculus.
			if (d == -1)
			{
				ret = -1;
				break ;
			}
			ret += d;
		}

		nodeComputeOrderCount[windowId] = ret;
		return ret;
	}

	static void CreateBackgroundTexture()
	{
		Func< Color, Texture2D > CreateTexture2DColor = (Color c) => {
			Texture2D	ret;
			ret = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			ret.SetPixel(0, 0, c);
			ret.Apply();
			return ret;
		};

		Func< string, Texture2D > CreateTexture2DFromFile = (string ressourcePath) => {
			return Resources.Load< Texture2D >(ressourcePath);
        };

        Color backgroundColor = new Color32(56, 56, 56, 255);
		Color resizeHandleColor = EditorGUIUtility.isProSkin
			? new Color32(56, 56, 56, 255)
            : new Color32(130, 130, 130, 255);
		Color selectorBackgroundColor = new Color32(80, 80, 80, 255);
		Color selectorCaseBackgroundColor = new Color32(110, 110, 110, 255);
		Color selectorCaseTitleBackgroundColor = new Color32(50, 50, 50, 255);
		
		backgroundTex = CreateTexture2DColor(backgroundColor);
		resizeHandleTex = CreateTexture2DColor(resizeHandleColor);
		selectorBackgroundTex = CreateTexture2DColor(selectorBackgroundColor);
		// debugTexture1 = CreateTexture2DColor(new Color(1f, 0f, 0f, .3f));
		selectorCaseBackgroundTex = CreateTexture2DColor(selectorCaseBackgroundColor);
		selectorCaseTitleBackgroundTex = CreateTexture2DColor(selectorCaseTitleBackgroundColor);

		preset2DSideViewTexture = CreateTexture2DFromFile("preview2DSideView");
		preset2DTopDownViewTexture = CreateTexture2DFromFile("preview2DTopDownView");
		preset3DPlaneTexture = CreateTexture2DFromFile("preview3DPlane");
		preset3DSphericalTexture = CreateTexture2DFromFile("preview3DSpherical");
		preset3DCubicTexture = CreateTexture2DFromFile("preview3DCubic");
		presetMeshTetxure = CreateTexture2DFromFile("previewMesh");
		preset1DDensityFieldTexture= CreateTexture2DFromFile("preview1DDensityField");
		preset2DDensityFieldTexture = CreateTexture2DFromFile("preview2DDensityField");
		preset3DDensityFieldTexture = CreateTexture2DFromFile("preview3DDensityField");
	}

    void DrawNodeCurve(Rect start, Rect end, int index, PWLink link)
    {
		Event e = Event.current;
		//swap start and end if they are inverted
		if (start.xMax > end.xMax)
			PWUtils.Swap< Rect >(ref start, ref end);

		int		id;
		if (link == null)
			id = -1;
		else
			id = GUIUtility.GetControlID((link.localName + link.distantName + index).GetHashCode(), FocusType.Passive);

        Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + Vector3.right * 100;
        Vector3 endTan = endPos + Vector3.left * 100;

		if (link != null && !draggingGraph && !draggingNode)
		{
			switch (e.GetTypeForControl(id))
			{
				case EventType.MouseDown:
					if (link.linkHighlight == PWLinkHighlight.Delete)
						break ;
					if (HandleUtility.nearestControl == id && (e.button == 0) || e.button == 1)
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
				GUIUtility.hotControl = id;
				link.hover = true;
			}
		}

		HandleUtility.AddControl(id, HandleUtility.DistancePointBezier(e.mousePosition, startPos, endPos, startTan, endTan) / 1.5f);
		if (e.type == EventType.Repaint)
		{
			PWLinkHighlight s = (link != null) ? (link.linkHighlight) : PWLinkHighlight.None;
			switch ((link != null) ? link.linkType : PWLinkType.BasicData)
			{
				case PWLinkType.Sampler2D:
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(.1f, .1f, .1f), 6, s);
					break ;
				case PWLinkType.Sampler3D:
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(.1f, .1f, .1f), 8, s);
					break ;
				case PWLinkType.ThreeChannel:
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(1f, 0f, 0f), 1, s);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(0f, 1f, 0f), 3, s);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(0f, 0f, 1f), 5, s);
					break ;
				case PWLinkType.FourChannel:
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(1f, 0f, 0f), 1, s);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(0f, 1f, 0f), 3, s);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(0f, 0f, 1f), 5, s);
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(.1f, .1f, .1f), 7, s);
					break ;
				default:
					DrawSelectedBezier(startPos, endPos, startTan, endTan, new Color(.1f, .1f, .1f), 4, s);
					break ;
			}
			if (link != null && link.linkHighlight == PWLinkHighlight.DeleteAndReset)
				link.linkHighlight = PWLinkHighlight.None;
			if (link != null && !link.selected && link.linkHighlight == PWLinkHighlight.Selected)
				link.linkHighlight = PWLinkHighlight.None;
		}
    }

	void	DrawSelectedBezier(Vector3 startPos, Vector3 endPos, Vector3 startTan, Vector3 endTan, Color c, int width, PWLinkHighlight linkHighlight)
	{
		switch (linkHighlight)
		{
			case PWLinkHighlight.Selected:
				Handles.DrawBezier(startPos, endPos, startTan, endTan, new Color(.1f, .1f, 1f, .7f), null, width + 2);
				break;
			case PWLinkHighlight.Delete:
			case PWLinkHighlight.DeleteAndReset:
				Handles.DrawBezier(startPos, endPos, startTan, endTan, new Color(1f, .1f, .1f, .85f), null, width + 2);
				break ;
		}
		Handles.DrawBezier(startPos, endPos, startTan, endTan, c, null, width);
	}
}