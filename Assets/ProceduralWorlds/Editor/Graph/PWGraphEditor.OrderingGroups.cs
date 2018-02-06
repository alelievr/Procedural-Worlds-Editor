using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using UnityEditor;
using UnityEngine.Profiling;

//Ordering group rendering for PWGraphEditor
public partial class PWGraphEditor
{
    
	void CreateNewOrderingGroup(object pos)
	{
		PWOrderingGroup	newOrderingGroup = new PWOrderingGroup();

		newOrderingGroup.Initialize((Vector2)pos);
		
	    graph.orderingGroups.Add(newOrderingGroup);
	}

	void DeleteOrderingGroup()
	{
		if (editorEvents.mouseOverOrderingGroup != null)
			graph.orderingGroups.Remove(editorEvents.mouseOverOrderingGroup);
	}

	void RenderOrderingGroups()
	{
		Profiler.BeginSample("[PW] Render ordering groups");

		foreach (var orderingGroup in graph.orderingGroups)
			orderingGroup.Render(graph.panPosition, position.size * (1 / graph.scale), ref graph.editorEvents);

		//if the mouse was not over an ordering group this frame
		if (!editorEvents.isMouseOverOrderingGroupFrame)
			editorEvents.mouseOverOrderingGroup = null;

		Profiler.EndSample();
	}
}
