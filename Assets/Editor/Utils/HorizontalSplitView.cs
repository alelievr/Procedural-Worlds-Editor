using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HorizontalSplitView {

	Vector2		scrollPosition = Vector2.zero;
	float		handlerPosition;
	bool		resize = false;
	Rect		availableRect;
	float			minWidth;
	float		maxWidth;

	Texture2D	resizeHandleTex;

	int			handleWidth = 4;

	public HorizontalSplitView(Texture2D handleTex, float hP, float min, float max)
	{
		resizeHandleTex = handleTex;
		handlerPosition = hP;
		minWidth = min;
		maxWidth = max;
	}

	public Rect Begin(Texture2D background = null)
	{
		Rect tmpRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		
		if (tmpRect.width > 0f)
			availableRect = tmpRect;

		Rect splittedPanelRect = new Rect(0, 0, availableRect.width, availableRect.height);
		return EditorGUILayout.BeginVertical(GUILayout.Width(handlerPosition), GUILayout.ExpandHeight(true));
	}

	float lastMouseX = Event.current.mousePosition.x;
	public Rect Split(Texture2D background = null)
	{
		EditorGUILayout.EndVertical();
		
		//TODO: min width and background color.
		//left bar separation and resize:
		Rect handleRect = new Rect(handlerPosition, availableRect.y, handleWidth, availableRect.height);
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
		handlerPosition = Mathf.Clamp(handlerPosition, minWidth, maxWidth);

		return new Rect(handlerPosition + 4, availableRect.y, availableRect.width - handlerPosition, availableRect.height);
	}

	public void End()
	{
		EditorGUILayout.EndHorizontal();
	}
}
