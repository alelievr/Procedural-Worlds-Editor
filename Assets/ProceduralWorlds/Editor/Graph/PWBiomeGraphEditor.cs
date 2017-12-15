using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;
using PW.Core;

public class PWBiomeGraphEditor : PWGraphEditor
{

	[MenuItem("Procedural Worlds/Biome graph")]
	static void Init()
	{
		PWBiomeGraphEditor window = (PWBiomeGraphEditor)GetWindow(typeof(PWBiomeGraphEditor));
		window.Show();
	}

	public override void OnEnable()
	{
		base.OnEnable();
	}

	public override void OnGUI()
	{
		int selectorWidth = 250;
		Rect nodeSelectorRect = new Rect(position.width - selectorWidth, 0, selectorWidth, position.height);

		// eventMasks[0] = nodeSelectorRect;

		//draw the node editor
		base.OnGUI();

		//draw the node selector
		nodeSelectorBar.DrawNodeSelector(nodeSelectorRect);
	}

	public override void OnDisable()
	{
		base.OnDisable();
	}

}
