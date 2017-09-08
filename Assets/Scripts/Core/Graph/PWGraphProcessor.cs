using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine;

using Debug = UnityEngine.Debug;
using NodeFieldDictionary = System.Collections.Generic.Dictionary< string, System.Collections.Generic.Dictionary< string, System.Reflection.FieldInfo > >;

namespace PW.Core
{
	public class PWGraphProcessor {

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
		bool CheckProcessErrors(PWLink link, PWNode node, bool realMode)
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
		
		void TrySetValue(FieldInfo prop, object val, PWNode target, bool realMode)
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
	
		void ProcessNodeLinks(PWNode node, bool realMode)
		{
			var links = node.GetLinks();

			foreach (var link in links)
			{
				//if we are in real mode, we check all errors and discard if there is any.
				if (CheckProcessErrors(link, node, realMode))
					continue ;
				
				var target = nodesDictionary[link.distantNodeId];
				var val = bakedNodeFields[link.localClassAQName][link.localName].GetValue(node);
				var prop = bakedNodeFields[link.distantClassAQName][link.distantName];
	
				// Debug.Log("local: " + link.localClassAQName + " / " + node.GetType() + " / " + node.nodeId);
				// Debug.Log("distant: " + link.distantClassAQName + " / " + target.GetType() + " / " + target.nodeId);
				// Debug.Log("set value: " + val.GetHashCode() + "(" + val + ")" + " to " + target.GetHashCode() + "(" + target + ")");

				// simple assignation, without multi-anchor
				if (link.distantIndex == -1 && link.localIndex == -1)
					TrySetValue(prop, val, target, realMode);
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

					TrySetValue(prop, localVal, target, realMode);
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
	
		float ProcessNode(PWNode node, bool realMode)
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
			
			ProcessNodeLinks(node, realMode);

			return calculTime;
		}

		public float Process(PWGraph graph)
		{
			float		calculTime = 0f;
			bool		realMode = graph.IsRealMode();

			if (graph.GetComputeSortedNodes() == null)
				graph.UpdateComputeOrder();
			
			foreach (var node in graph.GetComputeSortedNodes())
			{
				//ignore unlinked nodes
				if (node.computeOrder < 0)
					continue ;
				
				if (realMode)
					node.BeginFrameUpdate();
				
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
