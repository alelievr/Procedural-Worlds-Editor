using UnityEngine;
using UnityEditor;

[System.SerializableAttribute]
public class HorizontalSplitView {

	[SerializeField]
	public float	handlerPosition;
	[SerializeField]
	int				internHandlerPosition;
	[SerializeField]
	bool			resize = false;
	[SerializeField]
	Rect			availableRect;
	[SerializeField]
	float			minWidth;
	[SerializeField]
	float			maxWidth;
	[SerializeField]
	float			lastMouseX = -1;

	[SerializeField]
	Rect			savedBeginRect;

	[SerializeField]
	int				handleWidth = 4;

	public HorizontalSplitView(Texture2D handleTex, float hP, float min, float max)
	{
		handlerPosition = hP;
		minWidth = min;
		maxWidth = max;
	}

	public Rect Begin(Texture2D background = null)
	{
		Rect tmpRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		
		internHandlerPosition = (int)handlerPosition;
		if (tmpRect.width > 0f)
			availableRect = tmpRect;

		Rect beginRect = EditorGUILayout.BeginVertical(GUILayout.Width(internHandlerPosition), GUILayout.ExpandHeight(true));
		if (beginRect.width > 2)
			savedBeginRect = beginRect;
		return savedBeginRect;
	}

	public Rect Split(Texture2D resizeHandleTex = null)
	{
		EditorGUILayout.EndVertical();
		
		//left bar separation and resize:
		
		Rect handleRect = new Rect(internHandlerPosition - 1, availableRect.y, handleWidth, availableRect.height);
		Rect handleCatchRect = new Rect(internHandlerPosition - 1, availableRect.y, 6f, availableRect.height);
		GUI.DrawTexture(handleRect, resizeHandleTex);
		EditorGUIUtility.AddCursorRect(handleCatchRect, MouseCursor.ResizeHorizontal);

		if (Event.current.type == EventType.mouseDown && handleCatchRect.Contains(Event.current.mousePosition))
			resize = true;
		if (lastMouseX != -1 && resize)
			handlerPosition += Event.current.mousePosition.x - lastMouseX;
		if (Event.current.type == EventType.MouseUp)
			resize = false;
		lastMouseX = Event.current.mousePosition.x;
		internHandlerPosition = (int)Mathf.Clamp(handlerPosition, minWidth, maxWidth);
		handlerPosition = Mathf.Clamp(handlerPosition, minWidth, maxWidth);

		if (resize && Event.current.isMouse)
			Event.current.Use();

		return new Rect(internHandlerPosition + 3, availableRect.y, availableRect.width - internHandlerPosition, availableRect.height);
	}

	public void UpdateMinMax(float min, float max)
	{
		minWidth = min;
		maxWidth = max;
	}

	public void End()
	{
		EditorGUILayout.EndHorizontal();
	}
}
