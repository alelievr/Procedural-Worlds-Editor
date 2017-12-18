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
public partial class PWMainGraphEditor : PWGraphEditor
{

	[SerializeField]
	PWMainGraph						mainGraph { get { return graph as PWMainGraph; } }
	
	[SerializeField]
	HorizontalSplitView				h1;
	[SerializeField]
	HorizontalSplitView				h2;
	
	//graph, node, anchors and links control and 
	bool				previewMouseDrag = false;
	[System.NonSerializedAttribute]
	PWNode				mouseAboveNode;
	
	//events fields
	Vector2				lastMousePosition;
	[System.NonSerializedAttribute]
	Vector2				currentMousePosition;

	//terrain materializer
	PWTerrainBase		terrainMaterializer;

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

		OnWindowResize += WindowResizeCallback;
	}

	public override void OnDisable()
	{
		base.OnDisable();

		OnWindowResize -= WindowResizeCallback;
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
		
		if (!mainGraph.presetChoosed)
		{
			DrawPresetPanel();
			return ;
		}
		
		if (terrainMaterializer == null)
		{
			GameObject gtm = GameObject.Find("PWPreviewTerrain");
			if (gtm != null)
				terrainMaterializer = gtm.GetComponent< PWTerrainBase >();
		}

		h1.UpdateMinMax(position.width / 2, position.width - 3);
		h2.UpdateMinMax(50, position.width / 2);

		h1.Begin();
		Rect firstPanel = h2.Begin();
		settingsBar.DrawSettingsBar(firstPanel);
		Rect g = h2.Split(resizeHandleColor);
		optionBar.DrawOptionBar(g);
		Rect optionBarRect = GUILayoutUtility.GetLastRect();
		h2.End();
		Rect secondPanel = h1.Split(resizeHandleColor);
		nodeSelectorBar.DrawNodeSelector(secondPanel);
		h1.End();
		
		//add the handleWidth to the panel for event mask + 2 pixel for UX
		firstPanel.width += h1.handleWidth + 2;
		secondPanel.xMin -= h2.handleWidth + 2;
		//update event masks with our GUI parts
		eventMasks[0] = firstPanel;
		eventMasks[2] = secondPanel;
		
		//render all opened popups (at the end cause the have to be above other infos)
		//TODO: the new popup system
		// PWPopup.RenderAll(ref editorNeedRepaint);
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

	void WindowResizeCallback(Vector2 newSize)
	{
		//calcul the ratio for the window move:
		float r = position.size.x / windowSize.x;

		h1.handlePosition *= r;
		h2.handlePosition *= r;
	}

	void LoadStyles()
	{
	}

	void LoadAssets()
	{
		h1 = new HorizontalSplitView(resizeHandleTexture, position.width * 0.85f, position.width / 2, position.width - 4);
		h2 = new HorizontalSplitView(resizeHandleTexture, position.width * 0.25f, 4, position.width / 2);
		
		//load style: to move
		resizeHandleColor = EditorGUIUtility.isProSkin
			? new Color32(56, 56, 56, 255)
            : new Color32(130, 130, 130, 255);

		//loading preset panel images
		preset2DSideViewTexture = Resources.Load< Texture2D >("preview2DSideView");
		preset2DTopDownViewTexture = Resources.Load< Texture2D >("preview2DTopDownView");
		preset3DPlaneTexture = Resources.Load< Texture2D >("preview3DPlane");
		preset3DSphericalTexture = Resources.Load< Texture2D >("preview3DSpherical");
		preset3DCubicTexture = Resources.Load< Texture2D >("preview3DCubic");
		presetMeshTetxure = Resources.Load< Texture2D >("previewMesh");
		preset1DDensityFieldTexture= Resources.Load< Texture2D >("preview1DDensityField");
		preset2DDensityFieldTexture = Resources.Load< Texture2D >("preview2DDensityField");
		preset3DDensityFieldTexture = Resources.Load< Texture2D >("preview3DDensityField");
	}

}