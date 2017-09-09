using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using PW;
using PW.Core;
using PW.Node;

using Debug = UnityEngine.Debug;

[System.Serializable]
public partial class PWGraphEditor : EditorWindow {

	//the reference to the graph;
	public PWGraph				graph;

	//event masks, zones where the graph will not process events,
	//useful when you want to add a panel on the top of the graph.
	public List< Rect >			eventMasks = new List< Rect >();

	//event masking
	EventType					savedEventType;
	bool						restoreEvent;

	//storage class to gather events for a further use.
	PWGraphEventInfo			eventInfos = new PWGraphEventInfo();
	
	//editor textures:
	Texture2D					nodeEditorBackgroundTexture;

	//editor styles:
	GUIStyle					prefixLabelStyle;
	GUIStyle					defaultNodeWinow;
	GUIStyle					defaultNodeWinowSelected;
	
	//editor skin:
	public GUISkin				PWGUISkin;
	
	//current Event:
	Event e;

	public virtual void OnEnable()
	{
		Debug.Log("OnEnable graph editor");
		LoadCustomStyles();
		LoadCustomAssets();
	}

	public virtual void OnDisable()
	{
		
	}

	//draw the default node graph:
	public virtual void OnGUI()
	{
		e = Event.current;

		//render the graph in the background:
		GUI.depth = -10;

		eventInfos.Reset();

		//disable events if mouse is above an eventMask Rect.
		MaskEvents();

		//draw the background:
		RenderBackground();

		//manage selection:
		SelectAndDrag();

		//graph rendering
		
		EditorGUILayout.BeginHorizontal(); //is it useful ?
		{
			RenderOrderingGroups();
			RenderNodes();
			RenderLinks();
		}
		EditorGUILayout.EndHorizontal();

		ContextMenu();

		//fill and process remaining events if there is
		ManageEvents();

		//restore masked events:
		UnMaskEvents();

		//reset to default the depth
		GUI.depth = 0;
	}

	void MaskEvents()
	{
		restoreEvent = false;
		savedEventType = e.type;
		
		//check if we have an event outside of the graph event masks
		if (e.isMouse || e.isKey || e.isScrollWheel)
		{
			foreach (var eventMask in eventMasks)
				if (eventMask.Contains(e.mousePosition))
				{
					//if there is, we say to ignore the event and restore it later
					restoreEvent = true;
					e.type = EventType.Ignore;
				}
		}
	}

	void RenderBackground()
	{
		float	scale = 2f;
		
		GUI.DrawTextureWithTexCoords(
			new Rect(graph.panPosition.x % 128 - 128, graph.panPosition.y % 128 - 128, maxSize.x, maxSize.y),
			nodeEditorBackgroundTexture, new Rect(0, 0, (maxSize.x / nodeEditorBackgroundTexture.width) * scale,
			(maxSize.y / nodeEditorBackgroundTexture.height) * scale)
		);
	}

	void SelectAndDrag()
	{
		//rendering the selection rect
		if (e.type == EventType.mouseDrag && e.button == 0 && selecting)
			selectionRect.size = e.mousePosition - selectionRect.position;
		if (selecting)
		{
			Rect posiviteSelectionRect = PWUtils.CreateRect(selectionRect.min, selectionRect.max);
			Rect decaledSelectionRect = PWUtils.DecalRect(posiviteSelectionRect, -graph.panPosition);
			GUI.Label(selectionRect, "", selectionStyle);
			graph.ForeachAllNodes(n => n.selected = decaledSelectionRect.Overlaps(n.windowRect), false, true);
		}

		//multiple window drag:
		if (draggingSelectedNodes)
		{
				graph.ForeachAllNodes(n => {
				if (n.selected)
					n.windowRect.position += e.mousePosition - lastMousePosition;
				}, false, true);
		}
	}

	void RenderOrderingGroups()
	{
		mouseAboveOrderingGroup = null;
		foreach (var orderingGroup in graph.orderingGroups)
		{
			if (orderingGroup.Render(graph.panPosition, position.size))
				mouseAboveOrderingGroup = orderingGroup;
		}
	}

	void RenderNodes()
	{
		int		nodeId = 0;
		
		BeginWindows();
		{
			foreach (var node in graph.nodes)
			{
				string nodeName = (string.IsNullOrEmpty(node.externalName)) ? node.nodeTypeName : node.externalName;
				RenderNode(nodeId++, node, nodeName);
			}
	
			//display the graph input and output:
			RenderNode(nodeId++, graph.outputNode as PWNode, "output");
	
			if (graph.inputNode != null)
				RenderNode(nodeId++, graph.outputNode as PWNode, "input");
		}
		EndWindows();
	}

	void RenderLinks()
	{

	}

	void ManageEvents()
	{
		//we save with the s key
		if (e.type == EventType.KeyDown && e.keyCode == KeyCode.S)
		{
			e.Use();
			AssetDatabase.SaveAssets();
		}
		
		//click up outside of an anchor, stop dragging
		if (e.type == EventType.mouseUp && draggingLink)
			StopDragLink(false);
			
		//duplicate selected items if cmd+d:
		if (e.command && e.keyCode == KeyCode.D && e.type == EventType.KeyDown)
		{
			//duplicate the selected nodes
			var dupnList = new List< PWNode >();
			foreach (var node in graph.nodes)
			{
				if (node.selected)
					dupnList.Add(Instantiate(node));
				node.selected = false;
			}

			foreach (var toAdd in dupnList)
			{
				CreateNewNode(toAdd, toAdd.windowRect.position + new Vector2(40, 40), toAdd.name, true);
				toAdd.nodeId = graph.localNodeIdCount++;
				toAdd.DeleteAllLinks(false);
				toAdd.selected = true;
			}

			e.Use();
		}
	}

	void UnMaskEvents()
	{
		if (restoreEvent)
			e.type = savedEventType;
	}

	void DrawNodeGraphCore()
	{
		Event		e = Event.current;

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

		//draw the dragging link
		if (draggingLink)
			DrawNodeCurve(
				new Rect((int)startDragAnchor.anchorRect.center.x, (int)startDragAnchor.anchorRect.center.y, 0, 0),
				snappedToAnchorMouseRect,
				-1,
				null
			);
		
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
		bool	biomeReload = false;
		PWNode	reloadRequestedNode = null;
		int		reloadWeight = 0;
		graph.ForeachAllNodes(p => {
			if (e.type == EventType.Layout)
			{
				if (p.notifyDataChanged || p.notifyBiomeDataChanged)
				{
					biomeReload = p.notifyBiomeDataChanged;
					graphNeedReload = true;
					p.notifyDataChanged = false;
					p.notifyBiomeDataChanged = false;
					reloadRequested = true;
					reloadWeight = p.computeOrder;
					reloadRequestedNode = p;
				}
			}
		}, true, true);

		if (reloadRequested)
		{
			graph.ForeachAllNodes(n => {
				if (n.computeOrder >= reloadWeight)
				{
					if (biomeReload)
						n.biomeReloadRequested = true;
					else
						n.reloadRequested = true;
					n.SetReloadReuqestedNode(reloadRequestedNode);
				}
			}, true, true);
		}
	}

#region Draw utils functions and Ressource generation

	static void LoadCustomAssets()
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

#endregion
}
