using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HorizontalSplitView {

	Vector2		scrollPosition = Vector2.zero;
	float		handlerPosition = .5f;
	bool		resize = false;
	Rect		availableRect;

	Texture2D	resizeHandleTex;

	public HorizontalSplitView(Texture2D handleTex)
	{
		resizeHandleTex = handleTex;
	}

	public void Begin()
	{
		Rect tmpRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		
		if (tmpRect.width > 0f)
			availableRect = tmpRect;

		Rect splittedPanelRect = new Rect(0, 0, availableRect.width, availableRect.height);
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(availableRect.width * handlerPosition));
	}

	public void Split()
	{
		GUILayout.EndScrollView();
		
		//left bar separation and resize:
		Rect handleRect = new Rect(availableRect.width * handlerPosition, availableRect.y, 4f, availableRect.height);
		Rect handleCatchRect = new Rect(availableRect.width * handlerPosition - 2, availableRect.y, 8f, availableRect.height);
		GUI.DrawTexture(handleRect, resizeHandleTex);
		EditorGUIUtility.AddCursorRect(handleCatchRect, MouseCursor.ResizeHorizontal);

		if (Event.current.type == EventType.mouseDown && handleCatchRect.Contains(Event.current.mousePosition))
			resize = true;
		if (resize)
			handlerPosition = Event.current.mousePosition.x / availableRect.width;
		if (Event.current.type == EventType.MouseUp)
			resize = false;
	}

	public void End()
	{
		EditorGUILayout.EndHorizontal();
	}
}
