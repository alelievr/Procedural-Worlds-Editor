using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;
using UnityEditorInternal;
using System.Linq;

public class BiomeSurfaceGraphWindow : EditorWindow
{

	readonly BiomeSurfaceGraph	biomeSurfaceGraph = new BiomeSurfaceGraph();

	readonly List< BiomeSurfaceSwitch >	surfaceSwitches = new List< BiomeSurfaceSwitch >();
	ReorderableList				switchList;
	
	[SerializeField]
	List< BiomeSurfaceGraph.BiomeSurfaceCell > cellMap = new List< BiomeSurfaceGraph.BiomeSurfaceCell >();
	[SerializeField]
	List< Rect >				windowMap = new List< Rect >();

	float						searchHeight;
	float						searchSlope;

	[MenuItem("Window/Procedural Worlds/Debug/BiomeSurfaceGraph", false, 10)]
	public static void Open()
	{
		var win = EditorWindow.GetWindow< BiomeSurfaceGraphWindow >();

		win.Show();
	}

	public void OnEnable()
	{
		switchList = new ReorderableList(surfaceSwitches, typeof(BiomeSurfaceSwitch));

		switchList.drawElementCallback = (rect, index, active, focused) => {
			var sSwitch = surfaceSwitches[index];
			Rect stateRect = rect;
			stateRect.height = EditorGUIUtility.singleLineHeight;
			stateRect.width = 100;
			float defaultX = stateRect.x;
			EditorGUIUtility.labelWidth = 50;
			{
				sSwitch.surface.name = EditorGUI.TextField(stateRect, sSwitch.surface.name);
				stateRect.x += 120;
				sSwitch.surface.color.baseColor = EditorGUI.ColorField(stateRect, sSwitch.surface.color.baseColor);
			}
			stateRect.y += EditorGUIUtility.singleLineHeight;
			stateRect.x = defaultX;
			{
				sSwitch.heightEnabled = EditorGUI.Toggle(stateRect, "Height", sSwitch.heightEnabled);
				if (sSwitch.heightEnabled)
				{
					stateRect.x += 120;
					sSwitch.minHeight = EditorGUI.FloatField(stateRect, "min", sSwitch.minHeight);
					stateRect.x += 120;
					sSwitch.maxHeight = EditorGUI.FloatField(stateRect, "max", sSwitch.maxHeight);
				}
			}
			stateRect.y += EditorGUIUtility.singleLineHeight;
			stateRect.x = defaultX;
			{
				sSwitch.slopeEnabled = EditorGUI.Toggle(stateRect, "Slope", sSwitch.slopeEnabled);
				if (sSwitch.slopeEnabled)
				{
					stateRect.x += 120;
					sSwitch.minSlope = EditorGUI.FloatField(stateRect, "min", sSwitch.minSlope);
					stateRect.x += 120;
					sSwitch.maxSlope = EditorGUI.FloatField(stateRect, "max", sSwitch.maxSlope);
				}
			}
			stateRect.y += EditorGUIUtility.singleLineHeight;
			stateRect.x = defaultX;
			{
				sSwitch.paramEnabled = EditorGUI.Toggle(stateRect, "Param", sSwitch.paramEnabled);
				if (sSwitch.paramEnabled)
				{
					stateRect.x += 120;
					sSwitch.minParam = EditorGUI.FloatField(stateRect, "min", sSwitch.minParam);
					stateRect.x += 120;
					sSwitch.maxParam = EditorGUI.FloatField(stateRect, "max", sSwitch.maxParam);
				}
			}
		};
		switchList.onAddCallback = (list) => {
			var newSwitch = new BiomeSurfaceSwitch();

			newSwitch.surface.color = new BiomeSurfaceColor();
			newSwitch.surface.color.baseColor = Random.ColorHSV();
			surfaceSwitches.Add(newSwitch);
		};
		switchList.elementHeight = EditorGUIUtility.singleLineHeight * 4;

		if (surfaceSwitches.Count > 0)
			biomeSurfaceGraph.BuildGraph(surfaceSwitches);
	}

	public void OnGUI()
	{
		bool mouseOverNode = false;

		if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
			if (windowMap.Any(w => w.Contains(Event.current.mousePosition)))
				mouseOverNode = true;

		if (!mouseOverNode)
		{
			switchList.DoLayoutList();
	
			if (GUILayout.Button("Build graph"))
			{
				bool built = biomeSurfaceGraph.BuildGraph(surfaceSwitches);
				Debug.Log("Graph valid: " + built);
			}
			
			EditorGUILayout.LabelField("Graph is built: " + biomeSurfaceGraph.isBuilt);

			DrawSearchInput();
		}

		int i = 0;

		if (biomeSurfaceGraph.rootCell != null)
		{
			BeginWindows();
			foreach (var cell in biomeSurfaceGraph.cells)
			{
				if (windowMap.Count <= i)
					windowMap.Add(new Rect(0, 0, 150, 120));
				if (cellMap.Count <= i)
					cellMap.Add(cell);
				
				windowMap[i] = GUI.Window(i, windowMap[i], WindowCallback, cell.surface.name);
				cellMap[i] = cell;

				DrawLinks(windowMap[i], cell.links);
				
				i++;
			}
			EndWindows();
		}
	}

	void DrawSearchInput()
	{
		EditorGUILayout.BeginVertical(new GUIStyle("box"));

		searchHeight = EditorGUILayout.FloatField("Height", searchHeight);
		searchSlope = EditorGUILayout.FloatField("Slope", searchSlope);

		if (GUILayout.Button("Search"))
		{
			var surf = biomeSurfaceGraph.GetSurface(searchHeight, searchSlope);

			if (surf == null)
				Debug.Log("Surface not found !");
			else
				Debug.Log("Surface: " + surf.name + ", color: " + surf.color.baseColor);
		}

		EditorGUILayout.EndVertical();
	}

	void DrawLinks(Rect fromRect, List< BiomeSurfaceGraph.BiomeSurfaceLink > links)
	{
		int i = 0;
		foreach (var link in links)
		{
			Color linkColor = Color.blue;
			linkColor.a = .5f;

			var toIndex = cellMap.IndexOf(link.toCell);

			if (toIndex == -1)
				continue ;

			Rect toRect = windowMap[toIndex];

			Vector3 start = fromRect.min + new Vector2(0, i);
			Vector3 end = toRect.max;
			Vector3 startDir = (start - end);
			Vector3 endDir = (end - start);

			float tanPower = Mathf.Log((start - end).magnitude / 1000) / 4;

			Handles.DrawBezier(start, end, start + startDir * tanPower, end + endDir * tanPower, linkColor, null, 5);

			i += 10;
		}
	}

	void WindowCallback(int id)
	{
		var cell = cellMap[id];
		var sSwitch = surfaceSwitches[id];
		Rect r = EditorGUILayout.GetControlRect(false);
		EditorGUIUtility.DrawColorSwatch(r, cell.surface.color.baseColor);
		GUI.DragWindow();

		EditorGUILayout.BeginVertical();
		{
			EditorGUILayout.LabelField("Height min: " + sSwitch.minHeight + ", max: " + sSwitch.maxHeight);
			EditorGUILayout.LabelField("Slope min: " + sSwitch.minSlope + ", max: " + sSwitch.maxSlope);
			EditorGUILayout.LabelField("Param min: " + sSwitch.minParam + ", max: " + sSwitch.maxParam);
			EditorGUILayout.LabelField("Weight: " + cell.weight);
		}
		EditorGUILayout.EndVertical();
	}
		
}
