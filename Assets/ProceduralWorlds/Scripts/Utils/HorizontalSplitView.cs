using UnityEngine;
using UnityEditor;

[System.SerializableAttribute]
public class HorizontalSplitView
{

	[SerializeField]
	public float	handlePosition;
	[SerializeField]
	public int		handleWidth = 4;
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
	Rect			savedRect;

	[System.NonSerialized]
	bool			first = true;

	Event			e { get { return Event.current; } }

	public HorizontalSplitView(Texture2D handleTex, float hP, float min, float max)
	{
		handlePosition = hP;
		minWidth = min;
		maxWidth = max;
	}

	public Rect Begin()
	{
		Rect tmpRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		
		internHandlerPosition = (int)handlePosition;

		//if we are in the first frame, provide the true width value for layout (the only information we have currently)
		if (first)
		{
			availableRect = new Rect(0, 0, internHandlerPosition, 0);
			savedRect = new Rect(0, 0, internHandlerPosition, 0);
		}

		if (e.type == EventType.Repaint)
			availableRect = tmpRect;

		Rect beginRect = EditorGUILayout.BeginVertical(GUILayout.Width(internHandlerPosition), GUILayout.ExpandHeight(true));
		if (e.type == EventType.Repaint)
			savedRect = beginRect;

		first = false;

		return savedRect;
	}

	public Rect Split(Color resizeColor)
	{
		EditorGUILayout.EndVertical();
		
		//left bar separation and resize:
		Rect handleRect = new Rect(internHandlerPosition - 1, availableRect.y, handleWidth, availableRect.height);
		Rect handleCatchRect = new Rect(internHandlerPosition - 1, availableRect.y, 6f, availableRect.height);
		EditorGUI.DrawRect(handleRect, resizeColor);
		EditorGUIUtility.AddCursorRect(handleCatchRect, MouseCursor.ResizeHorizontal);

		if (Event.current.type == EventType.mouseDown && handleCatchRect.Contains(Event.current.mousePosition))
			resize = true;
		if (lastMouseX != -1 && resize)
			handlePosition += Event.current.mousePosition.x - lastMouseX;
		if (Event.current.rawType == EventType.MouseUp)
			resize = false;
		lastMouseX = Event.current.mousePosition.x;
		internHandlerPosition = (int)Mathf.Clamp(handlePosition, minWidth, maxWidth);
		handlePosition = Mathf.Clamp(handlePosition, minWidth, maxWidth);

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
