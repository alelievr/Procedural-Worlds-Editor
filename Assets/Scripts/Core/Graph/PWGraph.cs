using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;
using UnityEngine;

using Debug = UnityEngine.Debug;
using OrderedNodeList = System.Linq.IOrderedEnumerable< PW.PWNode >;

namespace PW.Core
{
    public class PWGraph : ScriptableObject {
    
	#region Graph Datas

        //version infos:
		public int								majorVersion = 0;
		public int								minorVersion = 0;
        public string							creator = null;


        //asset datas:
        public string							assetFilePath;
        public new string               	    name;


        //public internal graph datas:
        public List< PWNode >					nodes = new List< PWNode >();
        public List< PWOrderingGroup >			orderingGroups = new List< PWOrderingGroup >();
		public int								seed;
        
		
		//protected internal graph datas:
		protected bool					    	realMode;
		[System.NonSerialized]
		protected IOrderedEnumerable< PWNode >	computeOrderSortedNodes = null;
		[System.NonSerialized]
		protected Dictionary< int, PWNode >		nodesDictionary = new Dictionary< int, PWNode >();


        //editor datas:
		public Vector2							panPosition;
		public int								localNodeIdCount;
        public PWGUIManager						PWGUI;


        //input and output nodes:
        public PWNodeGraphInput					inputNode;
        public PWNodeGraphOutput				outputNode;

		//GraphProcessor to Process and ProcessOnce the graph
		[System.NonSerialized]
		private PWGraphProcessor				graphProcessor = new PWGraphProcessor();


		//public delegates:
		public delegate void OnLinkAction(PWNodeConnection link);
		public delegate void OnNodeReloadRequest(PWNode from);


		//node events:
		public event System.Action				OnNodeRemoved;
		public event System.Action				OnNodeAdded;
		public event System.Action				OnNodeSelected;
		public event System.Action				OnNodeUnSelected;
		//link events:
		public event OnLinkAction				OnLinkDragged;
		public event OnLinkAction				OnNodeLinked;
		public event OnLinkAction				OnNodeUnLinked;
		public event OnLinkAction				OnLinkSelected;
		public event OnLinkAction				OnLinkUnSelected;
		//parameter events:
		public event System.Action				OnSeedChanged;
		//graph events:
		public event System.Action				OnGraphStructureChanged;
		public event OnNodeReloadRequest		OnNodeReloadRequested;
	
	#endregion

		public virtual void OnEnable()
		{
			Debug.Log("OnEnable graph !");

			graphProcessor.Initialize();

			//add all existing nodes to the nodesDictionary
			for (int i = 0; i < nodes.Count; i++)
				nodesDictionary[nodes[i].nodeId] = nodes[i];
			if (inputNode != null)
				nodesDictionary[inputNode.nodeId] = inputNode;
			if (outputNode != null)
				nodesDictionary[outputNode.nodeId] = outputNode;
			
			//Events attach
			OnGraphStructureChanged += GraphStructureChangedCallback;
			OnNodeLinked += LinkChangedCallback;
			OnNodeUnLinked += LinkChangedCallback;
			OnNodeRemoved += NodeCountChangedCallback;
			OnNodeAdded += NodeCountChangedCallback;
		}

		public virtual void OnDisable()
		{
			//Events detach
			OnGraphStructureChanged -= GraphStructureChangedCallback;
			OnNodeLinked -= LinkChangedCallback;
			OnNodeUnLinked -= LinkChangedCallback;
			OnNodeRemoved -= NodeCountChangedCallback;
			OnNodeAdded -= NodeCountChangedCallback;
		}

		public float Process()
		{
			graphProcessor.UpdateNodeDictionary(nodesDictionary);
			return graphProcessor.Process(this);
		}

		public void	ProcessOnce()
		{
			Debug.LogWarning("Process once called !");
			graphProcessor.UpdateNodeDictionary(nodesDictionary);
			graphProcessor.ProcessOnce(this);
		}

		public void Export(string filePath)
		{

		}

		public void Import(string filePath, bool wipeDatas = false)
		{

		}

		public bool Execute(string command)
		{
			return false;
		}

		public bool SetInput(string fieldName, object value)
		{
			return false;
		}
	
		//Dictionary< nodeId, dependencyWeight >
		Dictionary< int, int > nodeComputeOrderCount = new Dictionary< int, int >();
		int EvaluateComputeOrder(bool first = true, int depth = 0, int nodeId = -1)
		{
			//Recursively evaluate compute order for each nodes:
			if (first)
			{
				nodeComputeOrderCount.Clear();
				inputNode.computeOrder = 0;
	
				foreach (var gNode in nodes)
					gNode.computeOrder = EvaluateComputeOrder(false, 1, gNode.nodeId);
	
				outputNode.computeOrder = EvaluateComputeOrder(false, 1, outputNode.nodeId);
	
				return 0;
			}
	
			//check if we the node have already been computed:
			if (nodeComputeOrderCount.ContainsKey(nodeId))
				return nodeComputeOrderCount[nodeId];
	
			var node = FindNodeById(nodeId);
			if (node == null)
			{
				Debug.LogWarning("[PW ComputeOrder] node (" + nodeId + ") not found ");
				return 0;
			}
	
			//check if the window have all these inputs to work:
			if (!node.CheckRequiredAnchorLink())
				return -1;
	
			//compute dependency weight:
			int	ret = 1;
			foreach (var dep in node.GetDependencies())
			{
				int d = EvaluateComputeOrder(false, depth + 1, dep.nodeId);
	
				//if dependency does not have enought datas to compute result, abort calculus.
				if (d == -1)
				{
					ret = -1;
					break ;
				}
				ret += d;
			}
	
			nodeComputeOrderCount[nodeId] = ret;
			return ret;
		}

		public void	UpdateComputeOrder()
		{
			EvaluateComputeOrder();

			computeOrderSortedNodes = nodesDictionary
					//select all nodes building an object with node value and graph name (if needed)
					.Select(kp => kp.Value)
					//sort the resulting list by computeOrder:
					.OrderBy(n => n.computeOrder);
		}

		public void NodeReloadRequest(PWNode from, System.Type target)
		{

		}

		public void NodeReloadRequest(PWNode from)
		{

		}
		
		public void NodeReloadRequest(PWNode from, PWNode to)
		{

		}
	
	#region Events handlers

		//retarget link and node events to GraphStructure event
		void		LinkChangedCallback(PWNodeConnection link) { OnGraphStructureChanged(); }
		void		NodeCountChangedCallback() { OnGraphStructureChanged(); }

		void		GraphStructureChangedCallback()
		{
			UpdateComputeOrder();
		}

	#endregion

	#region Nodes control API

		public bool		IsRealMode()
		{
			return realMode;
		}

		public PWNode	FindNodeById(int nodeId)
		{
			return nodesDictionary[nodeId];
		}

		public IOrderedEnumerable< PWNode >	GetComputeSortedNodes()
		{
			return computeOrderSortedNodes;
		}

		public bool		AddNode(PWNode newNode, bool force = false)
		{
			if (!force && nodesDictionary.ContainsKey(newNode.id))
				return false;
			nodesDictionary[newNode.id] = newNode;
			OnNodeAdded();
			return true;
		}

		public bool		RemoveNode(PWNode removeNode)
		{
			var item = nodesDictionary.First(kvp => kvp.Value == removeNode);
			nodes.Remove(removeNode);
			OnNodeRemoved();
			return nodesDictionary.Remove(item.Key);
		}
		
		public bool		RemoveNode(int nodeId)
		{
			OnNodeRemoved();
			return nodesDictionary.Remove(nodeId);
		}

	#endregion

    }
}