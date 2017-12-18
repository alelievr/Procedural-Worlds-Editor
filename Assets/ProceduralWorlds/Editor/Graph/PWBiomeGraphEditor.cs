using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;
using PW.Core;
using PW.Editor;

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

		layout.LoadStyles(position);
		
		layout.onDrawNodeSelector = (rect) => nodeSelectorBar.DrawNodeSelector(rect);
		layout.onDrawOptionBar = (rect) => optionBar.DrawOptionBar(rect);
		layout.onDrawSettingsBar = (rect) => settingsBar.DrawSettingsBar(rect);
	}

	public override void OnGUI()
	{
		//draw the node editor
		base.OnGUI();

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
	}

}
