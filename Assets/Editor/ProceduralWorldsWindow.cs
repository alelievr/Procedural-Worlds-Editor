// #define		DEBUG_GRAPH

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;

public class ProceduralWorldsWindow : EditorWindow {

	//graph, node, anchors and links control and 
	int					currentPickerWindow;
	int					mouseAboveNodeIndex;
	int					mouseAboveSubmachineIndex;
	bool				mouseAboveNodeAnchor;
	PWOrderingGroup		mouseAboveOrderingGroup;
	bool				draggingGraph = false;
	bool				draggingLink = false;
	bool				draggingNode = false;
	bool				draggingSelectedNodes = false;
	bool				draggingSelectedNodesFromContextMenu = false;
	public static bool	graphNeedReload = false;
	bool				previewMouseDrag = false;
	PWAnchorInfo		startDragAnchor;
	PWAnchorInfo		mouseAboveAnchorInfo;
	[System.NonSerializedAttribute]
	PWNode				mouseAboveNode;
	
	//list of all links
	int					linkIndex;
	List< PWLink >		currentLinks = new List< PWLink >();

	//events fields
	Vector2				lastMousePosition;
	Vector2				presetScrollPos;
	Vector2				windowSize;
	[System.NonSerializedAttribute]
	Vector2				currentMousePosition;

	//preview fields
	GameObject			previewScene;
	Camera				previewCamera;
	RenderTexture		previewCameraRenderTexture;

	//terrain materializer
	PWTerrainBase		terrainMaterializer;

	//multi-node selection
	[System.NonSerializedAttribute]
	Rect				selectionRect;
	[System.NonSerializedAttribute]
	bool				selecting = false;

	//current and parent graph
	[SerializeField]
	public PWNodeGraph	currentGraph;
	[SerializeField]
	public PWNodeGraph	parentGraph;

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

	void AddToSelector(string key, string color, GUIStyle windowColor, GUIStyle windowColorSelected, params object[] objs)
	{
		if (!nodeSelectorList.ContainsKey(key))
			nodeSelectorList[key] = new PWNodeStorageCategory(color);
		for (int i = 0; i < objs.Length; i += 2)
			nodeSelectorList[key].nodes.Add(new PWNodeStorage((string)objs[i], (Type)objs[i + 1], windowColor, windowColorSelected));
	}
	
	void InitializeNodeSelector()
	{
		//setup nodeList:
		foreach (var n in nodeSelectorList)
			n.Value.nodes.Clear();
		
		AddToSelector("Simple values", "redNode", redNodeWindow, redNodeWindowSelected,
			"Slider", typeof(PWNodeSlider));
		AddToSelector("Operations", "yellowNode", yellowNodeWindow, yellowNodeWindowSelected,
			"Add", typeof(PWNodeAdd));
		AddToSelector("Biomes", "greenNode", greenNodeWindow, greenNodeWindowSelected,
			"Water Level", typeof(PWNodeWaterLevel),
			"To Biome data", typeof(PWNodeBiomeData),
			"Biome switch", typeof(PWNodeBiomeSwitch),
			"Biome Binder", typeof(PWNodeBiomeBinder),
			"Biome blender", typeof(PWNodeBiomeBlender));
		AddToSelector("Noises And Masks", "blueNode", blueNodeWindow, blueNodeWindowSelected,
			"Perlin noise 2D", typeof(PWNodePerlinNoise2D),
			"Circle Noise Mask", typeof(PWNodeCircleNoiseMask));
		AddToSelector("Materializers", "purpleNode", purpleNodeWindow, purpleNodeWindowSelected,
			"SideView 2D terrain", typeof(PWNodeSideView2DTerrain),
			"TopDown 2D terrain", typeof(PWNodeTopDown2DTerrain));
		AddToSelector("Debug", "orangeNode", orangeNodeWindow, orangeNodeWindowSelected,
			"DebugLog", typeof(PWNodeDebugLog));
		AddToSelector("Custom", "whiteNode", whiteNodeWindow, whiteNodeWindowSelected);
	}

	void InitializeNewGraph(PWNodeGraph graph)
	{
		//setup splitted panels:
		graph.h1 = new HorizontalSplitView(resizeHandleTexture, position.width * 0.85f, position.width / 2, position.width - 4 - 20);
		graph.h2 = new HorizontalSplitView(resizeHandleTexture, position.width * .25f, 20, position.width / 2);

		graph.graphDecalPosition = Vector2.zero;

		graph.realMode = false;
		graph.presetChoosed = false;
		
		graph.localNodeIdCount = 0;
		graph.chunkSize = 16;
		graph.step = 1;
		graph.maxStep = 4;
		
		graph.parentReference = null;
		graph.outputNode = CreateNewNode(typeof(PWNodeGraphOutput), new Vector2(position.width - 100, (int)(position.height / 2)));
		graph.inputNode = CreateNewNode(typeof(PWNodeGraphInput), new Vector2(50, (int)(position.height / 2)));

		graph.firstInitialization = "initialized";
		graph.PWGUI = new PWGUIManager();

		graph.saveName = null;
		graph.externalName = "New ProceduralWorld";
	}

	void OnEnable()
	{
		GeneratePWAssets();
		
		nodeGraphWidowStyle = new GUIStyle();
		nodeGraphWidowStyle.normal.background = defaultBackgroundTexture;

		//current PWNodeGraph file have been deleted
		if (currentGraph == null)
			currentGraph = ScriptableObject.CreateInstance< PWNodeGraph >();

		if (parentGraph != null)
			parentGraph.ForeachAllNodes((n) => { if (n != null) n.RunNodeAwake(); }, true, true);

		//force graph to reload all chunks (just after compiled)
		graphNeedReload = true;

		currentGraph.unserializeInitialized = true;
	}

#endregion

#region Global GUI rendering

    void OnGUI()
    {
		var e = Event.current;
		currentGraph.isVisibleInEditor = true;
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

 		//background color:
		if (defaultBackgroundTexture == null || !currentGraph.unserializeInitialized || resizeHandleTexture == null)
			OnEnable();

		if (currentGraph.parentReference == null || String.IsNullOrEmpty(currentGraph.parentReference))
			currentGraph.parentReference = null;

		//function is in OnGUI cause in OnEnable, the position values are bad.
		if (currentGraph.firstInitialization == null)
			InitializeNewGraph(currentGraph);
		
		if (parentGraph == null)
			parentGraph = AssetDatabase.LoadAssetAtPath< PWNodeGraph >(AssetDatabase.GetAssetPath(currentGraph));
		
		if (windowSize != Vector2.zero && windowSize != position.size)
			OnWindowResize();
		
		windowSize = position.size;
		
		if (!currentGraph.presetChoosed)
		{
			DrawPresetPanel();
			return ;
		}
		
		// initialize unserialized fields in node:
		parentGraph.ForeachAllNodes((n) => { if (n != null && !n.unserializeInitialized) n.RunNodeAwake(); }, true, true);

		//esc key event:
		if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
		{
			if (draggingLink)
				StopDragLink(false);
			selecting = false;
			draggingSelectedNodes = false;
			draggingSelectedNodesFromContextMenu = false;
			draggingNode = false;
			currentGraph.ForeachAllNodes(n => n.selected = false, false, true);
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

		currentGraph.h1.UpdateMinMax(position.width / 2, position.width - 4);
		currentGraph.h2.UpdateMinMax(0, position.width / 2);

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

		//if event, repaint
		if (e.type == EventType.mouseDown
			|| e.type == EventType.mouseDrag
			|| e.type == EventType.mouseUp
			|| e.type == EventType.scrollWheel
			|| e.type == EventType.KeyDown
			|| e.type == EventType.Repaint
			|| e.type == EventType.KeyUp)
			Repaint();

		currentGraph.assetPath = AssetDatabase.GetAssetPath(currentGraph);
		currentMousePosition = e.mousePosition;

		//render all opened popups (at the end cause the have to be above other infos)
		PWPopup.RenderAll();
		
		if (GUI.changed && e.type == EventType.Layout)
		{
			EditorUtility.SetDirty(this);
			EditorUtility.SetDirty(currentGraph);
			AssetDatabase.SaveAssets();
			Debug.Log("saved all assets !");
		}
    }

#endregion

#region Preset panel (first screen)

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
		GUI.DrawTexture(new Rect(0, 0, position.width, position.height), defaultBackgroundTexture);

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

#endregion

#region Noise preview processing and rendering

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

#endregion

#region Left bar rendering

	void DrawLeftBar(Rect currentRect)
	{
		Event	e = Event.current;
		GUI.DrawTexture(currentRect, defaultBackgroundTexture);

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
				currentGraph.externalName = EditorGUILayout.TextField("ProceduralWorld name: ", currentGraph.externalName);

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

				//seed
				EditorGUI.BeginChangeCheck();
				GUI.SetNextControlName("seed");
				parentGraph.seed = EditorGUILayout.IntField("Seed", parentGraph.seed);
				if (EditorGUI.EndChangeCheck())
				{
					parentGraph.UpdateSeed(parentGraph.seed);
					graphNeedReload = true;
				}
				
				//chunk size:
				EditorGUI.BeginChangeCheck();
				GUI.SetNextControlName("chunk size");
				parentGraph.chunkSize = EditorGUILayout.IntField("Chunk size", parentGraph.chunkSize);
				parentGraph.chunkSize = Mathf.Clamp(parentGraph.chunkSize, 1, 1024);
				if (EditorGUI.EndChangeCheck())
				{
					parentGraph.UpdateChunkSize(parentGraph.chunkSize);
					graphNeedReload = true;
				}

				//step:
				EditorGUI.BeginChangeCheck();
				float min = 0.05f;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("step", prefixLabelStyle);
				parentGraph.PWGUI.Slider(ref parentGraph.step, ref min, ref parentGraph.maxStep, 0.01f, false, true);
				EditorGUILayout.EndHorizontal();
				if (EditorGUI.EndChangeCheck())
				{
					parentGraph.UpdateStep(parentGraph.step);
					graphNeedReload = true;
				}

				if (GUILayout.Button("Force graph to reload"))
				{
					parentGraph.ForeachAllNodes(n => n.forceReload = true, true, true);
					graphNeedReload = true;
					EvaluateComputeOrder();
					Debug.Log("graph reloaded !");
				}

			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndScrollView();
		
		//free focus of the selected fields
		if (e.type == EventType.MouseDown)
			GUI.FocusControl(null);
	}

#endregion

#region Right node selector rendering

	Rect DrawSelectorCase(string name, string color, bool title = false)
	{
		if (title)
		{
			GUI.color = PWColorPalette.GetColor(color);
			GUILayout.Label(name, nodeSelectorTitleStyle);
			GUI.color = Color.white;
		}
		else
			GUILayout.Label(name, nodeSelectorCaseStyle);

		return GUILayoutUtility.GetLastRect();
	}

	void DrawSelector(Rect currentRect)
	{
		GUI.DrawTexture(currentRect, defaultBackgroundTexture);
		currentGraph.selectorScrollPosition = EditorGUILayout.BeginScrollView(currentGraph.selectorScrollPosition, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginVertical(panelBackgroundStyle);
			{
				EditorGUIUtility.labelWidth = 0;
				EditorGUIUtility.fieldWidth = 0;
				GUILayout.BeginHorizontal(toolbarStyle);
				{
					currentGraph.searchString = GUILayout.TextField(currentGraph.searchString, toolbarSearchTextStyle);
					if (GUILayout.Button("", toolbarSearchCancelButtonStyle))
					{
						// Remove focus if cleared
						currentGraph.searchString = "";
						GUI.FocusControl(null);
					}
				}
				GUILayout.EndHorizontal();
				
				foreach (var nodeCategory in nodeSelectorList)
				{
					DrawSelectorCase(nodeCategory.Key, nodeCategory.Value.color, true);
					foreach (var nodeCase in nodeCategory.Value.nodes.Where(n => n.name.IndexOf(currentGraph.searchString, System.StringComparison.OrdinalIgnoreCase) >= 0))
					{
						Rect clickableRect = DrawSelectorCase(nodeCase.name, nodeCategory.Value.color);
	
						if (Event.current.type == EventType.MouseDown && clickableRect.Contains(Event.current.mousePosition))
							CreateNewNode(nodeCase.nodeType, -currentGraph.graphDecalPosition + position.size / 2, true);
					}
				}
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndScrollView();
	}

#endregion

#region Node graph header (breadcrumbs bar)
	
	void DrawNodeGraphHeader(Rect graphRect)
	{
		Event	e = Event.current;
		EditorGUILayout.BeginVertical(navBarBackgroundStyle);
		{
			/*Rect helperBarRect = */EditorGUILayout.BeginHorizontal(navBarBackgroundStyle, GUILayout.MaxHeight(40), GUILayout.ExpandWidth(true));
			{
				if (GUILayout.Button(rencenterIconTexture, GUILayout.Width(30), GUILayout.Height(30)))
					currentGraph.graphDecalPosition = Vector2.zero;
				if (GUILayout.Button(fileIconTexture, GUILayout.Width(30), GUILayout.Height(30)))
					EditorGUIUtility.PingObject(currentGraph);
			}
			EditorGUILayout.EndHorizontal();
			Rect breadcrumbsRect = EditorGUILayout.BeginHorizontal(navBarBackgroundStyle, GUILayout.MaxHeight(20), GUILayout.ExpandWidth(true));
			{
				breadcrumbsRect.yMin -= 1;
				breadcrumbsRect.xMin -= 1;
				
				PWNodeGraph g = currentGraph;
				var breadcrumbsList = new List< string >();
				while (g != null)
				{
					breadcrumbsList.Add(g.name);
					g = parentGraph.FindGraphByName(g.parentReference);
				}
				breadcrumbsList.Reverse();
				bool first = true;
				foreach (var b in breadcrumbsList)
				{
					if (GUILayout.Button(b, (first) ? breadcrumbsButtonLeftStyle : breadcrumbsButtonStyle, GUILayout.MaxWidth(150)))
						SwitchGraph(parentGraph.FindGraphByName(b));
					first = false;
				}
			}
			EditorGUILayout.EndHorizontal();
	
			//remove 4 pixels for the separation bar
			graphRect.size -= Vector2.right * 4;
	
			#if (DEBUG_GRAPH)
			foreach (var node in nodes)
				GUI.DrawTexture(PWUtils.DecalRect(node.rect, graphDecalPosition), debugTexture1);
			#endif
	
			if (e.type == EventType.MouseDown) //if event is mouse down
			{
				//TODO: remove the graph header height
				if (graphRect.Contains(e.mousePosition))
				{
					if (e.button == 2 || (e.command && e.button == 0))
						draggingGraph = true;
				}
				if (!mouseAboveNodeAnchor //if mouse is not above a node anchor
					&& mouseAboveNodeIndex == -1 //and mouse is notabove a node
					&& mouseAboveSubmachineIndex == -1 //and mouse is not above a submachine
					&& e.button == 0
					&& !e.command
					&& !e.control)
				{
					selecting = true;
					selectionRect.position = e.mousePosition;
					selectionRect.size = Vector2.zero;
				}
			}
			if (e.type == EventType.MouseUp)
			{
				selecting = false;
				draggingGraph = false;
				previewMouseDrag = false;
				draggingSelectedNodes = false;
				draggingSelectedNodesFromContextMenu = false;
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

#endregion

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

	void RenderNode(int id, PWNode node, string name, int index, ref bool mouseAboveAnchorLocal, ref bool mouseDraggingWindowLocal, bool submachine = false)
	{
		Event	e = Event.current;

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

				//attach link to the node:
				node.AttachLink(mouseAboveAnchor, startDragAnchor);
				var win = FindNodeById(startDragAnchor.nodeId);
				if (win != null)
				{
					win.AttachLink(startDragAnchor, mouseAboveAnchor);
					graphNeedReload = true;
				}
				else
					Debug.LogWarning("window id not found: " + startDragAnchor.nodeId);
				
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
		Event	e = Event.current;
		int		i;
		
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
			//if we are on the mother graph, render the terrain
			//TODO: other preview type for graph outputs in function of graph output type.
			if (e.type == EventType.Layout)
			{
				currentGraph.ForeachAllNodes(p => p.BeginFrameUpdate());
				if (graphNeedReload)
				{
					terrainMaterializer.DestroyAllChunks();
					graphNeedReload = false;
				}
				//updateChunks will update and generate new chunks if needed.
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
				string nodeName = (string.IsNullOrEmpty(node.externalName)) ? node.nodeTypeName : node.externalName;
				RenderNode(nodeId++, node, nodeName, i, ref mouseAboveAnchorLocal, ref draggingNodeLocal);
			}

			//display graph sub-PWGraphs
			i = 0;
			foreach (var graphName in currentGraph.subgraphReferences)
			{
				var graph = parentGraph.FindGraphByName(graphName);
				if (graph)
					RenderNode(nodeId++, graph.externalGraphNode, graph.externalGraphNode.externalName, i, ref mouseAboveAnchorLocal, ref draggingNodeLocal, true);
				i++;
			}

			//display the upper graph reference:
			if (currentGraph.parentReference != null)
				RenderNode(nodeId++, currentGraph.inputNode, "upper graph", -2, ref mouseAboveAnchorLocal, ref draggingNodeLocal);
			RenderNode(nodeId++, currentGraph.outputNode, "output", -2, ref mouseAboveAnchorLocal, ref draggingNodeLocal);

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

				//duplicate selected subgraphs
				/*var dupgList = new List< PWNodeGraph >();
				foreach (var subgraphName in currentGraph.subgraphReferences)
				{
					PWNodeGraph pwng = parentGraph.FindGraphByName(subgraphName);

					if (pwng.externalGraphNode.selected)
						dupgList.Add(Instantiate(pwng));
				}

				foreach (var toAdd in dupgList)
				{
					CreateNewNode(toAdd.externalGraphNode, toAdd.externalGraphNode.windowRect.position + new Vector2(40, 40), toAdd.externalName, true);

					//TODO: duplicate inner nodes too
				}*/

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
			int		reloadWeight = 0;
			currentGraph.ForeachAllNodes(p => {
				if (e.type == EventType.Layout)
				{
					p.EndFrameUpdate();
					if (p.notifyDataChanged)
					{
						graphNeedReload = true;
						p.notifyDataChanged = false;
						reloadRequested = true;
						reloadWeight = p.computeOrder;
					}
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

#endregion

#region Node, Submachine and OrderingGroup Utils

	void OnWindowResize()
	{
		//calcul the ratio for the window move:
		float r = position.size.x / windowSize.x;

		currentGraph.h1.handlerPosition *= r;
		currentGraph.h2.handlerPosition *= r;
	}

	void DeleteNode(object oNodeIndex)
	{
		int	id = (int)oNodeIndex;
		if (id < 0 || id > currentGraph.nodes.Count)
		{
			Debug.LogWarning("cant remove this node !");
			return ;
		}

		var node = currentGraph.nodes[id];

		if (node == null)
			return ;

		currentGraph.nodes.RemoveAt(id);

		DeleteNode(node);

		EvaluateComputeOrder();
	}

	void DeleteNode(PWNode node)
	{
		graphNeedReload = true;
		//remove all input links for each node links:
		foreach (var link in node.GetLinks())
		{
			var n = FindNodeById(link.distantNodeId);
			if (n != null)
				n.DeleteDependenciesByWindowTarget(link.localNodeId);
		}
		//remove all links for node dependencies
		foreach (var deps in node.GetDependencies())
		{
			var n = FindNodeById(deps.nodeId);
			if (n != null)
				n.DeleteLinkByWindowTarget(node.nodeId);
		}

		//remove the node
		currentGraph.nodesDictionary.Remove(node.nodeId);
		DestroyImmediate(node, true);
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

	void DeleteSubmachine(object oid)
	{
		int id = (int)oid;

		var subGraphName = currentGraph.subgraphReferences.ElementAt(id);

		if (subGraphName == null)
			return ;

		var subGraph = parentGraph.FindGraphByName(subGraphName);

		if (id < currentGraph.subgraphReferences.Count && id >= 0)
			currentGraph.subgraphReferences.RemoveAt(id);

		DeleteSubmachine(subGraph);

		graphNeedReload = true;

		AssetDatabase.SaveAssets();

		EvaluateComputeOrder();
	}

	void DeleteSubmachine(PWNodeGraph submachine)
	{
		if (submachine == null)
			return ;
		
		//remove all its nodes:
		foreach (var subgraphNode in submachine.nodes)
			DeleteNode(subgraphNode);

		//remove input and output:
		if (submachine.inputNode != null)
			DeleteNode(submachine.inputNode);
		if (submachine.outputNode != null)
			DeleteNode(submachine.outputNode);
		if (submachine.externalGraphNode != null)
			DeleteNode(submachine.externalGraphNode);

		//remove inner submachines:
		foreach (var subsubgraph in submachine.subgraphReferences)
			DeleteSubmachine(parentGraph.FindGraphByName(subsubgraph));

		//and finaly destroy the submachine:
		DestroyImmediate(submachine, true);
	}

	void CreatePWMachine(object mousePosition)
	{
		int	subgraphlocalNodeIdCount = 0;
		
		//calculate the subgraph starting window id count:
		int i = 0;
		PWNodeGraph g = currentGraph;
		while (g != null)
		{
			i++;
			g = parentGraph.FindGraphByName(g.parentReference);
		}
		subgraphlocalNodeIdCount = i * 1000000 + (currentGraph.localNodeIdCount++ * 10000);

		Vector2 pos = -currentGraph.graphDecalPosition + (Vector2)mousePosition;
		PWNodeGraph subgraph = ScriptableObject.CreateInstance< PWNodeGraph >();
		InitializeNewGraph(subgraph);
		subgraph.externalGraphNode = CreateNewNode(typeof(PWNodeGraphExternal), pos);
		//link external and internal nodes:
		(subgraph.externalGraphNode as PWNodeGraphExternal).InitGraphOut(subgraph.inputNode, subgraph.outputNode);
		(subgraph.outputNode as PWNodeGraphOutput).InitExternalNode(subgraph.externalGraphNode);
		subgraph.localNodeIdCount = subgraphlocalNodeIdCount;
		subgraph.presetChoosed = true;
		subgraph.parentReference = currentGraph.name;
		subgraph.externalName = "PW sub-machine";

		//copy the current layout:
		subgraph.h1.handlerPosition = currentGraph.h1.handlerPosition;
		subgraph.h2.handlerPosition = currentGraph.h2.handlerPosition;

		//assign new unique name
		subgraph.name = "sub" + currentGraph.localNodeIdCount + "-" + currentGraph.externalName;
		subgraph.externalGraphNode.externalName = subgraph.name;

		//save object to disk
		AssetDatabase.AddObjectToAsset(subgraph, currentGraph);
		AssetDatabase.SaveAssets();

		//add this new graph to the global graph storage
		currentGraph.subgraphReferences.Add(subgraph.name);
		parentGraph.graphInstancies[subgraph.name] = subgraph;

		AssetDatabase.Refresh();

		currentGraph.nodesDictionary[subgraph.externalGraphNode.nodeId] = subgraph.externalGraphNode;
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
		foreach (var g in graphToRemove)
		{
			currentGraph.subgraphReferences.Remove(g.name);
			DeleteSubmachine(g);
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
			if (deps.Count == 0)
				return null;

			//get the linked window from the dependency
			var linkNode = FindNodeById(deps[0].nodeId);
			if (linkNode == null)
				return null;

			//find the link of each dependency
			IEnumerable< PWLink > links = new List< PWLink >();
			foreach (var dep in deps)
				links = links.Concat(linkNode.GetLinks(dep.anchorId, node.nodeId, dep.connectedAnchorId));
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

	void UpdateLinkMode(PWLink link, PWProcessMode newMode)
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
				menu.AddItem(new GUIContent("New submachine"), false, CreatePWMachine, e.mousePosition);
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
					menu.AddItem(new GUIContent("Link/AutoProcess mode"), hoveredLink.mode == PWProcessMode.AutoProcess, () => { UpdateLinkMode(hoveredLink, PWProcessMode.AutoProcess); });
					menu.AddItem(new GUIContent("Link/RequestForProcess mode"), hoveredLink.mode == PWProcessMode.RequestForProcess, () => { UpdateLinkMode(hoveredLink, PWProcessMode.RequestForProcess); });
					menu.AddItem(new GUIContent("Link/Delete link"), false, DeleteLink, hoveredLink);
				}
				else
					menu.AddDisabledItem(new GUIContent("Link"));

                menu.AddSeparator("");
				if (mouseAboveNodeIndex != -1)
					menu.AddItem(new GUIContent("Delete node"), false, DeleteNode, mouseAboveNodeIndex);
				else if (mouseAboveSubmachineIndex != -1)
					menu.AddItem(new GUIContent("Delete submachine"), false, DeleteSubmachine, mouseAboveSubmachineIndex);
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

				menu.AddSeparator("");
				if (currentGraph.parentReference != null)
					menu.AddItem(new GUIContent("Go to parent"), false, () => SwitchGraph(parentGraph.FindGraphByName(currentGraph.parentReference)));
				else
					menu.AddDisabledItem(new GUIContent("Go to parent"));

                menu.ShowAsContext();
                e.Use();
            }
        }
	}

#endregion
	
#region Utils and miscellaneous

	//Dictionary< nodeId, dependencyWeight >
	Dictionary< int, int > nodeComputeOrderCount = new Dictionary< int, int >();
	int EvaluateComputeOrder(bool first = true, int depth = 0, int nodeId = -1)
	{
		//Recursively evaluate compute order for each nodes:
		if (first)
		{
			nodeComputeOrderCount.Clear();
			currentGraph.inputNode.computeOrder = 0;

			foreach (var gNode in currentGraph.nodes)
				gNode.computeOrder = EvaluateComputeOrder(false, 1, gNode.nodeId);
			foreach (var subGraphName in currentGraph.subgraphReferences)
			{
				PWNodeGraph g = parentGraph.FindGraphByName(subGraphName);
				if (g != null)
					g.externalGraphNode.computeOrder = EvaluateComputeOrder(false, 1, g.externalGraphNode.nodeId);
			}

			currentGraph.outputNode.computeOrder = EvaluateComputeOrder(false, 1, currentGraph.outputNode.nodeId);

			currentGraph.UpdateComputeOrder();

			currentGraph.RebakeGraphParts();

			return 0;
		}

		//check if we the node have already been computed:
		if (nodeComputeOrderCount.ContainsKey(nodeId))
			return nodeComputeOrderCount[nodeId];

		var node = FindNodeById(nodeId);
		if (node == null)
			return 0;

		//check if the window have all these inputs to work:
		if (!node.CheckRequiredAnchorLink())
			return -1;

		//compute dependency weight:
		int	ret = 1;
		foreach (var dep in node.GetDependencies())
		{
			int d = EvaluateComputeOrder(false, depth + 1, dep.nodeId);

			//if dependency does not have enought datas to compute result, abort calculus.
			if (d == -1)
			{
				ret = -1;
				break ;
			}
			ret += d;
		}

		nodeComputeOrderCount[nodeId] = ret;
		return ret;
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

	void SwitchGraph(PWNodeGraph graph)
	{
		if (graph == null)
			return ;
		currentGraph.isVisibleInEditor = false;
		StopDragLink(false);
		currentGraph = graph;
		currentGraph.isVisibleInEditor = true;
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

			//TODO: copy only required styles to the new style
		}
		if (nodeSelectorList.Count == 0)
			InitializeNodeSelector();
			
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
				GUIUtility.hotControl = id;
				link.hover = true;
			}
		}

		HandleUtility.AddControl(id, HandleUtility.DistancePointBezier(e.mousePosition, startPos, endPos, startTan, endTan) / 1.5f);
		if (e.type == EventType.Repaint)
		{
			PWLinkHighlight s = (link != null) ? (link.linkHighlight) : PWLinkHighlight.None;
			PWProcessMode m = (link != null) ? link.mode : PWProcessMode.AutoProcess;
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

	void	DrawSelectedBezier(Vector3 startPos, Vector3 endPos, Vector3 startTan, Vector3 endTan, Color c, int width, PWLinkHighlight linkHighlight, PWProcessMode linkMode)
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

		if (linkMode == PWProcessMode.RequestForProcess)
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