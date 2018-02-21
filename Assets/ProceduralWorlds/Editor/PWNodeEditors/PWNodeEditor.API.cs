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
		public delegate void					ReloadAction(PWNode from);
		public delegate void					MessageReceivedAction(PWNode from, object message);
		
		//fired when the node received a NotifyReload() or the user pressed Reload button in editor.
		public event ReloadAction				OnReload;
		//fired just after OnReload event;
		public event ReloadAction				OnPostReload;
		//fired when the node receive a SendMessage()
		protected event MessageReceivedAction	OnMessageReceived;
		
		//default notify reload will be sent to all node childs
		//also fire a Process event for the target nodes
		public void NotifyReload()
		{
			var nodes = graphRef.GetNodeChildsRecursive(node);

			foreach (var n in nodes)
				n.Reload(node);
			
			//add our node to the process pass
			nodes.Add(node);

			graphRef.ProcessNodes(nodes);
		}
		
		//send reload event to all node of the specified type
		public void NotifyReload(Type targetType)
		{
			var nodes = graphRef.FindNodesByType(targetType);
			
			foreach (var n in nodes)
				n.Reload(node);
		}

		//send reload to all nodes with a computeOrder bigger than minComputeOrder
		public void NotifyReload(int minComputeOrder)
		{
			var nodes = from n in graphRef.nodes
						where n.computeOrder > minComputeOrder
						select n;

			foreach (var n in nodes)
				n.Reload(node);
		}

		public void NotifyReload(PWNode n)
		{
			n.Reload(node);
		}

		public void NotifyReload(IEnumerable< PWNode > nodes)
		{
			foreach (var n in nodes)
				n.Reload(node);
		}

		public void Reload(PWNode from)
		{
			if (node.isProcessing)
			{
				Debug.LogError("Tried to reload a node from a processing pass !");
				return ;
			}
			if (OnReload != null)
				OnReload(from);
			if (OnPostReload != null)
				OnPostReload(from);
		}

		public void SendMessage(PWNode target, object message)
		{
			target.OnMessageReceived(node, message);
		}
		
		public void SendMessage(Type targetType, object message)
		{
			var nodes = from i in graphRef.nodes
						where i.GetType() == targetType
						select i;
						
			foreach (var n in nodes)
				n.OnMessageReceived(node, message);
		}
		
		public void SendMessage(IEnumerable< PWNode > nodes, object message)
		{
			foreach (var n in nodes)
				n.OnMessageReceived(node, message);
		}

	}
}