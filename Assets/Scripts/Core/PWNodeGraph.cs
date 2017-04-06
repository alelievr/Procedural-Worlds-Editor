using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;

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
		public string						saveName;
		[SerializeField]
		public Vector2						graphDecalPosition;
		[SerializeField]
		public int							localWindowIdCount;
		[SerializeField]
		public string						firstInitialization;
		[SerializeField]
		public bool							realMode;
		
		[SerializeField]
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
		public List< string >				PWNodeSubGraphs = new List< string >();
		[SerializeField]
		public string						PWNodeGraphParent = null;

		[SerializeField]
		public PWNode						inputNode;
		[SerializeField]
		public PWNode						outputNode;
		[SerializeField]
		public PWNode						externalGraphNode;

		[System.NonSerializedAttribute]
		public bool							unserializeInitialized = false;

		[System.NonSerializedAttribute]
		public Dictionary< int, PWNode > nodesDictionary = new Dictionary< int, PWNode >();

		[System.NonSerializedAttribute]
		Dictionary< string, Dictionary< string, FieldInfo > > bakedNodeFields = new Dictionary< string, Dictionary< string, FieldInfo > >();

		[System.NonSerializedAttribute]
		List< Type > allNodeTypeList = new List< Type >{
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
		
			//TODO: a Dictionary of PWNodeGraph
			
			//add all existing nodes to the nodesDictionary
			foreach (var node in nodes)
				nodesDictionary[node.windowId] = node;
			foreach (var subgraphName in PWNodeSubGraphs)
			{
				var subgraph = FindGraphByName(subgraphName);

				if (subgraph.externalGraphNode != null)
					nodesDictionary[subgraph.externalGraphNode.windowId] = subgraph.externalGraphNode;
			}
			if (externalGraphNode != null)
				nodesDictionary[externalGraphNode.windowId] = externalGraphNode;
			if (inputNode != null)
				nodesDictionary[inputNode.windowId] = inputNode;
			if (outputNode != null)
				nodesDictionary[outputNode.windowId] = outputNode;
		}
		
		void ProcessNodeLinks(PWNode node)
		{
			var links = node.GetLinks();
	
			foreach (var link in links)
			{
				var target = nodesDictionary[link.distantWindowId];
	
				if (target == null)
					continue ;
	
				var val = bakedNodeFields[link.localClassAQName][link.localName].GetValue(node);
				var prop = bakedNodeFields[link.distantClassAQName][link.distantName];
				if (link.distantIndex == -1)
					prop.SetValue(target, val);
				else //multiple object data:
				{
					PWValues values = (PWValues)prop.GetValue(target);
	
					if (values != null)
					{
						if (!values.AssignAt(link.distantIndex, val, link.localName))
							Debug.Log("failed to set distant indexed field value: " + link.distantName);
					}
				}
			}
		}

		public void ProcessGraph()
		{
			//here nodes are sorted by compute-order
			//TODO: rework this to get a working in-depth node process call
			//AND integrate notifyDataChanged in this todo.

			if (PWNodeGraphParent != null)
			{
				inputNode.Process();
				ProcessNodeLinks(inputNode);
			}
			foreach (var node in nodes)
				if (node != null && node.computeOrder != -1)
				{
					node.Process();
					ProcessNodeLinks(node);
				}
		/*	foreach (var graph in PWNodeSubGraphs)
			{
				graph.outputNode.Process();
				ProcessNodeLinks(graph.outputNode);
			}*/
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

		public PWNodeGraph FindGraphByName(string name = null)
		{
			if (name == null)
				return this;
			
			//TODO: find a solution to load assetBundle at runtime 

			return null;
		}

		public void ForeachAllNodes(System.Action< PWNode > callback, bool recursive = false, bool graphInputAndOutput = false, string graphName = null)
		{
			var graph = FindGraphByName(graphName);
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
				foreach (var subgraph in graph.PWNodeSubGraphs)
					ForeachAllNodes(callback, recursive, graphInputAndOutput, subgraph);
		}
    }
}