using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;
using ProceduralWorlds;
using System;

namespace ProceduralWorlds.Editor
{
	public partial class BaseGraphEditor
	{
		//editor textures:
		public Texture2D				nodeEditorBackgroundTexture;
		
		//editor skin:
		protected GUISkin				PWGUISkin;
	
		//color gradient used for compute time displayed under nodes
		private Gradient				greenRedGradient;
		
		protected GUIStyle				nodeGraphWidowStyle;
		
		protected GUIStyle				selectionStyle;
	
		protected GUIStyle				nodeStyle;
		protected GUIStyle				nodeHeaderStyle;
		protected GUIStyle				nodeSelectedStyle;
	
		[System.NonSerialized]
		protected bool					styleLoaded = false;
	
		void LoadAssets()
		{
			//load backgrounds and colors as texture
			nodeEditorBackgroundTexture = Resources.Load< Texture2D >("GUI/nodeEditorBackground");
			
			//style
			nodeGraphWidowStyle = new GUIStyle();
			nodeGraphWidowStyle.normal.background = ColorTheme.defaultBackgroundTexture;
	
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
			PWGUISkin = Resources.Load< GUISkin >("PWEditorSkin");
	
			selectionStyle = PWGUISkin.FindStyle("Selection");
	
			nodeStyle = PWGUISkin.FindStyle("Node");
			nodeSelectedStyle = PWGUISkin.FindStyle("NodeSelected");
			nodeHeaderStyle = PWGUISkin.FindStyle("NodeHeader");
			
			GUI.skin = PWGUISkin;
	
			styleLoaded = true;
		}
	
	}
}