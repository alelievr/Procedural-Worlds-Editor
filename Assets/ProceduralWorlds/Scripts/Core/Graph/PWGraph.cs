using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace PW.Core
{
	[Serializable]
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
		//returns all nodes in the graph (excluding input and output nodes)
        public List< PWNode >					nodes = new List< PWNode >();
        public List< PWOrderingGroup >			orderingGroups = new List< PWOrderingGroup >();
		//returns all nodes
		public IEnumerable< PWNode >			allNodes
		{
			get
			{
				yield return inputNode;
				yield return outputNode;
				foreach (var node in nodes)
					yield return node;
			}
		}
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
		[NonSerialized]
		protected IOrderedEnumerable< PWNode >	computeOrderSortedNodes;
		[NonSerialized]
		protected Dictionary< int, PWNode >		nodesDictionary = new Dictionary< int, PWNode >();
		[NonSerialized]
		private FieldInfo						inputNodeOutputValues;
		[NonSerialized]
		private FieldInfo						outputNodeInputValues;


        //editor datas:
		[SerializeField] Vector2				_panPosition;
		public Vector2							panPosition { get { return _panPosition + zoomPanCorrection; } set { _panPosition = value - zoomPanCorrection; } }
		public float							scale = 2;
		public Vector2							zoomPanCorrection;
		[NonSerialized]
		public PWGraphEditorEventInfo			editorEvents = new PWGraphEditorEventInfo();
		public bool								presetChoosed = false;
		public PWLayoutSettings					layoutSettings = new PWLayoutSettings();


        //input and output nodes:
        public PWNode							inputNode;
        public PWNode							outputNode;

		
		//useful variables:
		public bool								initialized = false;
		[NonSerialized]
		public bool								readyToProcess = false;
		public bool								hasProcessed { get { return graphProcessor.hasProcessed; } }


		//public delegates:
		public delegate void					LinkAction(PWNodeLink link);
		public delegate void					NodeAction(PWNode node);


		//node events:
		public event NodeAction					OnNodeRemoved;
		public event NodeAction					OnNodeAdded;
		//link events:
		//fired when a link start to be dragged
		public event LinkAction					OnLinkCreated;
		public event LinkAction					OnPostLinkCreated;
		public event LinkAction					OnLinkRemoved;
		public event Action						OnPostLinkRemoved;
		//graph events:
		public event Action						OnGraphStructureChanged;
		public event Action						OnAllNodeReady;
		public event Action						OnGraphPreProcess;
		public event Action						OnGraphPostProcess;
	
	#endregion

		//this method is called onlyv when the graph is created by the PWGraphFactory
		public virtual void Initialize()
		{
			//initialize the graph pan position
			panPosition = Vector2.zero;

			realMode = false;
			presetChoosed = false;
			
			InitializeInputAndOutputNodes();
			
			initialized = true;
		}

		public virtual void InitializeInputAndOutputNodes()
		{
			inputNode = CreateNewNode< PWNodeGraphInput >(new Vector2(-100, 0), "Input", true, false);
			outputNode = CreateNewNode< PWNodeGraphOutput >(new Vector2(100, 0), "Output", true, false);
		}

		public virtual void OnEnable()
		{
			//check if the object have been initialized, if not, quit.
			if (!initialized)
				return ;
				
			//Events attach
			OnPostLinkCreated += LinkPostAddedCallback;
			OnPostLinkRemoved += LinkPostRemovedCallback;
			OnNodeRemoved += NodeCountChangedCallback;
			OnNodeAdded += NodeCountChangedCallback;
			OnAllNodeReady += NodeReadyCallback;

			graphProcessor.OnEnable();

			//get the in and out node arrays refelection getters:
			inputNodeOutputValues = inputNode.GetType().GetField("outputValues", BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
			outputNodeInputValues = outputNode.GetType().GetField("inputValues",  BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);

			//Send OnAfterSerialize here because when graph's OnEnable function is
			// called, all it's nodes are already deserialized.
			foreach (var node in allNodes)
			{
				if (node != null)
					node.OnAfterGraphDeserialize(this);
			}
		}

		public virtual void OnDisable()
		{
			//Events detach
			OnPostLinkCreated -= LinkPostAddedCallback;
			OnPostLinkRemoved -= LinkPostRemovedCallback;
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

			readyToProcess = true;
		}

		//must be called after a Process() to get back datas
		public T GetOutput< T >()
		{
			//get the PWArray from the graph output node
			PWArray< object > outputArray = outputNodeInputValues.GetValue(outputNode) as PWArray< object >;

			if (outputArray == null)
			{
				Debug.LogError("[PWGraph] Graph's output array is null");
				return default(T);
			}

			//find if there are any object in the array of type T
			foreach (var obj in outputArray)
			{
				if (obj != null && typeof(T).IsAssignableFrom(obj.GetType()))
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
			clonedGraph.inputNode = Object.Instantiate(inputNode);
			AddInitializedNode(clonedGraph.inputNode, true, false);
			clonedGraph.outputNode = Object.Instantiate(outputNode);
			AddInitializedNode(clonedGraph.outputNode, true, false);
			
			//reenable the new graph so the new nodes are taken in account
			clonedGraph.OnDisable();
			clonedGraph.OnEnable();

			//reenable all cloned nodes
			foreach (var node in clonedGraph.allNodes)
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
			if (!readyToProcess)
				return -1;

			if (OnGraphPreProcess != null)
				OnGraphPreProcess();
			
			graphProcessor.UpdateNodeDictionary(nodesDictionary);
			float ret = graphProcessor.Process(this);
			
			if (OnGraphPostProcess != null)
				OnGraphPostProcess();
			
			return ret;
		}

		public void	ProcessOnce()
		{
			if (!readyToProcess)
				return ;
			
			Debug.LogWarning("Process once called !");
			graphProcessor.UpdateNodeDictionary(nodesDictionary);
			graphProcessor.ProcessOnce(this);
		}

		public void ProcessNodes(List< PWNode > nodes)
		{
			graphProcessor.UpdateNodeDictionary(nodesDictionary);
			graphProcessor.ProcessNodes(this, nodes);
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
			foreach (var inputAnchor in node.inputAnchors)
			{
				foreach (var link in inputAnchor.links)
				{
					if (link.fromNode == null)
						continue ;
					
					PWNode dep = link.fromNode;

					int d = EvaluateComputeOrder(false, depth + 1, dep.id);
		
					//if dependency does not have enought datas to compute result, abort calculus.
					if (d == -1 && inputAnchor.required)
					{
						ret = -1;
						break ;
					}
					ret += d;
				}
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

		public override string ToString()
		{
			return this.name + " " + " [" + GetType() + "]";
		}
	
	#region Events handlers

		//retarget link and node events to GraphStructure event
		void		LinkPostAddedCallback(PWNodeLink link)
		{
			if (OnGraphStructureChanged != null)
				OnGraphStructureChanged();
		}
		void		LinkPostRemovedCallback() { if (OnGraphStructureChanged != null) OnGraphStructureChanged(); }
		void		NodeCountChangedCallback(PWNode n) { if (OnGraphStructureChanged != null) OnGraphStructureChanged(); }

	#endregion
	
	#region Graph API

		public bool		IsRealMode()
		{
			return realMode;
		}

		public void		SetRealMode(bool value)
		{
			realMode = value;
		}

		//export the graph as commands in a file and return the created file path
		public void Export(string filePath)
		{
			PWGraphCLI.Export(this, filePath);
		}

		//wipeDatas will remove all the graph content before importing the file
		public void Import(string filePath, bool wipeDatas = false)
		{
			PWGraphCLI.Import(this, filePath, wipeDatas);
		}

		public void Execute(string command)
		{
			PWGraphCLI.Execute(this, command);
		}

		PWArray< object > GetInputArray()
		{
			return inputNodeOutputValues.GetValue(inputNode) as PWArray< object >;
		}

		public bool SetInput(string fieldName, object value)
		{
			var array = GetInputArray();

			int index = array.FindName(fieldName);

			if (index != -1)
				return array.AssignAt(index, value, fieldName, false);

			array.Add(value, fieldName);

			return true;
		}

		public bool SetInput(int index, object value, string name)
		{
			return GetInputArray().AssignAt(index, value, name, true);
		}

		public bool RemoveInput(string name)
		{
			return RemoveInput(GetInputArray().FindName(name));
		}

		public bool RemoveInput(int index)
		{
			return GetInputArray().RemoveAt(index);
		}

		public void ClearInput()
		{
			GetInputArray().Clear();
		}

	#endregion

	#region Nodes API

		public PWNode	FindNodeById(int nodeId)
		{
			PWNode ret;
			nodesDictionary.TryGetValue(nodeId, out ret);

			return ret;
		}

		public T		FindNodeById< T >(int nodeId) where T : PWNode
		{
			return FindNodeById(nodeId) as T;
		}

		public PWNode	FindNodeByName(string name)
		{
			return allNodes.FirstOrDefault(n => n.name == name);
		}

		public T		FindNodeByName< T >(string name) where T : PWNode
		{
			return FindNodeByName(name) as T;
		}

		public PWNode	FindNodeByType(Type type)
		{
			return allNodes.FirstOrDefault(n => n.GetType() == type);
		}
		
		public List< PWNode >	FindNodesByType(Type type)
		{
			return allNodes.Where(n => n.GetType() == type).ToList();
		}

		public T		FindNodeByType< T >() where T : PWNode
		{
			return allNodes.FirstOrDefault(n => n is T) as T;
		}
		
		public List< T >	FindNodesByType< T >() where T : PWNode
		{
			return allNodes.Where(n => n is T).Cast< T >().ToList();
		}

		public IOrderedEnumerable< PWNode >	GetComputeSortedNodes()
		{
			return computeOrderSortedNodes;
		}

		public T		CreateNewNode< T >(Vector2 position, string name = null, bool raiseEvents = true, bool addToList = true) where T : PWNode
		{
			return CreateNewNode(typeof(T), position, name, raiseEvents, addToList) as T;
		}

		public PWNode	CreateNewNode(Type nodeType, Vector2 position, string name = null, bool raiseEvents = true, bool addToList = true)
		{
			PWNode newNode = ScriptableObject.CreateInstance(nodeType) as PWNode;
			
			position.x = Mathf.RoundToInt(position.x);
			position.y = Mathf.RoundToInt(position.y);
			newNode.rect.position = position;
			
			newNode.Initialize(this);

			AddInitializedNode(newNode, raiseEvents, addToList);

			if (name != null)
				newNode.name = name;
			
			return newNode;
		}

		public void		AddInitializedNode(PWNode newNode, bool raiseEvents = true, bool addToList = true)
		{
			if (addToList)
				nodes.Add(newNode);
			
			nodesDictionary[newNode.id] = newNode;
			
			if (OnNodeAdded != null && raiseEvents)
				OnNodeAdded(newNode);
		}
		
		public bool		RemoveNode(int nodeId, bool raiseEvents = true)
		{
			return RemoveNode(nodesDictionary[nodeId], raiseEvents);
		}

		public bool		RemoveNode(PWNode removeNode, bool raiseEvents = true)
		{
			//can't delete an input/output node
			if (removeNode == inputNode || removeNode == outputNode)
				return false;
			
			int id = removeNode.id;
			nodes.Remove(removeNode);
			
			if (OnNodeRemoved != null && raiseEvents)
				OnNodeRemoved(removeNode);
			
			bool success = nodesDictionary.Remove(id);

			removeNode.RemoveSelf();

			return success;
		}

		public void		RemoveLink(PWNodeLink link, bool raiseEvents = true)
		{
			if (OnLinkRemoved != null && raiseEvents)
				OnLinkRemoved(link);
			
			if (link.fromAnchor != null)
				link.fromAnchor.RemoveLinkReference(link);
			if (link.toAnchor != null)
				link.toAnchor.RemoveLinkReference(link);
			nodeLinkTable.RemoveLink(link);

			if (OnPostLinkRemoved != null && raiseEvents)
				OnPostLinkRemoved();
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
		public PWNodeLink	CreateLink(PWAnchor fromAnchor, PWAnchor toAnchor, bool raiseEvents = true)
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
				Debug.LogWarning("[PWGraph] attempted to create a link between unlinkable anchors: " + fAnchor.fieldType + " into " + tAnchor.fieldType);
				return null;
			}

			link.Initialize(fAnchor, tAnchor);
			nodeLinkTable.AddLink(link);

			//raise link creation event
			if (OnLinkCreated != null && raiseEvents)
				OnLinkCreated(link);
			
			link.fromNode.AddLink(link);
			link.toNode.AddLink(link);

			if (OnPostLinkCreated != null && raiseEvents)
				OnPostLinkCreated(link);

			return link;
		}

		public List< PWNode > GetNodeChildsRecursive(PWNode begin)
		{
			var nodeStack = new Stack< PWNode >();
			var nodesMap = new HashSet< PWNode >();
			var nodesList = new List< PWNode >();

			nodeStack.Push(begin);

			while (nodeStack.Count != 0)
			{
				var node = nodeStack.Pop();

				foreach (var outputNode in node.GetOutputNodes())
					nodeStack.Push(outputNode);
				
				if (node != begin)
				{
					if (!nodesMap.Contains(node))
						nodesList.Add(node);
					nodesMap.Add(node);
				}
			}

			nodesList.Sort((n1, n2) => n1.computeOrder.CompareTo(n2.computeOrder));

			return nodesList;
		}

	#endregion

    }
}