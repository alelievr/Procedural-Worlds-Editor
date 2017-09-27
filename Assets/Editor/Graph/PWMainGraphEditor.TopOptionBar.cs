using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;
using PW.Core;

//Top option bar
public partial class PWMainGraphEditor {

	void DrawNodeGraphHeader(Rect graphRect)
	{
		Event	e = Event.current;
		EditorGUILayout.BeginVertical(navBarBackgroundStyle);
		{
			//Icon bar:
			EditorGUILayout.BeginHorizontal(navBarBackgroundStyle, GUILayout.MaxHeight(40), GUILayout.ExpandWidth(true));
			{
				//recenter the graph
				if (GUILayout.Button(rencenterIconTexture, GUILayout.Width(30), GUILayout.Height(30)))
					mainGraph.panPosition = Vector2.zero;
				//ping the current PW object in the project window
				if (GUILayout.Button(fileIconTexture, GUILayout.Width(30), GUILayout.Height(30)))
					EditorGUIUtility.PingObject(mainGraph);
				//Show every hidden object in your hierarchy
				if (GUILayout.Button(eyeIconTexture, GUILayout.Width(30), GUILayout.Height(30)))
				{
					var objs = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];

					foreach (var obj in objs)
					{
						if (obj.hideFlags == HideFlags.HideAndDontSave)
							obj.hideFlags = HideFlags.DontSave;
					}
				}
			}
			EditorGUILayout.EndHorizontal();
	
			//remove 4 pixels for the separation bar
			graphRect.size -= Vector2.right * 4;
	
			#if (DEBUG_GRAPH)
			foreach (var node in nodes)
				GUI.DrawTexture(PWUtils.DecalRect(node.rect, panPosition), debugTexture1);
			#endif
		}
		EditorGUILayout.EndVertical();
	}


}
