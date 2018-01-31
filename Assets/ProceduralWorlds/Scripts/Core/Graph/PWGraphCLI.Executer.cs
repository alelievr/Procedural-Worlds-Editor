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

		#region  Command Execution

		static void	CreateLink(PWGraph graph, PWGraphCommand command, string inputCommand)
		{
			//get nodes from the graph:
			PWNode fromNode = graph.FindNodeByName(command.fromName);
			PWNode toNode = graph.FindNodeByName(command.toName);

			if (fromNode == null)
				throw new Exception("Node " + command.fromName + " found in graph while parsing: '" + inputCommand + "'");
			if (toNode == null)
				throw new Exception("Node " + command.toName + " found in graph while parsing: '" + inputCommand + "'");
			
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

		#endregion // Command Execution

	}
}