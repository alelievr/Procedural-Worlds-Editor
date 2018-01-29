using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Node;
using PW;
using System;

public partial class PWGraphEditor
{

	//editor styles:
	protected static GUIStyle		defaultNodeWinow;
	protected static GUIStyle		defaultNodeWinowSelected;
	
	//editor textures:
	public static Texture2D			nodeEditorBackgroundTexture;
	
	//editor skin:
	protected GUISkin				PWGUISkin;

	//color gradient used for compute time displayed under nodes
	private static Gradient			greenRedGradient;
	
	protected static GUIStyle		whiteText; //TODO: replace by EditorStyles.whiteLabel ?
	protected static GUIStyle		whiteBoldText; //TODO: same ?
	protected static GUIStyle		nodeGraphWidowStyle;
	
	protected GUIStyle				selectionStyle;

	protected GUIStyle				nodeStyle;
	protected GUIStyle				nodeHeaderStyle;
	protected GUIStyle				nodeSelectedStyle;

	static void LoadAssets()
	{
		//load backgrounds and colors as texture
		nodeEditorBackgroundTexture = Resources.Load< Texture2D >("nodeEditorBackground");
		
		//style
		nodeGraphWidowStyle = new GUIStyle();
		nodeGraphWidowStyle.normal.background = PWColorTheme.defaultBackgroundTexture;

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

	void LoadStyles()
	{
		PWGUISkin = Resources.Load("PWEditorSkin") as GUISkin;

		selectionStyle = PWGUISkin.FindStyle("Selection");


		nodeStyle = PWGUISkin.FindStyle("Node");
		nodeSelectedStyle = PWGUISkin.FindStyle("NodeSelected");
		nodeHeaderStyle = PWGUISkin.FindStyle("NodeHeader");
		
		//TODO: still used ?
		whiteText = new GUIStyle();
		whiteText.normal.textColor = Color.white;
		whiteBoldText = new GUIStyle();
		whiteBoldText.fontStyle = FontStyle.Bold;
		whiteBoldText.normal.textColor = Color.white;
	}

}
