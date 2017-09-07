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
		
	#region Process and ProcessOnce
	
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

		public float Process()
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
		public void	ProcessOnce()
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
	
				UpdateComputeOrder();
	
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
			computeOrderSortedNodes = nodesDictionary
					//select all nodes building an object with node value and graph name (if needed)
					.Select(kp => kp.Value)
					//sort the resulting list by computeOrder:
					.OrderBy(n => n.computeOrder);
		}

	#endregion

    }
}