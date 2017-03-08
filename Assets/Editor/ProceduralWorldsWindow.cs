using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW;

public class ProceduralWorldsWindow : EditorWindow {

    private static Texture2D	backgroundTex;
	private static Texture2D	resizeHandleTex;

	List< PWNode >				nodes = new List< PWNode >();
    List<Rect>					windows = new List<Rect>();
    List<int>					windowsToAttach = new List<int>();
    List<int>					attachedWindows = new List<int>();
	
	static GUIStyle	whiteText;
	static GUIStyle	splittedPanel;

	static HorizontalSplitView	h1;
	static HorizontalSplitView	h2;

	[SerializeField]
	Vector2	leftBarScrollPosition;
	[SerializeField]
	Vector2	selectorScrollPosition;

	float		minWidth = 100;
	
	private class PWNodeStorage
	{
		public string		name;
		public System.Type	nodeInstace;
		
		public PWNodeStorage(string n, System.Type instance)
		{
			name = n;
			nodeInstace = instance;
		}
	}

	List< PWNodeStorage > nodeList = new List< PWNodeStorage >()
	{
		new PWNodeStorage("Slider", typeof(PWNodeSlider))
	};

	[MenuItem("Window/Procedural Worlds")]
	static void Init()
	{
		ProceduralWorldsWindow window = (ProceduralWorldsWindow)EditorWindow.GetWindow (typeof (ProceduralWorldsWindow));

		CreateBackgroundTexture();

		h1 = new HorizontalSplitView(resizeHandleTex, window.position.width - 250, window.position.width / 2, window.position.width - 4);
		h2 = new HorizontalSplitView(resizeHandleTex, 300, 0, window.position.width / 2);

		splittedPanel = new GUIStyle();
		splittedPanel.margin = new RectOffset(5, 0, 0, 0);

		window.Show();
	}

    void OnGUI()
    {
		//text colors:
		whiteText = new GUIStyle();
		whiteText.normal.textColor = Color.white;

        //background color:
		if (backgroundTex == null || h1 == null)
			Init();
		GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), backgroundTex, ScaleMode.StretchToFill);

		DrawNodeGraphCore();

		h1.Begin();
		Rect p1 = h2.Begin(backgroundTex);
		DrawLeftBar(p1);
		h2.Split();
		DrawNodeGraphHeader();
		h2.End();
		Rect p2 = h1.Split(backgroundTex);
		DrawLeftSelector(p2);
		h1.End();

		Repaint();
    }

	void DrawLeftBar(Rect currentRect)
	{
		GUI.DrawTexture(currentRect, backgroundTex);
		leftBarScrollPosition = EditorGUILayout.BeginScrollView(leftBarScrollPosition, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginVertical(splittedPanel);
			{
				EditorGUILayout.LabelField("Procedural Worlds Editor", whiteText);
		
				//draw preview view.
		
				//draw infos / debug / global settings view
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndScrollView();
	}

	string searchString = "";
	void DrawLeftSelector(Rect currentRect)
	{
		GUI.DrawTexture(currentRect, backgroundTex);
		selectorScrollPosition = EditorGUILayout.BeginScrollView(selectorScrollPosition, GUILayout.ExpandWidth(true));
		{
			EditorGUILayout.BeginVertical(splittedPanel);
			{
				//apply background color:
				GUI.DrawTexture(currentRect, backgroundTex);

				//TODO: dynamic search
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
				
				foreach (var node in nodeList)
				{
					PWNode n = (PWNode)System.Activator.CreateInstance(node.nodeInstace);

					Rect r = EditorGUILayout.GetControlRect();
					EditorGUI.LabelField(r, n.name, whiteText);

					r.height += 10;

					if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
					{
						nodes.Add(n);
						Debug.Log("added node of type: " + node.nodeInstace);
					}
				}
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndScrollView();
	}
	
	void DrawNodeGraphHeader()
	{
		EditorGUILayout.BeginVertical(splittedPanel);
		//if (GUILayout.Button("Create Node"))
		//	windows.Add(new Rect(position.center.x, position.center.y, 150, 400));
		EditorGUILayout.EndVertical();
	}

	string GetUniqueName(string name)
	{
		while (true)
		{
			if (!nodes.Any(p => p.name == name))
				return name;
			name += "*";
		}
	}

	void DrawNodeGraphCore()
	{
		EditorGUILayout.BeginHorizontal();
		{
			//draw links (will be moved to PWNode.cs)
			/*if (windowsToAttach.Count == 2)
			{
				attachedWindows.Add(windowsToAttach[0]);
				attachedWindows.Add(windowsToAttach[1]);
				windowsToAttach = new List<int>();
			}
			if (attachedWindows.Count >= 2)
				for (int i = 0; i < attachedWindows.Count; i += 2)
					DrawNodeCurve(windows[attachedWindows[i]], windows[attachedWindows[i + 1]]);*/

			BeginWindows();
			for (int i = 0; i < nodes.Count; i++)
				nodes[i].rect = GUI.Window(i, nodes[i].rect, nodes[i].OnGUI, nodes[i].name);
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
}