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
	
	bool		leftBarResize = false;
	float		splitNormalizedPosition = .5f;
	Rect		availableRect;
	GUIStyle	whiteText;

	[MenuItem("Window/Procedural Worlds")]
	static void Init()
	{
		ProceduralWorldsWindow window = (ProceduralWorldsWindow)EditorWindow.GetWindow (typeof (ProceduralWorldsWindow));

		CreateBackgroundTexture();

		window.Show();
	}

	Vector2 leftBarScrollPos = Vector2.zero;
    void OnGUI()
    {
		//text colors:
		whiteText = new GUIStyle();
		whiteText.normal.textColor = Color.white;

        //background color:
		if (backgroundTex == null)
			CreateBackgroundTexture();
       GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), backgroundTex, ScaleMode.StretchToFill);

		GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		{
			DrawLeftBar();
			DrawLeftSelector();
	
			DrawNodeGraph();
		}
		GUILayout.EndHorizontal();

		if (leftBarResize)
			Repaint();
    }

	void DrawLeftBar()
	{
		Rect tmpRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
		{
			if (tmpRect.width > 0f)
				availableRect = tmpRect;

			Debug.Log(availableRect);
			//left bar separation and resize:
			Rect handleRect = new Rect(availableRect.width * splitNormalizedPosition, availableRect.y, 4f, availableRect.height);
			Rect handleCatchRect = new Rect(availableRect.width * splitNormalizedPosition - 2, availableRect.y, 8f, availableRect.height);
			GUI.DrawTexture(handleRect, resizeHandleTex);
			EditorGUIUtility.AddCursorRect(handleCatchRect, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.mouseDown && handleCatchRect.Contains(Event.current.mousePosition))
                leftBarResize = true;
            if (leftBarResize)
				splitNormalizedPosition = Event.current.mousePosition.x / availableRect.width;
            if (Event.current.type == EventType.MouseUp)
                leftBarResize = false;

            GUILayout.BeginScrollView(leftBarScrollPos, GUILayout.Width(availableRect.width * splitNormalizedPosition));
			{
				EditorGUILayout.LabelField("Procedural Worlds Editor", whiteText);
				
				//draw preview view.

				//draw infos / debug / global settings view
			}
			GUILayout.EndScrollView();
		}
		EditorGUILayout.EndVertical();
	}

	void DrawLeftSelector()
	{
		//TODO: PWNode selector for creation
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