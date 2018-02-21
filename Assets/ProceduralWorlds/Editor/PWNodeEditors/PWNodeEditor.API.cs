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

			foreach (var node in node.nodes)
				node.Reload(this);
			
			//add our node to the process pass
			nodes.Add(this);

			graphRef.ProcessNodes(nodes);
		}
		
		//send reload event to all node of the specified type
		public void NotifyReload(Type targetType)
		{
			var nodes = graphRef.FindNodesByType(targetType);
			
			foreach (var node in nodes)
				node.Reload(this);
		}

		//send reload to all nodes with a computeOrder bigger than minComputeOrder
		public void NotifyReload(int minComputeOrder)
		{
			var nodes = from node in graphRef.nodes
						where node.computeOrder > minComputeOrder
						select node;

			foreach (var node in nodes)
				node.Reload(this);
		}

		public void NotifyReload(PWNode node)
		{
			node.Reload(this);
		}

		public void NotifyReload(IEnumerable< PWNode > nodes)
		{
			foreach (var node in nodes)
				node.Reload(this);
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
			target.OnMessageReceived(this, message);
		}
		
		public void SendMessage(Type targetType, object message)
		{
			var nodes = from node in graphRef.nodes
						where node.GetType() == targetType
						select node;
						
			foreach (var node in nodes)
				node.OnMessageReceived(this, message);
		}
		
		public void SendMessage(IEnumerable< PWNode > nodes, object message)
		{
			foreach (var node in nodes)
				node.OnMessageReceived(this, message);
		}

	}
}