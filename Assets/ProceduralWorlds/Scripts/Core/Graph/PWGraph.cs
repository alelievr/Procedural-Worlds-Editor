using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System;

using Debug = UnityEngine.Debug;
using OrderedNodeList = System.Linq.IOrderedEnumerable< PW.PWNode >;
using Object = UnityEngine.Object;

namespace PW.Core
{
	[System.Serializable]
    public abstract class PWGraph : ScriptableObject
	{
    
	#region Graph Datas

        //version infos:
		public int								majorVersion = 0;
		public int								minorVersion = 0;
        public string							creator = null;


        //asset datas:
        public string							assetFilePath;
		public string							PWFolderPath;
        public new string               	    name;
		public string							objectName { get { return base.name; } }


        //public internal graph datas:
        public List< PWNode >					nodes = new List< PWNode >();
        public List< PWOrderingGroup >			orderingGroups = new List< PWOrderingGroup >();
		[SerializeField] private int			_seed;
		public int								seed { get { return _seed; } set { if (OnSeedChanged != null) OnSeedChanged(); _seed = value; } }
		[SerializeField] private int			_chunkSize;
		public int								chunkSize { get { return _chunkSize; } set { if (OnChunkSizeChanged != null) OnChunkSizeChanged(); _chunkSize = value; } }
		[SerializeField] private Vector3		_chunkPosition;
		public Vector3							chunkPosition { get { return _chunkPosition; } set { if (OnChunkPositionChanged != null) OnChunkPositionChanged(); _chunkPosition = value; } }
		[SerializeField] private float			_step;
		public float							step { get { return _step; } set { if (OnStepChanged != null) OnStepChanged(); _step = value; } }
		public PWGraphType						graphType;


		//Link table, store all connections between node's anchors.
		//Why i choose to store node links in a separate container instead of
		//	dirrectly inside anchors which is much more simple ?
		//	Well ... It's because unity's serialization system sucks :D
		//	Indeed i need in my graph to use only one instance of PWNodeLink per link
		//	and with the unity serialization system, if i store the same instance of a class
		//	in two location (which will append if i store the link in anchors cauz there is
		//	two anchors per link) unity, during deserialization, will create two instances
		//	of the link so the link is not anymore a link but two non-connected links.
		//	So i decided to use a linkTable which store in one place the instances of links
		//	and use GUIDs to save links references in anchors so i can get them back using 
		//	this container.
		public PWNodeLinkTable					nodeLinkTable = new PWNodeLinkTable();
        
		
		//internal graph datas:
		[SerializeField]
		private PWGraphProcessor				graphProcessor = new PWGraphProcessor();
		protected bool					    	realMode;
		[System.NonSerialized]
		protected IOrderedEnumerable< PWNode >	computeOrderSortedNodes;
		[System.NonSerialized]
		protected Dictionary< int, PWNode >		nodesDictionary = new Dictionary< int, PWNode >();


        //editor datas:
		[SerializeField] Vector2				_panPosition;
		public Vector2							panPosition { get { return _panPosition + zoomPanCorrection; } set { _panPosition = value - zoomPanCorrection; } }
		public float							scale = 2;
		public Vector2							zoomPanCorrection;
        public PWGUIManager						PWGUI = new PWGUIManager();
		[System.NonSerialized]
		public PWGraphEditorEventInfo			editorEvents = new PWGraphEditorEventInfo();
		public float							maxStep;


        //input and output nodes:
        public PWNodeGraphInput					inputNode;
        public PWNodeGraphOutput				outputNode;

		
		//useful variables:
		public bool								initialized = false;


		//public delegates:
		public delegate void					LinkAction(PWNodeLink link);
		public delegate void					NodeAction(PWNode node);


		//node events:
		public event NodeAction					OnNodeRemoved;
		public event NodeAction					OnNodeAdded;
		public event NodeAction					OnNodeSelected;
		public event NodeAction					OnNodeUnselected;
		//link events:
		//fired when a link start to be dragged
		public event Action< PWAnchor >			OnLinkStartDragged;
		public event Action						OnLinkStopDragged;
		public event Action						OnLinkCanceled;
		public event LinkAction					OnLinkCreated;
		public event LinkAction					OnLinkRemoved;
		public event LinkAction					OnLinkUnselected;
		//parameter events:
		public event System.Action				OnSeedChanged;
		public event System.Action				OnChunkSizeChanged;
		public event System.Action				OnStepChanged;
		public event System.Action				OnChunkPositionChanged;
		//graph events:
		public event System.Action				OnGraphStructureChanged;
		public event System.Action				OnClickNowhere; //when click inside the graph, not on a node nor a link.
		public event System.Action				OnAllNodeReady;
		//editor button events:
		public event System.Action				OnForceReload;
		public event System.Action				OnForceReloadOnce;
	
	#endregion

		//this method is called onlyv when the graph is created by the PWGraphManager
		public virtual void Initialize()
		{
			Debug.LogWarning("Initialized graph !");

			//initialize the graph pan position
			panPosition = Vector2.zero;
	
			realMode = false;
			// presetChoosed = false;
			
			//default values:
			chunkSize = 16;
			step = 1;
			maxStep = 4;
			name = "New ProceduralWorld";
			
			inputNode = CreateNewNode< PWNodeGraphInput >(new Vector2(50, 0));
			outputNode = CreateNewNode< PWNodeGraphOutput >(new Vector2(-100, 0));
			
			initialized = true;
		}

		public virtual void OnEnable()
		{
			//check if the object have been initialized, if not, quit.
			if (!initialized)
				return ;

			//Events attach
			OnGraphStructureChanged += GraphStructureChangedCallback;
			OnLinkCreated += LinkChangedCallback;
			OnLinkRemoved += LinkChangedCallback;
			OnNodeRemoved += NodeCountChangedCallback;
			OnNodeAdded += NodeCountChangedCallback;
			OnAllNodeReady += NodeReadyCallback;

			graphProcessor.Initialize();
			
			//Send OnAfterSerialize here because when graph's OnEnable function is
			// called, all it's nodes are already deserialized.
			foreach (var node in nodes)
				node.OnAfterGraphDeserialize(this);
		}

		public virtual void OnDisable()
		{
			//Events detach
			OnGraphStructureChanged -= GraphStructureChangedCallback;
			OnLinkCreated -= LinkChangedCallback;
			OnLinkRemoved -= LinkChangedCallback;
			OnNodeRemoved -= NodeCountChangedCallback;
			OnNodeAdded -= NodeCountChangedCallback;
			OnAllNodeReady -= NodeReadyCallback;
		}

		public void NotifyNodeReady(PWNode node)
		{
			//if one node isn't ready, return
			if (nodes.Any(n => !n.ready))
				return ;

			if (OnAllNodeReady != null)
				OnAllNodeReady();
		}

		//when all nodes are ready, we can use their datas
		void NodeReadyCallback()
		{
			//add all existing nodes to the nodesDictionary
			for (int i = 0; i < nodes.Count; i++)
				nodesDictionary[nodes[i].id] = nodes[i];
			nodesDictionary[inputNode.id] = inputNode;
			nodesDictionary[outputNode.id] = outputNode;
			
			//Build compute order list
			UpdateComputeOrder();
		}

		//must be called after a Process() to get back datas
		public T GetOutput< T >()
		{
			//get the PWArray from the graph output node
			PWArray< object > outputArray = outputNode.inputValues;

			if (outputArray == null)
			{
				Debug.LogError("[PWGraph] Graph's output array is null");
				return default(T);
			}

			//find if there are any object in the array of type T
			foreach (var obj in outputArray)
			{
				if (obj != null && obj.GetType().IsAssignableFrom(typeof(T)))
				{
					Debug.Log("found output value: " + obj);
					return (T)obj;
				}
			}
			
			Debug.LogError("[PWGraph] Type '" + typeof(T) + "' was not found in the graph output values");
			return default(T);
		}

		//Durty Clone cauz unity ScriptableObject does not implement a Clone method >.<
		public PWGraph Clone()
		{
			//Instancing a new graph fmor this one will duplicate the object but not the nodes
			// so the OnEnable function will bind this new graph to our nodes, to revert
			// that we need to clone each nodes and assign them to this new graph.
			PWGraph clonedGraph = Object.Instantiate(this);

			//clean and add copies of nodes into the cloned graph
			clonedGraph.nodes.Clear();
			foreach (var node in nodes)
				clonedGraph.nodes.Add(Object.Instantiate(node));
			
			//reenable the new graph so the new nodes are taken in account
			clonedGraph.OnDisable();
			clonedGraph.OnEnable();

			//reenable all clone graph nodes
			foreach (var node in clonedGraph.nodes)
			{
				node.OnDisable();
				node.OnEnable();
			}

			//reenable our graph to rebind our nodes to our graph
			OnDisable();
			OnEnable();

			return clonedGraph;
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

		public void ProcessNodes(List< PWNode > nodes)
		{
			graphProcessor.UpdateNodeDictionary(nodesDictionary);
			graphProcessor.ProcessNodes(this, nodes);
		}

		//export the graph as commands in a file and return the created file path
		public string Export(string filePath)
		{
			return PWGraphCLI.Export(this, filePath);
		}

		//wipeDatas will remove all the graph content before importing the file
		public void Import(string filePath, bool wipeDatas = false)
		{
			Debug.Log("TODO");
		}

		public void Execute(string command)
		{
			PWGraphCLI.Execute(this, command);
		}

		public bool SetInput(string fieldName, object value)
		{
			Debug.Log("TODO");
			return false;
		}
	
		//Dictionary< nodeId, dependencyWeight >
		[System.NonSerialized]
		Dictionary< int, int > nodeComputeOrderCount = new Dictionary< int, int >();
		int EvaluateComputeOrder(bool first = true, int depth = 0, int nodeId = -1)
		{
			//Recursively evaluate compute order for each nodes:
			if (first)
			{
				nodeComputeOrderCount.Clear();

				inputNode.computeOrder = 0;
	
				foreach (var gNode in nodes)
					gNode.computeOrder = EvaluateComputeOrder(false, 1, gNode.id);
	
				outputNode.computeOrder = EvaluateComputeOrder(false, 1, outputNode.id);
	
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
			if (!node.canWork)
				return -1;
	
			//compute dependency weight:
			int	ret = 1;
			foreach (var dep in node.GetInputNodes())
			{
				int d = EvaluateComputeOrder(false, depth + 1, dep.id);
	
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
	
	#region Events handlers

		//retarget link and node events to GraphStructure event
		void		LinkChangedCallback(PWNodeLink link) { OnGraphStructureChanged(); }
		void		NodeCountChangedCallback(PWNode n) { OnGraphStructureChanged(); }

		void		GraphStructureChangedCallback() { UpdateComputeOrder(); }

		//event accessors for PWGraphEditor
		public void RaiseOnClickNowhere() { if (OnClickNowhere != null) OnClickNowhere(); }
		public void RaiseOnForceReload() { if (OnForceReload != null) OnForceReload(); UpdateComputeOrder(); }
		public void RaiseOnForceReloadOnce() { if (OnForceReloadOnce != null) OnForceReloadOnce(); UpdateComputeOrder(); }
		public void RaiseOnLinkStartDragged(PWAnchor anchor) { if (OnLinkStartDragged != null) OnLinkStartDragged(anchor); }
		public void RaiseOnLinkCancenled() { if (OnLinkCanceled != null) OnLinkCanceled(); }
		public void RaiseOnLinkStopDragged() { if (OnLinkStopDragged != null) OnLinkStopDragged(); }

	#endregion

	#region Nodes API

		public bool		IsRealMode()
		{
			return realMode;
		}

		public void		SetRealMode(bool value)
		{
			realMode = value;
		}

		public PWNode	FindNodeById(int nodeId)
		{
			return nodesDictionary[nodeId];
		}

		public PWNode	FindNodeByName(string name)
		{
			return nodes.FirstOrDefault(n => n.name == name);
		}

		public IOrderedEnumerable< PWNode >	GetComputeSortedNodes()
		{
			return computeOrderSortedNodes;
		}

		public T		CreateNewNode< T >(Vector2 position) where T : PWNode
		{
			return CreateNewNode(typeof(T), position) as T;
		}

		public PWNode	CreateNewNode(System.Type nodeType, Vector2 position)
		{
			PWNode newNode = ScriptableObject.CreateInstance(nodeType) as PWNode;
			
			position.x = Mathf.RoundToInt(position.x);
			position.y = Mathf.RoundToInt(position.y);
			newNode.rect.position = position;
			
			newNode.Initialize(this);

			nodes.Add(newNode);
			nodesDictionary[newNode.id] = newNode;

			if (OnNodeAdded != null)
				OnNodeAdded(newNode);
			
			return newNode;
		}
		
		public bool		RemoveNode(int nodeId)
		{
			return RemoveNode(nodesDictionary[nodeId]);
		}

		public bool		RemoveNode(PWNode removeNode)
		{
			//can't delete an input/output node
			if (removeNode == inputNode || removeNode == outputNode)
				return false;
			
			var item = nodesDictionary.First(kvp => kvp.Value == removeNode);
			nodes.Remove(removeNode);
			
			if (OnNodeRemoved != null)
				OnNodeRemoved(removeNode);
			
			return nodesDictionary.Remove(item.Key);
		}

		public void		RemoveLink(PWNodeLink link)
		{
			if (OnLinkRemoved != null)
				OnLinkRemoved(link);
			
			link.fromAnchor.RemoveLinkReference(link);
			link.toAnchor.RemoveLinkReference(link);
			nodeLinkTable.RemoveLink(link);
		}
		
		//Create a link from the anchor where the link was dragged and the parameter
		public PWNodeLink	CreateLink(PWAnchor anchor)
		{
			return CreateLink(editorEvents.startedLinkAnchor, anchor);
		}

		//SafeCreateLink will create link and delete other overlapping links if there are
		public PWNodeLink	SafeCreateLink(PWAnchor anchor)
		{
			return SafeCreateLink(editorEvents.startedLinkAnchor, anchor);
		}

		public PWNodeLink	SafeCreateLink(PWAnchor fromAnchor, PWAnchor toAnchor)
		{
			PWAnchor	fAnchor = fromAnchor;
			PWAnchor	tAnchor = toAnchor;

			//swap anchors if input/output are reversed
			if (fromAnchor.anchorType != PWAnchorType.Output)
			{
				tAnchor = fromAnchor;
				fAnchor = toAnchor;
			}

			if (!PWAnchorUtils.AnchorAreAssignable(fAnchor, tAnchor))
				return null;
			
			if (tAnchor.linkCount > 0)
				tAnchor.RemoveAllLinks();
			
			return CreateLink(fAnchor, tAnchor);
		}

		//create a link without checking for duplication
		public PWNodeLink	CreateLink(PWAnchor fromAnchor, PWAnchor toAnchor)
		{
			PWNodeLink	link = new PWNodeLink();
			PWAnchor	fAnchor = fromAnchor;
			PWAnchor	tAnchor = toAnchor;
			
			//swap anchors if input/output are reversed
			if (fromAnchor.anchorType != PWAnchorType.Output)
			{
				tAnchor = fromAnchor;
				fAnchor = toAnchor;
			}

			if (!PWAnchorUtils.AnchorAreAssignable(fAnchor, tAnchor))
			{
				Debug.LogWarning("[PWGraph] attemp to create a link between unlinkable anchors: " + fAnchor.fieldType + " into " + tAnchor.fieldType);
				return null;
			}

			link.Initialize(fAnchor, tAnchor);
			nodeLinkTable.AddLink(link);

			//raise link creation event
			OnLinkCreated(link);

			if (Event.current != null)
				Event.current.Use();

			return link;
		}

		public IEnumerable< PWNode > GetNodeChildsRecursive(PWNode begin)
		{
			Stack< PWNode > nodeStack = new Stack< PWNode >();

			nodeStack.Push(begin);

			while (nodeStack.Count != 0)
			{
				var node = nodeStack.Pop();

				foreach (var outputNode in node.GetOutputNodes())
					nodeStack.Push(outputNode);
				
				if (node != begin)
					yield return node;
			}
		}

	#endregion

    }
}