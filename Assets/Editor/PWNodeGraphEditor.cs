using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;

[CustomEditor(typeof(PWNodeGraph))]
public class PWNodeGraphEditor : Editor {

	public override bool HasPreviewGUI()
	{
		return true;
	}

	public override void OnPreviewGUI(Rect rect, GUIStyle background)
	{
		//TODO: chunk preview here
	}

	public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int textureWidth, int textureHeight)
	{
		//TODO: texture preview here
		return new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
	}

	public override void OnPreviewSettings()
	{
		
	}

	public override void OnInspectorGUI()
	{

	}
}