using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;

//Ordering group rendering for PWGraphEditor
public partial class PWGraphEditor {
    
	void CreateNewOrderingGroup(object pos)
	{
	    graph.orderingGroups.Add(new PWOrderingGroup((Vector2)pos));
	}

	void DeleteOrderingGroup()
	{
		if (eventInfos.mouseOverOrderingGroup != null)
			graph.orderingGroups.Remove(eventInfos.mouseOverOrderingGroup);
	}

}
