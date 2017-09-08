using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine;

using Debug = UnityEngine.Debug;
using NodeFieldDictionary = System.Collections.Generic.Dictionary< string, System.Collections.Generic.Dictionary< string, System.Reflection.FieldInfo > >;
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
		protected NodeFieldDictionary			bakedNodeFields = new NodeFieldDictionary();
		protected Dictionary< int, PWNode >		nodesDictionary = new Dictionary< int, PWNode >();


        //editor datas:
		public Vector2							graphDecalPosition;
		public int								localNodeIdCount;
        public PWGUIManager						PWGUI;


        //input and output nodes:
        public PWNodeGraphInput					inputNode;
        public PWNodeGraphOutput				outputNode;
	
	#endregion
	
	#region OnEnable and OnDisable
	
		void BakeNode(Type t)
		{
			var dico = new Dictionary< string, FieldInfo >();
			bakedNodeFields[t.AssemblyQualifiedName] = dico;
	
			foreach (var field in t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
				dico[field.Name] = field;
		}
	
		public virtual void OnEnable()
		{
			//bake node fields to accelerate data transfer from node to node.
			bakedNodeFields.Clear();
			foreach (var nodeType in PWNodeTypeProvider.GetAllNodeTypes())
				BakeNode(nodeType);

			//add all existing nodes to the nodesDictionary
			for (int i = 0; i < nodes.Count; i++)
				nodesDictionary[nodes[i].nodeId] = nodes[i];
			if (inputNode != null)
				nodesDictionary[inputNode.nodeId] = inputNode;
			if (outputNode != null)
				nodesDictionary[outputNode.nodeId] = outputNode;
		}

		public virtual void OnDisable()
		{
			
		}

	#endregion

	#region Process and ProcessOnce

		//Check errors when transferring values from a node to another
		bool CheckProcessErrors(PWLink link, PWNode node)
		{
			if (!realMode)
			{
				if (!nodesDictionary.ContainsKey(link.distantNodeId))
				{
					Debug.LogError("[PW Process] " + "node id (" + link.distantNodeId + ") not found in nodes dictionary");
					return true;
				}

				if (nodesDictionary[link.distantNodeId] == null)
				{
					Debug.LogError("[PW Process] " + "node id (" + link.distantNodeId + ") is null in nodes dictionary");
					return true;
				}

				if (!bakedNodeFields.ContainsKey(link.localClassAQName)
					|| !bakedNodeFields[link.localClassAQName].ContainsKey(link.localName)
					|| !bakedNodeFields[link.distantClassAQName].ContainsKey(link.distantName))
				{
					Debug.LogError("[PW Process] Can't find field: " + link.localName + " in " + link.localClassAQName + " OR " + link.distantName + " in " + link.distantClassAQName);
					return true;
				}
					
				if (bakedNodeFields[link.localClassAQName][link.localName].GetValue(node) == null)
				{
					Debug.Log("[PW Process] tring to assign null value from " + link.localClassAQName + "." + link.localName);
					return true;
				}
			}

			return false;
		}
		
		void TrySetValue(FieldInfo prop, object val, PWNode target)
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
	
		void ProcessNodeLinks(PWNode node)
		{
			var links = node.GetLinks();

			foreach (var link in links)
			{
				//if we are in real mode, we check all errors and discard if there is any.
				if (CheckProcessErrors(link, node))
					continue ;
				
				var target = nodesDictionary[link.distantNodeId];
				var val = bakedNodeFields[link.localClassAQName][link.localName].GetValue(node);
				var prop = bakedNodeFields[link.distantClassAQName][link.distantName];
	
				// Debug.Log("local: " + link.localClassAQName + " / " + node.GetType() + " / " + node.nodeId);
				// Debug.Log("distant: " + link.distantClassAQName + " / " + target.GetType() + " / " + target.nodeId);
				// Debug.Log("set value: " + val.GetHashCode() + "(" + val + ")" + " to " + target.GetHashCode() + "(" + target + ")");

				// simple assignation, without multi-anchor
				if (link.distantIndex == -1 && link.localIndex == -1)
					TrySetValue(prop, val, target);
				//distant link is a multi-anchor
				else if (link.distantIndex != -1 && link.localIndex == -1)
				{
					PWValues values = (PWValues)prop.GetValue(target);
	
					if (values != null)
					{
						if (!values.AssignAt(link.distantIndex, val, link.localName))
							Debug.Log("failed to set distant indexed field value: " + link.distantName);
					}
				}
				//local link is a multi-anchor
				else if (link.distantIndex == -1 && link.localIndex != -1 && val != null)
				{
					object localVal = ((PWValues)val).At(link.localIndex);

					TrySetValue(prop, localVal, target);
				}
				// both are multi-anchors
				else if (val != null)
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

		public float Process()
		{
			float		calculTime = 0f;

			if (computeOrderSortedNodes == null)
				UpdateComputeOrder();
			
			foreach (var node in computeOrderSortedNodes)
			{
				//ignore unlinked nodes
				if (node.computeOrder < 0)
					continue ;
				
				if (realMode)
					node.BeginFrameUpdate();
				
				calculTime += ProcessNode(node);
			}
			return calculTime;
		}

		//call all processOnce functions
		public void	ProcessOnce()
		{
			Debug.LogWarning("Process once called !");
			
			if (computeOrderSortedNodes == null)
				UpdateComputeOrder();

			foreach (var node in computeOrderSortedNodes)
			{
				//ignore unlinked nodes
				if (node.computeOrder < 0)
					continue ;
				
				node.OnNodeProcessOnce();

				ProcessNodeLinks(node);
			}
		}

	#endregion

	#region UpdateComputeOrder
	
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
				return 0;
	
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

	#endregion

	#region Misc

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

		public PWNode FindNodeById(int nodeId)
		{
			return nodesDictionary[nodeId];
		}

	#endregion

    }
}