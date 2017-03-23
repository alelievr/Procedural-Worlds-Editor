// #define		DEBUG_GRAPH

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using PW;
using UnityStandardAssets.ImageEffects;

public class ProceduralWorldsWindow : EditorWindow {

    private static Texture2D	backgroundTex;
	private static Texture2D	resizeHandleTex;
	private static Texture2D	selectorBackgroundTex;
	private static Texture2D	debugTexture1;
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
	Vector2				lastMousePosition;
	Vector2				presetScrollPos;
	Vector2				windowSize;

	GameObject			previewScene;
	Camera				previewCamera;
	RenderTexture		previewCameraRenderTexture;

	PWTerrainBase		terrainMaterializer;

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
	Dictionary< string, List< PWNodeStorage > > nodeSelectorList = new Dictionary< string, List< PWNodeStorage > >();

	[System.NonSerializedAttribute]
	Dictionary< string, Dictionary< string, FieldInfo > > bakedNodeFields = new Dictionary< string, Dictionary< string, FieldInfo > >();

	[System.NonSerializedAttribute]
	Dictionary< PWOutputType, Type > terrainRenderers = new Dictionary< PWOutputType, Type >()
	{
		{PWOutputType.SIDEVIEW_2D, typeof(PWSideView2DTerrain)},
		{PWOutputType.TOPDOWNVIEW_2D, typeof(PWTopDown2DTerrain)},
	};

	[MenuItem("Window/Procedural Worlds")]
	static void Init()
	{
		ProceduralWorldsWindow window = (ProceduralWorldsWindow)EditorWindow.GetWindow (typeof (ProceduralWorldsWindow));

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
		
		graph.outputNode = ScriptableObject.CreateInstance< PWNodeGraphOutput >();
		graph.outputNode.SetWindowId(currentGraph.localWindowIdCount++);
		graph.outputNode.windowRect.position = new Vector2(position.width - 100, (int)(position.height / 2));

		graph.inputNode = ScriptableObject.CreateInstance< PWNodeGraphInput >();
		graph.inputNode.SetWindowId(currentGraph.localWindowIdCount++);
		graph.inputNode.windowRect.position = new Vector2(50, (int)(position.height / 2));

		graph.firstInitialization = "initialized";

		graph.saveName = null;
		graph.name = "New ProceduralWorld";
	}

	void BakeNode(Type t)
	{
		var dico = new Dictionary< string, FieldInfo >();
		bakedNodeFields[t.AssemblyQualifiedName] = dico;

		foreach (var field in t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			dico[field.Name] = field;
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

		//bake the fieldInfo types:
		bakedNodeFields.Clear();
		foreach (var nodeCat in nodeSelectorList)
			foreach (var nodeClass in nodeCat.Value)
				BakeNode(nodeClass.nodeType);
		BakeNode(typeof(PWNodeGraphOutput));
		BakeNode(typeof(PWNodeGraphInput));
		
		if (currentGraph == null)
			currentGraph = ScriptableObject.CreateInstance< PWNodeGraph >();
			
		//clear the corrupted node:
		for (int i = 0; i < currentGraph.nodes.Count; i++)
			if (currentGraph.nodes[i] == null)
				DeleteNode(i--);
	}

    void OnGUI()
    {
		EditorUtility.SetDirty(this);

		//initialize graph the first time he was created
		//function is in OnGUI cause in OnEnable, the position values are bad.
		if (currentGraph.firstInitialization == null)
			InitializeNewGraph(currentGraph);
			
		//text colors:
		whiteText = new GUIStyle();
		whiteText.normal.textColor = Color.white;
		whiteBoldText = new GUIStyle();
		whiteBoldText.fontStyle = FontStyle.Bold;
		whiteBoldText.normal.textColor = Color.white;

        //background color:
		if (backgroundTex == null || currentGraph.h1 == null || resizeHandleTex == null)
			OnEnable();
		
		if (currentGraph.firstInitialization != "initialized")
			return ;

		if (!currentGraph.presetChoosed)
		{
			DrawPresetPanel();
			return ;
		}

		if (windowSize != Vector2.zero && windowSize != position.size)
			OnWindowResize();

		//esc key event:
		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
		{
			if (currentGraph.draggingLink)
				currentGraph.draggingLink = false;
		}

		GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), backgroundTex, ScaleMode.StretchToFill);

		if (Event.current.type == EventType.Layout)
			ProcessPreviewScene(currentGraph.outputType);

		if (terrainMaterializer == null)
			terrainMaterializer = GameObject.Find("PWPreviewTerrain").GetComponent< PWTerrainBase >();

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
					callback();
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

	void LambdaIfNotNull< T >(T obj, Action< T > callback)
	{
		if (obj != null)
			callback(obj);
	}

	void ProcessPreviewScene(PWOutputType outputType)
	{
		if (previewScene == null)
		{
			//TODO: try find the previewScene by name
			//delete it if outputType does not match the preewview scene type.
			switch (outputType)
			{
				case PWOutputType.DENSITY_2D:
				case PWOutputType.SIDEVIEW_2D:
					previewScene = Instantiate(Resources.Load("PWPreviewSideView2D", typeof(GameObject)) as GameObject);
					break ;
				case PWOutputType.TOPDOWNVIEW_2D:
					previewScene = Instantiate(Resources.Load("PWPreviewTopDown2D", typeof(GameObject)) as GameObject);
					break ;
				default: //for 3d previewScenes:
					previewScene = Instantiate(Resources.Load("PWPreview3D", typeof(GameObject)) as GameObject);
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
		if (terrainMaterializer.initialized == false)
			terrainMaterializer.InitGraph(currentGraph);
	}

	void DrawLeftBar(Rect currentRect)
	{
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
				currentGraph.name = EditorGUILayout.TextField("ProceduralWorld name: ", currentGraph.name);

				if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.Ignore)
					&& !GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)
					&& GUI.GetNameOfFocusedControl() == "PWName")
					GUI.FocusControl(null);
		
				if (currentGraph.parent == null)
				{
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Load graph"))
					{
						currentPickerWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
						EditorGUIUtility.ShowObjectPicker< PWNodeGraph >(null, false, "", currentPickerWindow);
					}
					else if (GUILayout.Button("Save this graph"))
					{
						if (currentGraph.saveName != null)
							return ;
	
						string path = AssetDatabase.GetAssetPath(Selection.activeObject);
						if (path == "")
							path = "Assets";
						else if (Path.GetExtension(path) != "")
							path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
	
						currentGraph.saveName = currentGraph.name;
						string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + currentGraph.saveName + ".asset");
	
						AssetDatabase.CreateAsset(currentGraph, assetPathAndName);
	
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
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
					EditorGUILayout.EndHorizontal();
				}

				GUI.DrawTexture(previewRect, previewCameraRenderTexture);
		
				//TODO: draw infos / debug / global settings view

				if (currentGraph.parent == null)
				{
					EditorGUI.BeginChangeCheck();
					currentGraph.seed = EditorGUILayout.IntField("Seed", currentGraph.seed);
					if (EditorGUI.EndChangeCheck())
						ForeachAllNodes((p) => p.seed = currentGraph.seed, true, true);
					
					//chunk size:
					EditorGUI.BeginChangeCheck();
					currentGraph.chunkSize = EditorGUILayout.IntField("Chunk size", currentGraph.chunkSize);
					if (EditorGUI.EndChangeCheck())
						ForeachAllNodes((p) => p.chunkSize = currentGraph.chunkSize, true, true);
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
		EditorGUILayout.BeginVertical(splittedPanel);
		{
			//TODO: render the breadcrumbs bar
	
			//remove 4 pixels for the separation bar
			graphRect.size -= Vector2.right * 4;
	
			#if (DEBUG_GRAPH)
			foreach (var node in nodes)
				GUI.DrawTexture(PWUtils.DecalRect(node.rect, graphDecalPosition), debugTexture1);
			#endif
	
			if (Event.current.type == EventType.MouseDown //if event is mouse down
				&& Event.current.button == 0
				&& !currentGraph.mouseAboveNodeAnchor //if mouse is not above a node anchor
				&& graphRect.Contains(Event.current.mousePosition) //and mouse position is in graph
				&& !currentGraph.nodes.Any(n => PWUtils.DecalRect(n.windowRect,currentGraph. graphDecalPosition, true).Contains(Event.current.mousePosition))) //and mouse is not above a window
				currentGraph.dragginGraph = true;
			if (Event.current.type == EventType.MouseUp)
				currentGraph.dragginGraph = false;
			if (Event.current.type == EventType.Layout)
			{
				if (currentGraph.dragginGraph)
					currentGraph.graphDecalPosition += Event.current.mousePosition - lastMousePosition;
				lastMousePosition = Event.current.mousePosition;
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
		var ret = currentGraph.nodes.FirstOrDefault(n => n.windowId == id);

		if (ret != null)
			return ret;
		var gInput = currentGraph.subGraphs.FirstOrDefault(g => g.inputNode.windowId == id);
		if (gInput != null && gInput.inputNode != null)
			return gInput.inputNode;
		var gOutput = currentGraph.subGraphs.FirstOrDefault(g => g.outputNode.windowId == id);
		if (gOutput != null && gOutput.outputNode != null)
			return gOutput.outputNode;

		if (currentGraph.inputNode.windowId == id)
			return currentGraph.inputNode;
		if (currentGraph.outputNode.windowId == id)
			return currentGraph.outputNode;

		return null;
	}

	void RenderNode(int id, PWNode node, string name, int index, ref bool mouseAboveAnchorLocal, bool submachine = false)
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
		if (currentGraph.draggingLink)
			node.HighlightLinkableAnchorsTo(currentGraph.startDragAnchor);
		node.DisplayHiddenMultipleAnchors(currentGraph.draggingLink);

		//process envent, state and position for node anchors:
		var mouseAboveAnchor = node.ProcessAnchors();
		if (mouseAboveAnchor.mouseAbove)
			mouseAboveAnchorLocal = true;

		//if you press the mouse above an anchor, start the link drag
		if (mouseAboveAnchor.mouseAbove && e.type == EventType.MouseDown)
		{
			currentGraph.startDragAnchor = mouseAboveAnchor;
			currentGraph.draggingLink = true;
		}

		//render node anchors:
		node.RenderAnchors();

		//end dragging:
		if (e.type == EventType.mouseUp && currentGraph.draggingLink == true)
			if (mouseAboveAnchor.mouseAbove)
			{
				//attach link to the node:
				node.AttachLink(mouseAboveAnchor, currentGraph.startDragAnchor);
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
		var links = node.GetLinks();
		foreach (var link in links)
		{
			// Debug.Log("link: " + link.localWindowId + ":" + link.localAnchorId + " to " + link.distantWindowId + ":" + link.distantAnchorId);
			var fromWindow = FindNodeByWindowId(link.localWindowId);
			var toWindow = FindNodeByWindowId(link.distantWindowId);

			if (toWindow == null) //invalid window ids
			{
				node.RemoveLinkByWindowTarget(link.distantWindowId);
				Debug.LogWarning("window not found: " + link.localWindowId + ", " + link.distantWindowId);
				continue ;
			}

			Rect? fromAnchor = fromWindow.GetAnchorRect(link.localAnchorId);
			Rect? toAnchor = toWindow.GetAnchorRect(link.distantAnchorId);
			if (fromAnchor != null && toAnchor != null)
				DrawNodeCurve(fromAnchor.Value, toAnchor.Value, Color.black);
		}

		//check if user have pressed the close button of this window:
		if (node.WindowShouldClose())
			DeleteNode(index);
	}

	void RenderNodeLinks(PWNode node)
	{
		var links = node.GetLinks();

		foreach (var link in links)
		{
			var target = FindNodeByWindowId(link.distantWindowId);

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
				{
					if (!values.AssignAt(link.distantIndex, val, link.localName))
						Debug.Log("failed to set distant indexed field value: " + link.distantName);
				}
			}
		}
	}

	void DrawNodeGraphCore()
	{
		Event	e = Event.current;
		int		i;

		Rect graphRect = EditorGUILayout.BeginHorizontal();
		{
			if (Event.current.type == EventType.Layout)
				ForeachAllNodes(p => p.BeginFrameUpdate());
			//We run the calcul the nodes:
			if (e.type == EventType.Layout)
			{
				currentGraph.ProcessGraph();

				terrainMaterializer.UpdateChunks();
			}

			bool	mouseAboveAnchorLocal = false;
			mouseAboveNodeIndex = -1;
			mouseAboveSubmachineIndex = -1;
			PWNode.windowRenderOrder = 0;
			int		windowId = 0;
			BeginWindows();
			for (i = 0; i < currentGraph.nodes.Count; i++)
			{
				var node = currentGraph.nodes[i];
				string nodeName = (string.IsNullOrEmpty(node.name)) ? node.nodeTypeName : node.name;
				RenderNode(windowId++, node, nodeName, i, ref mouseAboveAnchorLocal);
			}

			//display graph sub-PWGraphs
			i = 0;
			foreach (var graph in currentGraph.subGraphs)
			{
				graph.outputNode.useExternalWinowRect = true;
				RenderNode(windowId++, graph.outputNode, graph.name, i, ref mouseAboveAnchorLocal, true);
				i++;
			}

			//display the upper graph reference:
			if (currentGraph.parent != null)
				RenderNode(windowId++, currentGraph.inputNode, "upper graph", -1, ref mouseAboveAnchorLocal);
			RenderNode(windowId++, currentGraph.outputNode, "output", -1, ref mouseAboveAnchorLocal);

			EndWindows();
			
			if (e.type == EventType.Repaint)
			{
				if (currentGraph.parent != null)
					RenderNodeLinks(currentGraph.inputNode);
				foreach (var node in currentGraph.nodes)
					RenderNodeLinks(node);
				foreach (var graph in currentGraph.subGraphs)
					RenderNodeLinks(graph.outputNode);
				RenderNodeLinks(currentGraph.outputNode);
			}
			
			//submachine enter button click management:
			foreach (var graph in currentGraph.subGraphs)
			{
				if (graph.outputNode.specialButtonClick)
				{
					//enter to subgraph:
					currentGraph.draggingLink = false;
					graph.outputNode.useExternalWinowRect = false;
					currentGraph = graph;
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

			if (Event.current.type == EventType.Layout)
				ForeachAllNodes(p => p.EndFrameUpdate());
		}
		EditorGUILayout.EndHorizontal();
	}

	void OnWindowResize()
	{

	}

	void DeleteNode(object oid)
	{
		int	id = (int)oid;

		//remove all input links for each node links:
		foreach (var link in currentGraph.nodes[id].GetLinks())
		{
			var node = FindNodeByWindowId(link.distantWindowId);
			if (node != null)
				node.RemoveDependency(link.localWindowId);
		}
		//remove all links for node dependencies
		foreach (var deps in currentGraph.nodes[id].GetDependencies())
		{
			var node = FindNodeByWindowId(deps);
			if (node != null)
				node.RemoveLinkByWindowTarget(deps);
		}

		//remove the node
		currentGraph.nodes.RemoveAt(id);
	}

	void CreateNewNode(object type)
	{
		//TODO: if mouse is in the node graph, add the new node at the mouse position instead of the center of the window
		Type t = (Type)type;
		PWNode newNode = ScriptableObject.CreateInstance(t) as PWNode;
		//center to the middle of the screen:
		newNode.windowRect.position = -currentGraph.graphDecalPosition + new Vector2((int)(position.width / 2), (int)(position.height / 2));
		newNode.SetWindowId(currentGraph.localWindowIdCount++);
		newNode.nodeTypeName = t.ToString();
		newNode.chunkSize = currentGraph.chunkSize;
		currentGraph.nodes.Add(newNode);
	}

	void DeleteSubmachine(object oid)
	{
		int id = (int)oid;

		if (id < currentGraph.subGraphs.Count && id >= 0)
			currentGraph.subGraphs.RemoveAt(id);
	}

	void CreatePWMachine()
	{
		Vector2 pos = -currentGraph.graphDecalPosition + new Vector2((int)(position.width / 2), (int)(position.height / 2));
		PWNodeGraph subgraph = ScriptableObject.CreateInstance< PWNodeGraph >();
		InitializeNewGraph(subgraph);
		subgraph.presetChoosed = true;
		subgraph.inputNode.useExternalWinowRect = true;
		subgraph.inputNode.windowRect.position = pos;
		subgraph.parent = currentGraph;
		subgraph.name = "PW sub-machine";
		currentGraph.subGraphs.Add(subgraph);
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
				if (mouseAboveNodeIndex != -1)
					menu.AddItem(new GUIContent("Delete node"), false, DeleteNode, mouseAboveNodeIndex);
				else if (mouseAboveSubmachineIndex != -1)
					menu.AddItem(new GUIContent("Delete submachine"), false, DeleteSubmachine, mouseAboveSubmachineIndex);
				else
					menu.AddDisabledItem(new GUIContent("Delete node"));
                menu.AddSeparator("");
				menu.AddItem(new GUIContent("New PWMachine"), false, CreatePWMachine);
				foreach (var nodeCat in nodeSelectorList)
				{
					string menuString = "Create new/" + nodeCat.Key + "/";
					foreach (var nodeClass in nodeCat.Value)
						menu.AddItem(new GUIContent(menuString + nodeClass.name), false, CreateNewNode, nodeClass.nodeType);
				}
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

		var node = FindNodeByWindowId(windowId);
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
		debugTexture1 = CreateTexture2DColor(new Color(1f, 0f, 0f, .3f));
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

	void ForeachAllNodes(Action< PWNode > callback, bool recursive = false, bool graphInputAndOutput = false, PWNodeGraph graph = null)
	{
		if (graph == null)
			graph = currentGraph;
		foreach (var node in graph.nodes)
			callback(node);
		if (graphInputAndOutput)
		{
			callback(graph.inputNode);
			callback(graph.outputNode);
		}
		if (recursive)
			foreach (var subgraph in graph.subGraphs)
				ForeachAllNodes(callback, recursive, graphInputAndOutput, subgraph);
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