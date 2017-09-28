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
public partial class PWMainGraphEditor : PWGraphEditor {

	[SerializeField]
	PWMainGraph						mainGraph { get { return graph as PWMainGraph; } }
	
	[SerializeField]
	public HorizontalSplitView		h1;
	[SerializeField]
	public HorizontalSplitView		h2;
	[SerializeField]
	public string					searchString = "";
	
	//graph, node, anchors and links control and 
	bool				previewMouseDrag = false;
	bool				editorNeedRepaint = false;
	[System.NonSerializedAttribute]
	PWNode				mouseAboveNode;
	
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

#region Internal editor styles and textures

	private static Texture2D	resizeHandleTexture;

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
	private static Texture2D	eyeIconTexture;
	
#endregion

#region Initialization and data baking

	[MenuItem("Procedural Worlds/Main graph")]
	static void Init()
	{
		PWMainGraphEditor window = (PWMainGraphEditor)EditorWindow.GetWindow (typeof (PWMainGraphEditor));

		window.Show();
	}

/*	void InitializeNewGraph(PWNodeGraph graph)
	{
		//setup splitted panels:
	}*/

	Color resizeHandleColor;
	public override void OnEnable()
	{
		base.OnEnable();
		
		LoadStyles();
		
		LoadAssets();
	}

#endregion

#region Global GUI rendering

	//call all rendering methods:
    public override void OnGUI()
    {
		//render the whole graph
		base.OnGUI();

		var e = Event.current;

		//prevent popup events to influence the rest of the GUI
		PWPopup.eventType = e.type;
		PWGUIManager.editorWindowRect = position;
		if (PWPopup.mouseAbove && e.type != EventType.Repaint && e.type != EventType.Layout)
			e.type = EventType.Ignore;

		//update the current GUI settings storage and clear drawed popup list:
		mainGraph.PWGUI.StartFrame();
		if (e.type == EventType.Layout)
			PWPopup.ClearAll();

		//text colors:

		if (windowSize != Vector2.zero && windowSize != position.size)
			OnWindowResize();
		
		windowSize = position.size;
		
		if (!mainGraph.presetChoosed)
		{
			DrawPresetPanel();
			return ;
		}
		
		if (e.type == EventType.Layout)
			ProcessPreviewScene(mainGraph.outputType);

		if (terrainMaterializer == null)
		{
			GameObject gtm = GameObject.Find("PWPreviewTerrain");
			if (gtm != null)
				terrainMaterializer = gtm.GetComponent< PWTerrainBase >();
		}

		h1.UpdateMinMax(position.width / 2, position.width - 3);
		h2.UpdateMinMax(50, position.width / 2);

		h1.Begin();
		Rect p1 = h2.Begin();
		DrawLeftBar(p1);
		Rect g = h2.Split(resizeHandleColor);
		DrawNodeGraphHeader(g);
		h2.End();
		Rect p2 = h1.Split(resizeHandleColor);
		DrawSelector(p2);
		h1.End();

		//FIXME
		if (!editorNeedRepaint)
			editorNeedRepaint = e.isMouse || e.type == EventType.ScrollWheel;

		//if event, repaint
		if ((editorNeedRepaint))
		{
			Repaint();
			editorNeedRepaint = false;
		}

		//render all opened popups (at the end cause the have to be above other infos)
		PWPopup.RenderAll(ref editorNeedRepaint);
    }

#endregion

//Manage to do something with this:

/*			if (e.type == EventType.Layout)
			{
				graph.ForeachAllNodes(n => n.BeginFrameUpdate(), true, true);

				if (graphNeedReload)
				{
					graphNeedReload = false;
					
					terrainMaterializer.DestroyAllChunks();

					//load another instance of the current graph to separate calls:
					if (terrainMaterializer.graph != null && terrainMaterializer.graph.GetHashCode() != graph.GetHashCode())
						DestroyImmediate(terrainMaterializer.graph);
					terrainMaterializer.InitGraph(CloneGraph(graph));

					Debug.Log("graph: " + graph.GetHashCode() + " , terrainMat: " + terrainMaterializer.graph.GetHashCode());
					//process the instance of the graph in our editor so we can see datas on chunk 0, 0, 0
					graph.realMode = false;
					graph.ForeachAllNodes(n => n.Updategraph(graph));
					graph.UpdateChunkPosition(Vector3.zero);

					if (graphNeedReloadOnce)
						graph.ProcessGraphOnce();
					graphNeedReloadOnce = false;

					graph.ProcessGraph();
				}
				//updateChunks will update and generate new chunks if needed.
				//TODOMAYBE: remove this when workers will be added to the Terrain.
				terrainMaterializer.UpdateChunks();
			}*/

//TODO: eventize
	void OnWindowResize()
	{
		//calcul the ratio for the window move:
		float r = position.size.x / windowSize.x;

		h1.handlerPosition *= r;
		h2.handlerPosition *= r;
	}

	void LoadStyles()
	{
	}

	void LoadAssets()
	{
		
		Func< string, Texture2D > CreateTexture2DFromFile = (string ressourcePath) => {
			return Resources.Load< Texture2D >(ressourcePath);
        };
		

		//load style: to move
		resizeHandleColor = EditorGUIUtility.isProSkin
			? new Color32(56, 56, 56, 255)
            : new Color32(130, 130, 130, 255);

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
		eyeIconTexture = CreateTexture2DFromFile("ic_eye");
	}

}