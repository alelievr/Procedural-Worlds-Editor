using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Profiling;

using Debug = UnityEngine.Debug;
using NodeFieldDictionary = System.Collections.Generic.Dictionary< string, System.Collections.Generic.Dictionary< string, System.Reflection.FieldInfo > >;

namespace PW.Core
{
	public class PWGraphProcessor
	{

		NodeFieldDictionary			bakedNodeFields = new NodeFieldDictionary();
		Dictionary< int, PWNode >	nodesDictionary;

	#region Initialization
		
		void BakeNode(System.Type t)
		{
			var dico = new Dictionary< string, FieldInfo >();
			bakedNodeFields[t.AssemblyQualifiedName] = dico;
	
			foreach (var field in t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
				dico[field.Name] = field;
		}
	
		public void Initialize()
		{
			//bake node fields to accelerate data transfer from node to node.
			bakedNodeFields.Clear();
			foreach (var nodeType in PWNodeTypeProvider.GetAllNodeTypes())
				BakeNode(nodeType);
		}
	
	#endregion

		//Check errors when transferring values from a node to another
		bool CheckProcessErrors(PWNodeLink link, PWNode node, bool realMode)
		{
			if (!realMode)
			{
				if (!nodesDictionary.ContainsKey(link.toNode.id))
				{
					Debug.LogError("[PW Process] " + "node id (" + link.toNode.id + ") not found in nodes dictionary");
					return true;
				}

				if (nodesDictionary[link.toNode.id] == null)
				{
					Debug.LogError("[PW Process] " + "node id (" + link.toNode.id + ") is null in nodes dictionary");
					return true;
				}

				if (!bakedNodeFields.ContainsKey(link.fromNode.classAQName)
					|| !bakedNodeFields[link.fromNode.classAQName].ContainsKey(link.fromAnchor.fieldName)
					|| !bakedNodeFields[link.toNode.classAQName].ContainsKey(link.toAnchor.fieldName))
				{
					Debug.LogError("[PW Process] Can't find field: "
						+ link.fromAnchor.fieldName + " in " + link.fromNode.classAQName
						+ " OR " + link.toAnchor.fieldName + " in " + link.toNode.classAQName);
					return true;
				}
					
				if (bakedNodeFields[link.fromNode.classAQName][link.fromAnchor.fieldName].GetValue(node) == null)
				{
					Debug.Log("[PW Process] tring to assign null value from "
						+ link.fromNode.classAQName + "." + link.fromAnchor.fieldName);
					return true;
				}
			}

			return false;
		}
		
		void TrySetValue(FieldInfo prop, object val, PWNode target, PWNode from, bool realMode, bool clone = false)
		{
			//clone the input variable if requested by input anchor and if possible.
			if (clone && val.GetType().IsAssignableFrom(typeof(Sampler)))
				val = (val as Sampler).Clone(prop.GetValue(from) as Sampler);
			
			if (realMode)
				prop.SetValue(target, val);
			else
				try {
					prop.SetValue(target, val);
				} catch (Exception e) {
					Debug.LogError("[PWGraph Processor] " + e);
				}
		}
	
		void ProcessNodeLinks(PWNode node, bool realMode)
		{
			var links = node.GetOutputLinks();

			if (!realMode)
				Profiler.BeginSample("Process node links " + node);

			foreach (var link in links)
			{
				//if we are in real mode, we check all errors and discard if there is any.
				if (CheckProcessErrors(link, node, realMode))
					continue ;

				var val = bakedNodeFields[link.fromNode.classAQName][link.fromAnchor.fieldName].GetValue(node);
				var prop = bakedNodeFields[link.toNode.classAQName][link.toAnchor.fieldName];
	
				// Debug.Log("local: " + link.fromNode.classAQName + " / " + node.GetType() + " / " + node.nodeId);
				// Debug.Log("distant: " + link.distantClassAQName + " / " + target.GetType() + " / " + target.nodeId);
				// Debug.Log("set value: " + val.GetHashCode() + "(" + val + ")" + " to " + target.GetHashCode() + "(" + target + ")");

				//Without multi-anchor, simple assignation
				if (link.toAnchor.fieldIndex == -1 && link.fromAnchor.fieldIndex == -1)
					TrySetValue(prop, val, link.toNode, link.fromNode, realMode, (link.toAnchor.transferType == PWTransferType.Copy));
				
				//Distant anchor is a multi-anchor
				else if (link.toAnchor.fieldIndex != -1 && link.fromAnchor.fieldIndex == -1)
				{
					
					var pwArray = prop.GetValue(link.toNode);

					//TODO: bake this abomination
					bool b = (bool)pwArray.GetType().GetMethod("AssignAt").Invoke(pwArray, new object[]{link.toAnchor.fieldIndex, val, link.fromAnchor.name, false});

					if (!b)
						Debug.LogError("[PWGraph Processor] Failed to set distant indexed field value: " + link.toAnchor.fieldName + " at index: " + link.toAnchor.fieldIndex);
				}

				//Local link is a multi-anchor
				else if (link.toAnchor.fieldIndex == -1 && link.fromAnchor.fieldIndex != -1 && val != null)
				{
					//TODO: bake this abomination
					object localVal = val.GetType().GetMethod("At").Invoke(val, new object[]{link.fromAnchor.fieldIndex});

					TrySetValue(prop, localVal, link.toNode, link.fromNode, realMode);
				}

				//Both are multi-anchors
				else if (val != null)
				{
					//TODO: brun these abomination
					object localVal = val.GetType().GetMethod("At").Invoke(val, new object[]{link.fromAnchor.fieldIndex});
	
					var pwArray = prop.GetValue(link.toNode);
					var assign = pwArray.GetType().GetMethod("AssignAt");
					if (!(bool)assign.Invoke(pwArray, new object[]{link.toAnchor.fieldIndex, localVal, link.fromAnchor.name, false}))
						Debug.Log("[PWGraph Processor] Failed to set distant indexed field value: " + link.toAnchor.fieldName);
				}
			}

			if (!realMode)
				Profiler.EndSample();
		}
	
		float ProcessNode(PWNode node, bool realMode)
		{
			float	calculTime = 0;

			//if you are in editor mode, update the process time of the node
			if (!realMode)
			{
				Profiler.BeginSample("Process node " + node);
				Stopwatch	st = new Stopwatch();

				st.Start();
				node.Process();
				st.Stop();

				node.processTime = st.ElapsedMilliseconds;
				calculTime = node.processTime;
			}
			else
				node.Process();

			ProcessNodeLinks(node, realMode);

			if (!realMode)
				Profiler.EndSample();

			return calculTime;
		}

		public float Process(PWGraph graph)
		{
			float	calculTime = 0f;
			bool	realMode = graph.IsRealMode();

			if (graph.GetComputeSortedNodes() == null)
				graph.UpdateComputeOrder();
			
			foreach (var node in graph.GetComputeSortedNodes())
			{
				//ignore unlinked nodes
				if (node.computeOrder < 0)
					continue ;
				
				calculTime += ProcessNode(node, realMode);
			}
			return calculTime;
		}

		public float ProcessNodes(PWGraph graph, List< PWNode > nodes)
		{
			float	calculTime = 0f;
			bool	realMode = graph.IsRealMode();

			//sort nodes by compute order:
			nodes.Sort((n1, n2) => n1.computeOrder.CompareTo(n2.computeOrder));

			foreach(var node in nodes)
			{
				if (node.computeOrder < 0)
					continue ;
				
				calculTime += ProcessNode(node, realMode);
			}

			return calculTime;
		}
		
		public void	ProcessOnce(PWGraph graph)
		{
			if (graph.GetComputeSortedNodes() == null)
				graph.UpdateComputeOrder();

			foreach (var node in graph.GetComputeSortedNodes())
			{
				//ignore unlinked nodes
				if (node.computeOrder < 0)
					continue ;
				
				node.OnNodeProcessOnce();

				ProcessNodeLinks(node, graph.IsRealMode());
			}
		}

	#region Utils
	
		public void UpdateNodeDictionary(Dictionary< int, PWNode > nd)
		{
			nodesDictionary = nd;
		}

	#endregion
	}
}
