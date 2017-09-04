using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System;
using Debug = UnityEngine.Debug;

namespace PW.Core
{
	using Node;
	
	public enum	PWTerrainOutputMode
	{
		None,
		SideView2D,
		TopDown2D,
		Planar3D,
		Spherical3D,
		Cubic3D,
		Density1D,
		Density2D,
		Density3D,
		Mesh,
	}

	public enum PWGraphProcessMode
	{
		Normal,		//output a disaplayable terrain (with isosurface / oth)
		Geologic,	//output a structure containing all maps for a chunk (terrain, wet, temp, biomes, ...)
	}

	[CreateAssetMenu(fileName = "New ProceduralWorld", menuName = "Procedural World", order = 1)]
	[System.SerializableAttribute]
	public class PWNodeGraph : ScriptableObject {

		[SerializeField]
		public List< PWNode >				nodes = new List< PWNode >();
		[SerializeField]
		public List< PWOrderingGroup >		orderingGroups = new List< PWOrderingGroup >();

		//Editor only variables:
		[SerializeField]
		public int							majorVersion = 0;
		public int							minorVersion = 0;

		[SerializeField]
		public HorizontalSplitView			h1;
		[SerializeField]
		public HorizontalSplitView			h2;
	
		[SerializeField]
		public Vector2						leftBarScrollPosition;
		[SerializeField]
		public Vector2						selectorScrollPosition;
		
		[SerializeField]
		public string						searchString = "";

		//Asset variables:
		[SerializeField]
		public string						externalName;
		[SerializeField]
		public string						assetPath;
		[SerializeField]
		public string						saveName;
		[SerializeField]
		public Vector2						graphDecalPosition;
		[SerializeField]
		[HideInInspector]
		public int							localNodeIdCount;
		[SerializeField]
		[HideInInspector]
		public string						firstInitialization = null;
		[SerializeField]
		public bool							realMode;

		[SerializeField]
		public bool							presetChoosed;

		[SerializeField]
		public int							chunkSize;
		[SerializeField]
		public int							seed;
		[SerializeField]
		public float						step;
		[SerializeField]
		public float						geologicTerrainStep;
		[SerializeField]
		public float						maxStep;

		[SerializeField]
		public PWTerrainOutputMode			outputType;

		[SerializeField]
		public PWNode						inputNode;
		[SerializeField]
		public PWNode						outputNode;

		[SerializeField]
		public PWGraphProcessMode			processMode;

		//for GUI settings storage
		[SerializeField]
		public PWGUIManager					PWGUI;

		[System.NonSerialized]
		IOrderedEnumerable< PWNode >		computeOrderSortedNodes = null;

		[System.NonSerialized]
		public bool							unserializeInitialized = false;

		[System.NonSerialized]
		public Dictionary< int, PWNode >	nodesDictionary = new Dictionary< int, PWNode >();

		[System.NonSerialized]
		Dictionary< string, Dictionary< string, FieldInfo > > bakedNodeFields = new Dictionary< string, Dictionary< string, FieldInfo > >();

		[System.NonSerialized]
		public bool							isVisibleInEditor = false;

		[System.NonSerialized]
		List< Type > allNodeTypeList = new List< Type > {
			typeof(PWNodeSlider), typeof(PWNodeTexture2D), typeof(PWNodeMaterial), typeof(PWNodeConstant), typeof(PWNodeMesh), typeof(PWNodeGameObject), typeof(PWNodeColor), typeof(PWNodeSurfaceMaps),
			typeof(PWNodeAdd), typeof(PWNodeCurve),
			typeof(PWNodeDebugLog),
			typeof(PWNodeCircleNoiseMask),
			typeof(PWNodePerlinNoise2D),
			typeof(PWNodeSideView2DTerrain), typeof(PWNodeTopDown2DTerrain),
			typeof(PWNodeGraphInput), typeof(PWNodeGraphOutput), typeof(PWNodeGraphExternal),
			typeof(PWNodeBiomeData), typeof(PWNodeBiomeBinder), typeof(PWNodeWaterLevel),
			typeof(PWNodeBiomeBlender), typeof(PWNodeBiomeSwitch), typeof(PWNodeBiomeTemperature),
			typeof(PWNodeBiomeWetness), typeof(PWNodeBiomeSurface), typeof(PWNodeBiomeTerrain),

		};

		[System.NonSerialized]
		Vector3							currentChunkPosition;

		//Precomputed data part:
		[SerializeField]
		public TerrainDetail			terrainDetail = new TerrainDetail();
		[SerializeField]
		public int						geologicDistanceCheck;

		[System.NonSerialized]
		public GeologicBakedDatas		geologicBakedDatas = new GeologicBakedDatas();
		
	#region Graph baking (prepare regions that will be computed once)

		void BakeNode(Type t)
		{
			var dico = new Dictionary< string, FieldInfo >();
			bakedNodeFields[t.AssemblyQualifiedName] = dico;
	
			foreach (var field in t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
				dico[field.Name] = field;
		}

		void InsertNodeIfNotExists(List< PWNode > toComputeList, PWNode node)
		{
			if (toComputeList.Any(i => i.nodeId == node.nodeId))
				return ;
			
			toComputeList.Insert(0, node);
		}

		void TryBuildGraphPart(PWNode leadNode, List< PWNode > toComputeList, int depth = 0)
		{
			InsertNodeIfNotExists(toComputeList, leadNode);
			
			foreach (var dep in leadNode.GetDependencies())
			{
				var depNode = FindNodebyId(dep.nodeId);

				if (depNode.processMode == PWNodeProcessMode.RequestForProcess)
				{
					InsertNodeIfNotExists(toComputeList, depNode);

					TryBuildGraphPart(depNode, toComputeList, depth++);
				}
			}
		}

		[System.NonSerializedAttribute]
		//store the list of nodes to becomputed per RequestForProcess nodes so if you ask to process
		// a node, it will just take the list from this dic. and compute them in the order of the list
		Dictionary< int, List< PWNode > >	bakedGraphParts = new Dictionary< int, List< PWNode > >();
		void BakeGraphParts(bool threaded = true)
		{
			//TODO: thread this, can be long

			ForeachAllNodes((n) => {
				if (!n)
					return ;
					
				var links = n.GetLinks();
				var deps = n.GetDependencies();

				if (links.Count > 0 && links.All(l => l.mode == PWNodeProcessMode.RequestForProcess))
				{
					List< PWNode > toComputeList;

					if (bakedGraphParts.ContainsKey(n.nodeId))
						toComputeList = bakedGraphParts[n.nodeId];
					else
						toComputeList = bakedGraphParts[n.nodeId] = new List< PWNode >();
					
					//analyze of "extern groups" (group of node with only one RequestForProcess link at the end)
					if (deps.Any(d => FindNodebyId(d.nodeId).processMode == PWNodeProcessMode.RequestForProcess))
					{
						TryBuildGraphPart(n, toComputeList);

						return ;
					}

					//go back to the farest node dependency with the RequestForProcess links:
					foreach (var depNodeId in n.GetDependencies().Select(d => d.nodeId).Distinct())
					{
						var node = FindNodebyId(depNodeId);

						if (node == null)
							continue ;
						
						if (node.GetLinks().All(dl => dl.mode == PWNodeProcessMode.RequestForProcess))
							InsertNodeIfNotExists(toComputeList, node);
					}
					InsertNodeIfNotExists(toComputeList, n);

					foreach (var link in links.Where(l => l.mode == PWNodeProcessMode.RequestForProcess).GroupBy(l => l.localNodeId).Select(g => g.First()))
					{
						PWNode node = FindNodebyId(link.distantNodeId);
					
						//if the node goes nowhere, add it
						if (node.GetLinks().Count == 0)
						{
							List< PWNode > subToComputeList;
							if (bakedGraphParts.ContainsKey(node.nodeId))
								subToComputeList = bakedGraphParts[node.nodeId];
							else
								subToComputeList = bakedGraphParts[node.nodeId] = new List< PWNode >();
							
							subToComputeList.RemoveAll(nodeInfo => toComputeList.Any(ni => ni.nodeId == nodeInfo.nodeId));

							subToComputeList.InsertRange(0, toComputeList);
							if (!subToComputeList.Any(ni => ni.nodeId == node.nodeId))
								subToComputeList.Insert(subToComputeList.Count, node);
						}
					}
				}
			}, true, true);
			
			/*Debug.Log("created graph parts: " + bakedGraphParts.Count);
			foreach (var kp in bakedGraphParts)
			{
				Debug.Log("to compute list for node " + kp.Key);
				foreach (var ne in kp.Value)
					Debug.Log("\tne: " + ne.node.nodeId);
			}*/
		}

		public void RebakeGraphParts()
		{
			foreach (var kp in bakedGraphParts)
				kp.Value.Clear();
			bakedGraphParts.Clear();
			BakeGraphParts(false);
		}

		void		BakeNeededGeologicDatas()
		{
			float		oldStep = step;
			processMode = PWGraphProcessMode.Geologic;
			step = geologicTerrainStep;

			for (int x = 0; x < geologicDistanceCheck; x++)
				for (int y = 0; y < geologicDistanceCheck; y++)
					ProcessGraph();

			UpdateChunkPosition(currentChunkPosition);
			processMode = PWGraphProcessMode.Normal;
			step = oldStep;
		}

	#endregion

	#region Graph initialization
	
		public void OnEnable()
		{
			//bake node fields to accelerate data transfer from node to node.
			bakedNodeFields.Clear();
			foreach (var nodeType in allNodeTypeList)
				BakeNode(nodeType);

			//add all existing nodes to the nodesDictionary
			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes[i] != null)
				{
					nodes[i].RunNodeAwake();
					nodesDictionary[nodes[i].nodeId] = nodes[i];
				}
				else
				{
					nodes.RemoveAt(i);
					i--;
				}
			}
			if (inputNode != null)
				nodesDictionary[inputNode.nodeId] = inputNode;
			if (outputNode != null)
				nodesDictionary[outputNode.nodeId] = outputNode;

			foreach (var node in nodes)
				node.UpdateCurrentGraph(this);
			if (inputNode != null)
				inputNode.UpdateCurrentGraph(this);
			if (outputNode != null)
				outputNode.UpdateCurrentGraph(this);
			
			//bake the graph parts (RequestForProcess nodes)
			BakeGraphParts();
		}

	#endregion

	#region Graph processing

		void ProcessNodeLinks(PWNode node)
		{
			var links = node.GetLinks();

			foreach (var link in links)
			{
				if (!nodesDictionary.ContainsKey(link.distantNodeId))
					continue;
				
				if (link.mode == PWNodeProcessMode.RequestForProcess)
					continue ;

				var target = nodesDictionary[link.distantNodeId];
	
				if (target == null)
					continue ;
	
				// Debug.Log("local: " + link.localClassAQName + " / " + node.GetType() + " / " + node.nodeId);
				// Debug.Log("distant: " + link.distantClassAQName + " / " + target.GetType() + " / " + target.nodeId);
				
				//ignore old links not removed cause of property removed in a script at compilation
				if (!realMode)
					if (!bakedNodeFields.ContainsKey(link.localClassAQName)
						|| !bakedNodeFields[link.localClassAQName].ContainsKey(link.localName)
						|| !bakedNodeFields[link.distantClassAQName].ContainsKey(link.distantName))
						{
							Debug.LogError("Can't find field: " + link.localName + " in " + link.localClassAQName + " OR " + link.distantName + " in " + link.distantClassAQName);
							continue ;
						}

				var val = bakedNodeFields[link.localClassAQName][link.localName].GetValue(node);
				if (val == null)
					Debug.Log("null value of node: " + node.GetType() + " of field: " + link.localName);
				var prop = bakedNodeFields[link.distantClassAQName][link.distantName];

				// Debug.Log("set value: " + val.GetHashCode() + "(" + val + ")" + " to " + target.GetHashCode() + "(" + target + ")");

				// simple assignation, without multi-anchor
				if (link.distantIndex == -1 && link.localIndex == -1)
				{
					if (realMode)
						prop.SetValue(target, val);
					else
						try {
							prop.SetValue(target, val);
						} catch (Exception e) {
							Debug.LogError(e);
						}
				}
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

					if (realMode)
						prop.SetValue(target, localVal);
					else
					{
						try {
							prop.SetValue(target, localVal);
						} catch {
							Debug.LogWarning("can't assign " + link.localName + " to " + link.distantName);
						}
					}
				}
				else if (val != null) // both are multi-anchors
				{
					PWValues values = (PWValues)prop.GetValue(target);
					object localVal = ((PWValues)val).At(link.localIndex);
	
					if (values != null)
					{
						// Debug.Log("assigned total multi");
						if (!values.AssignAt(link.distantIndex, localVal, link.localName))
							Debug.Log("failed to set distant indexed field value: " + link.distantName);
					}
				}
			}
		}

		float ProcessNode(PWNode node)
		{
			float	calculTime = 0;

			//if you are in editor mode, update the process time of the node
			if (!realMode)
			{
				Stopwatch	st = new Stopwatch();

				st.Start();
				node.Process();
				st.Stop();

				node.processTime = st.ElapsedMilliseconds;
				calculTime = node.processTime;
			}
			else
				node.Process();

			if (realMode)
				node.EndFrameUpdate();
			
			ProcessNodeLinks(node);

			return calculTime;
		}

		public float ProcessGraph()
		{
			//TODO: processMode
			float		calculTime = 0f;

			if (computeOrderSortedNodes == null)
				UpdateComputeOrder();

			if (terrainDetail.biomeDetailMask != 0 && processMode == PWGraphProcessMode.Normal)
				BakeNeededGeologicDatas();
			
			foreach (var node in computeOrderSortedNodes)
			{
				//ignore unlink nodes
				if (node.computeOrder < 0)
					continue ;

				//TODO: uncomment when TerrainBuilder node will be OK
				// if (processMode == PWGraphProcessMode.Geologic && type == typeof(PWNodeTerrainBuilder))
					// return ;
				
				if (realMode)
					node.BeginFrameUpdate();
				
				//if node outputs is only in RequestForProcess mode, avoid the computing
				var links = node.GetLinks();
				if (links.Count > 0 && !links.Any(l => l.mode == PWNodeProcessMode.AutoProcess))
					continue ;
				
				calculTime += ProcessNode(node);
			}
			return calculTime;
		}

		//call all processOnce functions
		public void	ProcessGraphOnce()
		{
			Debug.LogWarning("Process once called !");
			
			if (computeOrderSortedNodes == null)
				UpdateComputeOrder();

			foreach (var node in computeOrderSortedNodes)
			{
				if (node.computeOrder < 0)
					continue ;
				
				node.OnNodeProcessOnce();

				ProcessNodeLinks(node);
			}
		}

		public bool RequestProcessing(int nodeId)
		{
			if (bakedGraphParts.ContainsKey(nodeId))
			{
				var toComputeList = bakedGraphParts[nodeId];

				//compute the list
				foreach (var nodeInfo in toComputeList)
					ProcessNode(nodeInfo);

				return true;
			}
			return false;
		}
	
	#endregion

	#region Graph API

		public void			UpdateSeed(int seed)
		{
			this.seed = seed;
			ForeachAllNodes((n) => n.seed = seed, true, true);
		}

		public void			UpdateChunkPosition(Vector3 chunkPos)
		{
			currentChunkPosition = chunkPos;
			ForeachAllNodes((n) => n.chunkPosition = chunkPos, true, true);
		}

		public void			UpdateChunkSize(int chunkSize)
		{
			this.chunkSize = chunkSize;
			ForeachAllNodes((n) => n.chunkSize = chunkSize, true, true);
		}

		public void			UpdateStep(float step)
		{
			this.step = step;
			ForeachAllNodes((n) => n.step = step, true, true);
		}

		public PWNode		FindNodebyId(int nodeId)
		{
			if (nodesDictionary.ContainsKey(nodeId))
				return nodesDictionary[nodeId];
			return null;
		}

		public void			ForeachAllNodes(System.Action< PWNode > callback, bool recursive = false, bool graphInputAndOutput = false, PWNodeGraph graph = null)
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
		}

		public void	UpdateComputeOrder()
		{
			computeOrderSortedNodes = nodesDictionary
					//select all nodes building an object with node value and graph name (if needed)
					.Select(kp => kp.Value)
					//sort the resulting list by computeOrder:
					.OrderBy(n => n.computeOrder);
		}

	#endregion
    }
}
