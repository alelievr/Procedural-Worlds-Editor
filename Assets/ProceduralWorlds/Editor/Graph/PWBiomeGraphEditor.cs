using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;
using PW.Core;
using PW.Editor;
using PW.Biomator;

public class PWBiomeGraphEditor : PWGraphEditor
{
	
	[SerializeField]
	PWGraphLayout		layout = new PWGraphLayout();

	[MenuItem("Procedural Worlds/Biome graph")]
	static void Init()
	{
		PWBiomeGraphEditor window = (PWBiomeGraphEditor)GetWindow(typeof(PWBiomeGraphEditor));
		window.Show();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		
		OnWindowResize += WindowResizeCallback;
		OnGraphChanged += GraphLoadedCallback;

		layout.LoadStyles(position);
		
		layout.onDrawNodeSelector = (rect) => nodeSelectorBar.DrawNodeSelector(rect);
		layout.onDrawOptionBar = (rect) => optionBar.DrawOptionBar(rect);
		layout.onDrawSettingsBar = (rect) => settingsBar.DrawSettingsBar(rect);
	}

	void GraphLoadedCallback(PWGraph graph)
	{
		if (graph == null)
			return ;
		
		settingsBar.onDrawAdditionalSettings = DrawBiomeSettingsBar;
	}

	void DrawBiomeSettingsBar(Rect rect)
	{
		biomeGraph.surfaceType = (BiomeSurfaceType)EditorGUILayout.EnumPopup("Biome surface type", biomeGraph.surfaceType);
	}

	public override void OnGUI()
	{
		//draw the node editor
		base.OnGUI();
		
		if (graph == null)
			return ;

		layout.Render2ResizablePanel(this, position);
	}
	
	void WindowResizeCallback(Vector2 newSize)
	{
		layout.ResizeWindow(newSize, position);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		
		OnWindowResize -= WindowResizeCallback;
		OnGraphChanged -= GraphLoadedCallback;
	}

}
