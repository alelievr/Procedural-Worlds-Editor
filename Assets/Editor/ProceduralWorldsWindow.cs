using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;

public class ProceduralWorldsWindow : EditorWindow {

    private static Texture2D	backgroundTex;
	private static Texture2D	resizeHandleTex;

    List<Rect> windows = new List<Rect>();
    List<int> windowsToAttach = new List<int>();
    List<int> attachedWindows = new List<int>();
	
	static GUIStyle	whiteText;
	static GUIStyle	splittedPanel;

	static HorizontalSplitView	h1;
	static HorizontalSplitView	h2;

	Vector2	leftBarScrollPosition;
	Vector2	selectorScrollPosition;

	float		minWidth = 100;

	[MenuItem("Window/Procedural Worlds")]
	static void Init()
	{
		ProceduralWorldsWindow window = (ProceduralWorldsWindow)EditorWindow.GetWindow (typeof (ProceduralWorldsWindow));

		CreateBackgroundTexture();

		h1 = new HorizontalSplitView(resizeHandleTex, 100);
		h2 = new HorizontalSplitView(resizeHandleTex, 300); //TODO: winize.x - 100

		splittedPanel = new GUIStyle();
		splittedPanel.margin = new RectOffset(1, 0, 0, 0);

		window.Show();
	}

    void OnGUI()
    {
		//text colors:
		whiteText = new GUIStyle();
		whiteText.normal.textColor = Color.white;

        //background color:
		if (backgroundTex == null)
			Init();
		GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), backgroundTex, ScaleMode.StretchToFill);

		h1.Begin();
		h2.Begin();
		DrawLeftBar();
		h2.Split();
		DrawNodeGraph();
		h2.End();
		h1.Split();
		DrawLeftSelector();
		h1.End();

		Repaint();
    }

	void DrawLeftBar()
	{
		leftBarScrollPosition = EditorGUILayout.BeginScrollView(leftBarScrollPosition, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.LabelField("Procedural Worlds Editor", whiteText);
	
			//draw preview view.
	
			//draw infos / debug / global settings view
		}
		EditorGUILayout.EndScrollView();
	}

	string searchString = "";
	void DrawLeftSelector()
	{
		selectorScrollPosition = EditorGUILayout.BeginScrollView(selectorScrollPosition, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginVertical(splittedPanel);
			{
				EditorGUIUtility.labelWidth = 0;
				EditorGUIUtility.fieldWidth = 0;
				GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
				{
					searchString = GUILayout.TextField(searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
					if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
					{
						// Remove focus if cleared
						searchString = "";
						GUI.FocusControl(null);
					}
				}
				GUILayout.EndHorizontal();
				
				//TODO: PWNode selector for creation
				//TODO: left selector background color:
		
				EditorGUILayout.LabelField("list of components", whiteText);
				
				//TOTO: draw list of components
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndScrollView();
	}

	void DrawNodeGraph()
	{
		EditorGUILayout.BeginHorizontal();
		{
			//draw links (will be moved to PWNode.cs)
			if (windowsToAttach.Count == 2)
			{
				attachedWindows.Add(windowsToAttach[0]);
				attachedWindows.Add(windowsToAttach[1]);
				windowsToAttach = new List<int>();
			}
			if (attachedWindows.Count >= 2)
				for (int i = 0; i < attachedWindows.Count; i += 2)
					DrawNodeCurve(windows[attachedWindows[i]], windows[attachedWindows[i + 1]]);
	
			//Window render (will also be moved to PWNode)
			BeginWindows();
			if (GUILayout.Button("Create Node"))
				windows.Add(new Rect(10, 10, 100, 100));
	
			for (int i = 0; i < windows.Count; i++)
				windows[i] = GUI.Window(i, windows[i], DrawNodeWindow, "Window " + i);
			EndWindows();
		}
		EditorGUILayout.EndHorizontal();
	}

	static void CreateBackgroundTexture()
	{
        Color backgroundColor = EditorGUIUtility.isProSkin
			? new Color32(56, 56, 56, 255)
			: new Color32(56, 56, 56, 255);
		backgroundTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		backgroundTex.SetPixel(0, 0, backgroundColor);
		backgroundTex.Apply();
		Color resizeHandleColor = EditorGUIUtility.isProSkin
			? new Color32(56, 56, 56, 255)
            : new Color32(194, 194, 194, 255);
		resizeHandleTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		resizeHandleTex.SetPixel(0, 0, resizeHandleColor);
		resizeHandleTex.Apply();
	}

    void DrawNodeCurve(Rect start, Rect end)
    {
        Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
        Color shadowCol = new Color(0, 0, 1f, 0.06f);

        for (int i = 0; i < 3; i++)
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);

        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
    }

    void DrawNodeWindow(int id)
    {
        if (GUILayout.Button("Attach"))
            windowsToAttach.Add(id);

        GUI.DragWindow();
    }
}