using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;
using UnityEngine;

using Debug = UnityEngine.Debug;
using OrderedNodeList = System.Linq.IOrderedEnumerable< PW.PWNode >;

namespace PW.Core
{
	[System.Serializable]
    public class PWGraph : ScriptableObject {
    
	#region Graph Datas

        //version infos:
		public int								majorVersion = 0;
		public int								minorVersion = 0;
        public string							creator = null;


        //asset datas:
        public string							assetFilePath;
		public string							PWFolderPath;
        public new string               	    name;


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
		public Vector2							panPosition;
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
		public event LinkAction					OnLinkDragged;
		public event LinkAction					OnLinkCanceled;
		public event LinkAction					OnLinkCreated;
		public event LinkAction					OnLinkRemoved;
		public event LinkAction					OnLinkSelected;
		public event LinkAction					OnLinkUnselected;
		//parameter events:
		public event System.Action				OnSeedChanged;
		public event System.Action				OnChunkSizeChanged;
		public event System.Action				OnStepChanged;
		public event System.Action				OnChunkPositionChanged;
		//graph events:
		public event System.Action				OnGraphStructureChanged;
		public event System.Action				OnClickNowhere; //when click inside the graph, not on a node nor a link.
		//editor button events:
		public event System.Action				OnForceReload;
		public event System.Action				OnForceReloadOnce;
	
	#endregion

		//this method must only be called when a new graph is created from PWgraphManager class
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
			
			outputNode = CreateNewNode< PWNodeGraphOutput >(new Vector2(-100, 0));
			inputNode = CreateNewNode< PWNodeGraphInput >(new Vector2(50, 0));
			
			initialized = true;
		}

		public virtual void OnEnable()
		{
			//check if the object have been initialized, if not, quit.
			if (!initialized)
				return ;

			Debug.Log("OnEnable graph, node count: " + nodes.Count);

			graphProcessor.Initialize();

			//add all existing nodes to the nodesDictionary
			for (int i = 0; i < nodes.Count; i++)
				nodesDictionary[nodes[i].id] = nodes[i];
			nodesDictionary[inputNode.id] = inputNode;
			nodesDictionary[outputNode.id] = outputNode;
			
			//Send OnAfterSerialize here because when graph's OnEnable function is
			//	called, all it's nodes are already deserialized.
			foreach (var node in nodes)
				node.OnAfterDeserialize(this);

			//Build compute order list
			UpdateComputeOrder();
			
			//Attach node's events
			foreach (var node in nodes)
				AttachNodeEvents(node);
			
			//Events attach
			OnGraphStructureChanged += GraphStructureChangedCallback;
			OnLinkCreated += LinkChangedCallback;
			OnLinkRemoved += LinkChangedCallback;
			OnNodeRemoved += NodeCountChangedCallback;
			OnNodeAdded += NodeCountChangedCallback;
		}

		public virtual void OnDisable()
		{
			//Events detach
			OnGraphStructureChanged -= GraphStructureChangedCallback;
			OnLinkCreated -= LinkChangedCallback;
			OnLinkRemoved -= LinkChangedCallback;
			OnNodeRemoved -= NodeCountChangedCallback;
			OnNodeAdded -= NodeCountChangedCallback;

			
			foreach (var node in nodes)
				DetachNodeEvents(node);
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

			Debug.Log("TODO");
		}

		public void Import(string filePath, bool wipeDatas = false)
		{

			Debug.Log("TODO");
		}

		public bool Execute(string command)
		{
			
			return false;
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
		void		LinkSelectedCallback(PWNodeLink link) { OnLinkSelected(link); }
		void		LinkUnselectedCallback(PWNodeLink link) { OnLinkUnselected(link); }
		void		LinkRemovedCallback(PWNodeLink link) { RemoveLink(link); }

		void		AnchorLinkedCallback(PWAnchor anchor) { CreateLink(anchor); }

		void		GraphStructureChangedCallback() { UpdateComputeOrder(); }

		void		AnchorLinked(PWAnchor anchor)
		{
			
		}

		public void RaiseOnClickNowhere() { OnClickNowhere(); }
		public void RaiseOnLinkSelected(PWNodeLink link) { OnLinkSelected(link); }
		public void RaiseOnForceReload() { OnForceReload(); UpdateComputeOrder(); }
		public void RaiseOnForceReloadOnce() { OnForceReloadOnce(); }

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
			
			newNode.Initialize(this);

			AttachNodeEvents(newNode);

			nodes.Add(newNode);
			nodesDictionary[newNode.id] = newNode;

			if (OnNodeAdded != null)
				OnNodeAdded(newNode);
			
			return newNode;
		}

		public bool		RemoveNode(PWNode removeNode)
		{
			DetachNodeEvents(removeNode);
			var item = nodesDictionary.First(kvp => kvp.Value == removeNode);
			nodes.Remove(removeNode);
			
			if (OnNodeRemoved != null)
				OnNodeRemoved(removeNode);
			
			return nodesDictionary.Remove(item.Key);
		}
		
		public bool		RemoveNode(int nodeId)
		{
			PWNode node = nodesDictionary[nodeId];

			DetachNodeEvents(node);
			//sending this event will cause the node remove self.

			if (OnNodeRemoved != null)
				OnNodeRemoved(node);
			
			return nodesDictionary.Remove(nodeId);
		}

		public void		RemoveLink(PWNodeLink link)
		{
			if (OnLinkRemoved != null)
				OnLinkRemoved(link);
			
			link.fromAnchor.RemoveLink(link);
			link.toAnchor.RemoveLink(link);
			nodeLinkTable.RemoveLink(link);
		}

		//Create a link from the anchor where the link was dragged and the parameter
		public PWNodeLink	CreateLink(PWAnchor anchor)
		{
			PWAnchor fromAnchor = editorEvents.startedLinkAnchor;

			if (fromAnchor == null || anchor == null)
				return null;
			
			return CreateLink(fromAnchor, anchor);
		}

		public PWNodeLink	CreateLink(PWAnchor fromAnchor, PWAnchor toAnchor)
		{
			PWNodeLink link = new PWNodeLink();
			PWAnchor	fAnchor = fromAnchor;
			PWAnchor	tAnchor = toAnchor;
			
			//swap anchors if input/output are reversed
			if (fromAnchor.anchorType != PWAnchorType.Output)
				tAnchor = fromAnchor;
			if (toAnchor.anchorType != PWAnchorType.Input)
				fAnchor = toAnchor;

			if (!PWAnchorUtils.AnchorAreAssignable(fAnchor, tAnchor))
			{
				Debug.LogWarning("[PWGraph] attemp to create a link between unlinkable anchors");
				return null;
			}

			link.Initialize(fAnchor, tAnchor);
			nodeLinkTable.AddLink(link);
			return link;
		}

		void AttachNodeEvents(PWNode node)
		{
			node.OnLinkSelected += LinkSelectedCallback;
			node.OnLinkUnselected += LinkUnselectedCallback;
			node.OnAnchorLinked += AnchorLinkedCallback;
			node.OnLinkRemoved += LinkRemovedCallback;
		}

		void DetachNodeEvents(PWNode node)
		{
			node.OnLinkSelected -= LinkSelectedCallback;
			node.OnLinkUnselected -= LinkUnselectedCallback;
			node.OnAnchorLinked -= AnchorLinkedCallback;
			node.OnLinkRemoved -= LinkRemovedCallback;
		}

		public IEnumerable< PWNode > GetNodeChildsRecursive(PWNode begin)
		{
			//TODO using the new (actually non-existing) node-link system.
			return null;
		}

	#endregion

    }
}