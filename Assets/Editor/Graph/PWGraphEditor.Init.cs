using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Node;
using System;

public partial class PWGraphEditor {

	//editor styles:
	GUIStyle					prefixLabelStyle;
	GUIStyle					defaultNodeWinow;
	GUIStyle					defaultNodeWinowSelected;
	
	//editor textures:
	static Texture2D			nodeEditorBackgroundTexture;
	private static Texture2D	defaultBackgroundTexture;
	
	//editor skin:
	public GUISkin				PWGUISkin;

	//color gradient used for compute time displayed under nodes
	private static Gradient		greenRedGradient;
	
	static GUIStyle		whiteText;
	static GUIStyle		whiteBoldText;
	static GUIStyle		navBarBackgroundStyle;
	static GUIStyle		panelBackgroundStyle;
	static GUIStyle		nodeGraphWidowStyle;

	public GUIStyle		toolbarSearchCancelButtonStyle;
	public GUIStyle		toolbarSearchTextStyle;
	public GUIStyle		toolbarStyle;
	public GUIStyle		nodeSelectorTitleStyle;
	public GUIStyle		nodeSelectorCaseStyle;
	public GUIStyle		selectionStyle;

	public GUIStyle		nodeWindow;
	public GUIStyle		nodeWindowSelected;

	static void LoadAssets()
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
		defaultBackgroundTexture = CreateTexture2DColor(defaultBackgroundColor);
		nodeEditorBackgroundTexture = CreateTexture2DFromFile("nodeEditorBackground");
		
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

	void LoadStyles()
	{
		PWGUISkin = Resources.Load("PWEditorSkin") as GUISkin;

		//initialize if null
		toolbarStyle = new GUIStyle("Toolbar");
		toolbarSearchTextStyle = new GUIStyle("ToolbarSeachTextField");
		toolbarSearchCancelButtonStyle = new GUIStyle("ToolbarSeachCancelButton");

		nodeSelectorTitleStyle = PWGUISkin.FindStyle("NodeSelectorTitle");
		nodeSelectorCaseStyle = PWGUISkin.FindStyle("NodeSelectorCase");

		selectionStyle = PWGUISkin.FindStyle("Selection");

		navBarBackgroundStyle = PWGUISkin.FindStyle("NavBarBackground");
		panelBackgroundStyle = PWGUISkin.FindStyle("PanelBackground");

		prefixLabelStyle = PWGUISkin.FindStyle("PrefixLabel");

		nodeWindow = PWGUISkin.FindStyle("NodeWindow");
		nodeWindowSelected = PWGUISkin.FindStyle("NodeWindowSelected");
		
		//set the custom style for the editor
		GUI.skin = PWGUISkin;
	}

}
