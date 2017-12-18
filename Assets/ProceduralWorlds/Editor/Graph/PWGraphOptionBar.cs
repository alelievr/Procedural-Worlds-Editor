using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using UnityEditor;

namespace PW.Editor
{
	public class PWGraphOptionBar
	{
		//graph
		PWGraph				graph;


		//option bar styles:
		GUIStyle			navBarBackgroundStyle;
		
		static Texture2D	rencenterIconTexture;
		static Texture2D	fileIconTexture;
		static Texture2D	eyeIconTexture;

		public PWGraphOptionBar(PWGraph graph)
		{
			this.graph = graph;
		}

		public void LoadStyles()
		{
			rencenterIconTexture = Resources.Load< Texture2D >("ic_recenter");
			fileIconTexture = Resources.Load< Texture2D >("ic_file");
			eyeIconTexture = Resources.Load< Texture2D >("ic_eye");
			
			navBarBackgroundStyle = new GUIStyle("NavBarBackground");
		}
		
		public void DrawOptionBar(Rect graphRect)
		{
			Event	e = Event.current;
			EditorGUILayout.BeginVertical(navBarBackgroundStyle);
			{
				//Icon bar:
				EditorGUILayout.BeginHorizontal(navBarBackgroundStyle, GUILayout.MaxHeight(40), GUILayout.ExpandWidth(true));
				{
					//recenter the graph
					if (GUILayout.Button(rencenterIconTexture, GUILayout.Width(30), GUILayout.Height(30)))
						graph.panPosition = Vector2.zero;
					
					//ping the current PW object in the project window
					if (GUILayout.Button(fileIconTexture, GUILayout.Width(30), GUILayout.Height(30)))
						EditorGUIUtility.PingObject(graph);
				}
				EditorGUILayout.EndHorizontal();
		
				//remove 4 pixels for the separation bar
				graphRect.size -= Vector2.right * 4;
			}
			EditorGUILayout.EndVertical();
		}

	}
}