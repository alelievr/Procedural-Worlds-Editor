using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;

public class PWBiomeGraphEditor : PWGraphEditor {

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
		base.OnGUI();
	}

	public override void OnDisable()
	{
		base.OnDisable();
	}

}
