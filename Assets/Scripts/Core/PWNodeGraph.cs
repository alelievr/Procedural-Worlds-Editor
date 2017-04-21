using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System;
using Debug = UnityEngine.Debug;

namespace PW
{
	public enum	PWOutputType
	{
		NONE,
		SIDEVIEW_2D,
		TOPDOWNVIEW_2D,
		PLANE_3D,
		SPHERICAL_3D,
		CUBIC_3D,
		DENSITY_1D,
		DENSITY_2D,
		DENSITY_3D,
		MESH,
	}

	[CreateAssetMenu(fileName = "New ProceduralWorld", menuName = "Procedural World", order = 1)]
	[System.SerializableAttribute]
	public class PWNodeGraph : ScriptableObject {
	
		[SerializeField]
		public List< PWNode >				nodes = new List< PWNode >();
		
		[SerializeField]
		public HorizontalSplitView			h1;
		[SerializeField]
		public HorizontalSplitView			h2;
	
		[SerializeField]
		public Vector2						leftBarScrollPosition;
		[SerializeField]
		public Vector2						selectorScrollPosition;
	
		[SerializeField]
		public string						externalName;
		[SerializeField]
		public string						assetName;
		[SerializeField]
		public string						assetPath;
		[SerializeField]
		public string						saveName;
		[SerializeField]
		public Vector2						graphDecalPosition;
		[SerializeField]
		[HideInInspector]
		public int							localWindowIdCount;
		[SerializeField]
		[HideInInspector]
		public string						firstInitialization = null;
		[SerializeField]
		public bool							realMode;
		
		[SerializeField]
		[HideInInspector]
		public string						searchString = "";

		[SerializeField]
		public bool							presetChoosed;

		[SerializeField]
		public int							chunkSize;
		[SerializeField]
		public int							seed;

		[SerializeField]
		public PWOutputType					outputType;

		[SerializeField]
		public List< string >				subgraphReferences = new List< string >();
		[SerializeField]
		public string						parentReference;

		[SerializeField]
		public PWNode						inputNode;
		[SerializeField]
		public PWNode						outputNode;
		[SerializeField]
		public PWNode						externalGraphNode;

		[System.NonSerializedAttribute]
		IOrderedEnumerable< PWNodeComputeInfo > computeOrderSortedNodes = null;

		[System.NonSerializedAttribute]
		private bool						graphInstanciesLoaded = false;
		[System.NonSerializedAttribute]
		public bool							unserializeInitialized = false;

		[System.NonSerializedAttribute]
		public Dictionary< string, PWNodeGraph >	graphInstancies = new Dictionary< string, PWNodeGraph >();
		[System.NonSerializedAttribute]
		public Dictionary< int, PWNode >			nodesDictionary = new Dictionary< int, PWNode >();

		[System.NonSerializedAttribute]
		Dictionary< string, Dictionary< string, FieldInfo > > bakedNodeFields = new Dictionary< string, Dictionary< string, FieldInfo > >();

		[System.NonSerializedAttribute]
		public bool			isVisibleInEditor = false;

		[System.NonSerializedAttribute]
		List< Type > allNodeTypeList = new List< Type > {
			typeof(PWNodeSlider),
			typeof(PWNodeAdd),
			typeof(PWNodeDebugLog),
			typeof(PWNodeCircleNoiseMask),
			typeof(PWNodePerlinNoise2D),
			typeof(PWNodeSideView2DTerrain), typeof(PWNodeTopDown2DTerrain),
			typeof(PWNodeGraphInput), typeof(PWNodeGraphOutput), typeof(PWNodeGraphExternal),
		};

		void BakeNode(Type t)
		{
			var dico = new Dictionary< string, FieldInfo >();
			bakedNodeFields[t.AssemblyQualifiedName] = dico;
	
			foreach (var field in t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
				dico[field.Name] = field;
		}
	
		public void OnEnable()
		{
			//bake node fields to accelerate data transfer from node to node.
			bakedNodeFields.Clear();
			foreach (var nodeType in allNodeTypeList)
				BakeNode(nodeType);

			LoadGraphInstances();
			
			//add all existing nodes to the nodesDictionary
			foreach (var node in nodes)
				nodesDictionary[node.windowId] = node;
			foreach (var subgraphName in subgraphReferences)
			{
				var subgraph = FindGraphByName(subgraphName);

				if (subgraph != null && subgraph.externalGraphNode != null)
					nodesDictionary[subgraph.externalGraphNode.windowId] = subgraph.externalGraphNode;
			}
			if (externalGraphNode != null)
				nodesDictionary[externalGraphNode.windowId] = externalGraphNode;
			if (inputNode != null)
				nodesDictionary[inputNode.windowId] = inputNode;
			if (outputNode != null)
				nodesDictionary[outputNode.windowId] = outputNode;
		}

		private class PWNodeComputeInfo
		{
			public PWNode node;
			public string graphName;

			public PWNodeComputeInfo(PWNode n, string g) {
				node = n;
				graphName = g;
			}
		}

		public void	UpdateComputeOrder()
		{
			computeOrderSortedNodes = nodesDictionary
					//select all nodes building an object with node value and graph name (if needed)
					.Select(kp => new PWNodeComputeInfo(kp.Value,
						//if node is an external node, find the name of his graph
						(kp.Value.GetType() == typeof(PWNodeGraphExternal)
							? subgraphReferences.FirstOrDefault(gName => {
								var g = FindGraphByName(gName);
								if (g.externalGraphNode.windowId == kp.Value.windowId)
									return true;
								return false;
								})
					: null)))
					//sort the resulting list by computeOrder:
					.OrderBy(n => n.node.computeOrder);
		}

		void ProcessNodeLinks(PWNode node)
		{
			var links = node.GetLinks();

			foreach (var link in links)
			{
				if (!nodesDictionary.ContainsKey(link.distantWindowId))
					continue;

				var target = nodesDictionary[link.distantWindowId];
	
				if (target == null)
					continue ;
	
				// Debug.Log("local: " + link.localClassAQName + " / " + node.GetType() + " / " + node.windowId);
				// Debug.Log("distant: " + link.distantClassAQName + " / " + target.GetType() + " / " + target.windowId);
				
				var val = bakedNodeFields[link.localClassAQName][link.localName].GetValue(node);
				if (val == null)
					Debug.Log("null value of node: " + node.GetType() + " of field: " + link.localName);
				var prop = bakedNodeFields[link.distantClassAQName][link.distantName];
				//simple assignation, without multi-anchor
				if (link.distantIndex == -1 && link.localIndex == -1)
					prop.SetValue(target, val);
				else if (link.distantIndex != -1 && link.localIndex == -1) //distant link is a multi-anchor
				{
					PWValues values = (PWValues)prop.GetValue(target);
	
					if (values != null)
					{
						if (!values.AssignAt(link.distantIndex, val, link.localName))
							Debug.Log("failed to set distant indexed field value: " + link.distantName);
					}
				}
				else if (link.distantIndex == -1 && link.localIndex != -1 && val != null) //local link is a multi-anchor
				{
					object localVal = ((PWValues)val).At(link.localIndex);

					prop.SetValue(target, localVal);
				}
				else if (val != null) // both are multi-anchors
				{
					PWValues values = (PWValues)prop.GetValue(target);
					object localVal = ((PWValues)val).At(link.localIndex);
	
					if (values != null)
					{
						if (!values.AssignAt(link.distantIndex, localVal, link.localName))
							Debug.Log("failed to set distant indexed field value: " + link.distantName);
					}
				}
			}
		}

		public void ProcessGraph()
		{
			if (computeOrderSortedNodes == null)
				UpdateComputeOrder();
			
			foreach (var nodeInfo in computeOrderSortedNodes)
			{
				//ignore unlink nodes
				if (nodeInfo.node.computeOrder < 0)
					continue ;
				if (realMode || !isVisibleInEditor)
					nodeInfo.node.BeginFrameUpdate();
				
				//if you are in editor mode, update the process time of the node
				if (!realMode)
				{
					Stopwatch	st = new Stopwatch();
					st.Start();
					nodeInfo.node.Process();
					st.Stop();
					nodeInfo.node.processTime = st.ElapsedMilliseconds;
				}
				else
					nodeInfo.node.Process();

				if (realMode || !isVisibleInEditor)
					nodeInfo.node.EndFrameUpdate();
				ProcessNodeLinks(nodeInfo.node);

				//if node was an external node, compute his subgraph
				if (nodeInfo.graphName != null)
				{
					PWNodeGraph g = FindGraphByName(nodeInfo.graphName);
					g.ProcessGraph();
				}
			}
		}

		public void	UpdateSeed(int seed)
		{
			this.seed = seed;
			ForeachAllNodes((n) => n.seed = seed, true, true);
		}

		public void UpdateChunkPosition(Vector3 chunkPos)
		{
			ForeachAllNodes((n) => n.chunkPosition = chunkPos, true, true);
		}

		public void UpdateChunkSize(int chunkSize)
		{
			this.chunkSize = chunkSize;
			ForeachAllNodes((n) => n.chunkSize = chunkSize, true, true);
		}

		void LoadGraphInstances()
		{
			//load all available graph instancies in the AssetDatabase:
			if (!String.IsNullOrEmpty(assetPath))
			{
				int		resourceIndex = assetPath.IndexOf("Resources");
				if (resourceIndex != -1)
				{
					string resourcePath = Path.ChangeExtension(assetPath.Substring(resourceIndex + 10), null);
					var graphs = Resources.LoadAll(resourcePath, typeof(PWNodeGraph));
					foreach (var graph in graphs)
					{
						if (graphInstancies.ContainsKey(graph.name))
							continue ;
						graphInstancies.Add(graph.name, graph as PWNodeGraph);
					}
				}
			}
		}

		public PWNodeGraph FindGraphByName(string name)
		{
			PWNodeGraph		ret;
				
			if (name == null)
				return null;
			if (graphInstancies.TryGetValue(name, out ret))
				return ret;
			return null;
		}

		public void ForeachAllNodes(System.Action< PWNode > callback, bool recursive = false, bool graphInputAndOutput = false, PWNodeGraph graph = null)
		{
			if (graph == null)
				graph = this;
			foreach (var node in graph.nodes)
				callback(node);
			if (graphInputAndOutput)
			{
				callback(graph.inputNode);
				callback(graph.outputNode);
			}
			if (recursive)
				foreach (var subgraphName in graph.subgraphReferences)
				{
					var g = FindGraphByName(subgraphName);
					if (g != null)
						ForeachAllNodes(callback, recursive, graphInputAndOutput, g);
				}
		}
    }
}