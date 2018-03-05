using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;
using System.IO;

namespace PW.Core
{
	public class PWGraphCLIAttributes : Pairs< string, object > {}

	//Command line interpreter for PWGraph
	public static partial class PWGraphCLI
	{

		static Dictionary< PWGraphCommandType, Action< PWGraph, PWGraphCommand, string > > commandTypeFunctions = new Dictionary< PWGraphCommandType, Action< PWGraph, PWGraphCommand, string > >()
		{
			{PWGraphCommandType.Link, CreateLink},
			{PWGraphCommandType.NewNode, CreateNode},
			{PWGraphCommandType.NewNodePosition, CreateNode},
			{PWGraphCommandType.LinkAnchor, CreateLinkAnchor},
			{PWGraphCommandType.LinkAnchorName, CreateLinkAnchorName},
		};
		
		static void CreateLinkAnchorName(PWGraph graph, PWGraphCommand command, string inputCommand)
		{
			PWNode fromNode, toNode;

			GetNodes(graph, command, out fromNode, out toNode, inputCommand);

			var fromAnchorField = fromNode.outputAnchorFields.Find(af => af.fieldName == command.fromAnchorFieldName);
			var toAnchorField = toNode.inputAnchorFields.Find(af => af.fieldName == command.toAnchorFieldName);

			if (fromAnchorField == null)
				throw new Exception("Anchor " + command.fromAnchorFieldName + " not found in node: " + fromNode);
			if (toAnchorField == null)
				throw new Exception("Anchor " + command.toAnchorFieldName + " not found in node: " + toNode);
			
			PWAnchor fromAnchor, toAnchor;
			
			FindAnchors(fromAnchorField, toAnchorField, out fromAnchor, out toAnchor);

			graph.SafeCreateLink(fromAnchor, toAnchor);
		}

		static void CreateLinkAnchor(PWGraph graph, PWGraphCommand command, string inputCommand)
		{
			PWNode fromNode, toNode;

			GetNodes(graph, command, out fromNode, out toNode, inputCommand);

			var fromAnchorFields = fromNode.outputAnchorFields;
			var toAnchorFields = toNode.inputAnchorFields;

			if (command.fromAnchorIndex < 0 || command.fromAnchorIndex >= fromAnchorFields.Count)
				throw new Exception("Anchor " + command.fromAnchorIndex + " out of range in node: " + fromNode);
			if (command.toAnchorIndex < 0 || command.toAnchorIndex >= toAnchorFields.Count)
				throw new Exception("Anchor " + command.fromAnchorIndex + " out of range in node: " + toNode);
			
			var fromAnchorField = fromAnchorFields[command.fromAnchorIndex];
			var toAnchorField = toAnchorFields[command.toAnchorIndex];

			PWAnchor fromAnchor, toAnchor;
			
			FindAnchors(fromAnchorField, toAnchorField, out fromAnchor, out toAnchor);

			graph.SafeCreateLink(fromAnchor, toAnchor);
		}

		static void	CreateLink(PWGraph graph, PWGraphCommand command, string inputCommand)
		{
			//get nodes from the graph:
			PWNode fromNode, toNode;

			GetNodes(graph, command, out fromNode, out toNode, inputCommand);
			
			//Create the first linkable anchors we found:
			foreach (var outAnchor in fromNode.outputAnchors)
				foreach (var inAnchor in toNode.inputAnchors)
					if (PWAnchorUtils.AnchorAreAssignable(outAnchor, inAnchor))
					{
						//if the input anchor is already linked, find another
						if (inAnchor.linkCount == 1)
							continue ;
						
						graph.CreateLink(outAnchor, inAnchor);
						return ;
					}
			
			Debug.LogError("Can't link " + fromNode + " with " + toNode);
		}

		static void	CreateNode(PWGraph graph, PWGraphCommand command, string inputCommand)
		{
			Vector2	position = command.position;
			PWNode node = null;
			
			//if we receive a CreateNode with input/output graph nodes, we assign them so we don't have multiple inout/output nodes
			if (command.nodeType == typeof(PWNodeGraphInput) || command.nodeType == typeof(PWNodeBiomeGraphInput))
				node = graph.inputNode;
			else if (command.nodeType == typeof(PWNodeGraphOutput) || command.nodeType == typeof(PWNodeBiomeGraphOutput))
				node = graph.outputNode;
			else
				node = graph.CreateNewNode(command.nodeType, position);
			
			//set the position again for input/output nodes
			node.rect.position = position;
			
			Type nodeType = node.GetType();

			if (!String.IsNullOrEmpty(command.attributes))
			{
				foreach (var attr in PWJson.Parse(command.attributes))
				{
					FieldInfo attrField = nodeType.GetField(attr.first, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

					if (attrField != null)
						attrField.SetValue(node, attr.second);
					else
						Debug.LogError("Attribute " + attr.first + " can be found in node " + node);
				}
			}

			node.name = command.name;
		}

		public static void Execute(PWGraph graph, string inputCommand)
		{
			PWGraphCommand command = Parse(inputCommand);

			if (commandTypeFunctions.ContainsKey(command.type))
				commandTypeFunctions[command.type](graph, command, inputCommand);
			else
				throw new Exception("Command type not handled: " + command.type);
		}

		#region Utils

		static void GetNodes(PWGraph graph, PWGraphCommand command, out PWNode fromNode, out PWNode toNode, string inputCommand)
		{
			fromNode = graph.FindNodeByName(command.fromNodeName);
			toNode = graph.FindNodeByName(command.toNodeName);

			Debug.Log("Graph nodes: ");
			foreach (var node in graph.nodes)
				Debug.Log("node: " + node);

			if (fromNode == null)
				throw new Exception("Node " + command.fromNodeName + " not found in graph while parsing: '" + inputCommand + "'");
			if (toNode == null)
				throw new Exception("Node " + command.toNodeName + " not found in graph while parsing: '" + inputCommand + "'");
		}

		static void FindAnchors(PWAnchorField fromAnchorField, PWAnchorField toAnchorField, out PWAnchor fromAnchor, out PWAnchor toAnchor)
		{
			if (fromAnchorField.multiple)
				fromAnchor = fromAnchorField.anchors.Find(a => a.linkCount == 0);
			else
				fromAnchor = fromAnchorField.anchors.First();
			
			if (toAnchorField.multiple)
				toAnchor = toAnchorField.anchors.Find(a => a.linkCount == 0);
			else
				toAnchor = toAnchorField.anchors.First();
		}

		#endregion

	}
}