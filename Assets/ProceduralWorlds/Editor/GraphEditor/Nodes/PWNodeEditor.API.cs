using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using PW;
using PW.Node;

namespace PW.Editor
{
	public abstract partial class PWNodeEditor
	{
		//default notify reload will be sent to all node childs
		//also fire a Process event for the target nodes
		public void NotifyReload()
		{
			var nodes = graphRef.GetNodeChildsRecursive(nodeRef);

			foreach (var node in nodes)
				openedNodeEdiors[node].OnNodePreProcess();
			
			//add our node to the process pass
			nodes.Add(nodeRef);

			graphRef.ProcessNodes(nodes);
			
			foreach (var editorKP in openedNodeEdiors)
				editorKP.Value.OnNodePostProcess();
		}

	}
}