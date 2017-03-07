using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HorizontalSplitView {

	Vector2		scrollPosition = Vector2.zero;
	float		handlerPosition;
	bool		resize = false;
	Rect		availableRect;

	Texture2D	resizeHandleTex;

	public HorizontalSplitView(Texture2D handleTex, float hP)
	{
		resizeHandleTex = handleTex;
		handlerPosition = hP;
	}

	public void Begin()
	{
		Rect tmpRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		
		if (tmpRect.width > 0f)
			availableRect = tmpRect;

		Rect splittedPanelRect = new Rect(0, 0, availableRect.width, availableRect.height);
		GUILayout.BeginVertical(GUILayout.Width(handlerPosition), GUILayout.ExpandHeight(true));
	}

	float lastMouseX = Event.current.mousePosition.x;
	public void Split()
	{
		GUILayout.EndVertical();
		
		//TODO: min width and background color.
		//left bar separation and resize:
		Rect handleRect = new Rect(handlerPosition, availableRect.y, 4f, availableRect.height);
		Rect handleCatchRect = new Rect(handlerPosition, availableRect.y, 8f, availableRect.height);
		GUI.DrawTexture(handleRect, resizeHandleTex);
		EditorGUIUtility.AddCursorRect(handleCatchRect, MouseCursor.ResizeHorizontal);

		if (Event.current.type == EventType.mouseDown && handleCatchRect.Contains(Event.current.mousePosition))
			resize = true;
		if (resize)
			handlerPosition += Event.current.mousePosition.x - lastMouseX;
		if (Event.current.type == EventType.MouseUp)
			resize = false;
		lastMouseX = Event.current.mousePosition.x;
	}

	public void End()
	{
		EditorGUILayout.EndHorizontal();
	}
}
