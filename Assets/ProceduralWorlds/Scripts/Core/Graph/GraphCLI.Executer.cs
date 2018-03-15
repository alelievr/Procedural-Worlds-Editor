using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;
using System.IO;

namespace ProceduralWorlds.Core
{
	public class BaseGraphCLIAttributes : Pairs< string, object > {}

	//Command line interpreter for BaseGraph
	public static partial class BaseGraphCLI
	{

		delegate void CommandAction(BaseGraph graph, BaseGraphCommand command, string originalCommand);

		static Dictionary< BaseGraphCommandType, CommandAction > commandTypeFunctions = new Dictionary< BaseGraphCommandType, CommandAction >
		{
			{BaseGraphCommandType.Link, CreateLink},
			{BaseGraphCommandType.NewNode, CreateNode},
			{BaseGraphCommandType.NewNodePosition, CreateNode},
			{BaseGraphCommandType.LinkAnchor, CreateLinkAnchor},
			{BaseGraphCommandType.LinkAnchorName, CreateLinkAnchorName},
		};
		
		static void CreateLinkAnchorName(BaseGraph graph, BaseGraphCommand command, string inputCommand)
		{
			BaseNode fromNode, toNode;

			GetNodes(graph, command, out fromNode, out toNode, inputCommand);

			var fromAnchorField = fromNode.outputAnchorFields.Find(af => af.fieldName == command.fromAnchorFieldName);
			var toAnchorField = toNode.inputAnchorFields.Find(af => af.fieldName == command.toAnchorFieldName);

			if (fromAnchorField == null)
				throw new InvalidOperationException("Anchor " + command.fromAnchorFieldName + " not found in node: " + fromNode);
			if (toAnchorField == null)
				throw new InvalidOperationException("Anchor " + command.toAnchorFieldName + " not found in node: " + toNode);
			
			Anchor fromAnchor, toAnchor;
			
			FindAnchors(fromAnchorField, toAnchorField, out fromAnchor, out toAnchor);

			graph.SafeCreateLink(fromAnchor, toAnchor);
		}

		static void CreateLinkAnchor(BaseGraph graph, BaseGraphCommand command, string inputCommand)
		{
			BaseNode fromNode, toNode;

			GetNodes(graph, command, out fromNode, out toNode, inputCommand);

			var fromAnchorFields = fromNode.outputAnchorFields;
			var toAnchorFields = toNode.inputAnchorFields;

			if (command.fromAnchorIndex < 0 || command.fromAnchorIndex >= fromAnchorFields.Count)
				throw new InvalidOperationException("Anchor " + command.fromAnchorIndex + " out of range in node: " + fromNode);
			if (command.toAnchorIndex < 0 || command.toAnchorIndex >= toAnchorFields.Count)
				throw new InvalidOperationException("Anchor " + command.fromAnchorIndex + " out of range in node: " + toNode);
			
			var fromAnchorField = fromAnchorFields[command.fromAnchorIndex];
			var toAnchorField = toAnchorFields[command.toAnchorIndex];

			Anchor fromAnchor, toAnchor;
			
			FindAnchors(fromAnchorField, toAnchorField, out fromAnchor, out toAnchor);

			graph.SafeCreateLink(fromAnchor, toAnchor);
		}

		static void	CreateLink(BaseGraph graph, BaseGraphCommand command, string inputCommand)
		{
			//get nodes from the graph:
			BaseNode fromNode, toNode;

			GetNodes(graph, command, out fromNode, out toNode, inputCommand);
			
			//Create the first linkable anchors we found:
			foreach (var outAnchor in fromNode.outputAnchors)
				foreach (var inAnchor in toNode.inputAnchors)
					if (AnchorUtils.AnchorAreAssignable(outAnchor, inAnchor))
					{
						//if the input anchor is already linked, find another
						if (inAnchor.linkCount == 1)
							continue ;
						
						graph.CreateLink(outAnchor, inAnchor);
						return ;
					}
			
			Debug.LogError("Can't link " + fromNode + " with " + toNode);
		}

		static void	CreateNode(BaseGraph graph, BaseGraphCommand command, string inputCommand)
		{
			Vector2	position = command.position;
			BaseNode node = null;
			
			//if we receive a CreateNode with input/output graph nodes, we assign them so we don't have multiple inout/output nodes
			if (NodeTypeProvider.inputGraphTypes.Contains(command.nodeType))
				node = graph.inputNode;
			else if (NodeTypeProvider.outputGraphTypes.Contains(command.nodeType))
				node = graph.outputNode;
			else
				node = graph.CreateNewNode(command.nodeType, position);
			
			//set the position again for input/output nodes
			node.rect.position = position;
			
			Type nodeType = node.GetType();

			if (!String.IsNullOrEmpty(command.attributes))
			{
				foreach (var attr in Jsonizer.Parse(command.attributes))
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

		public static void Execute(BaseGraph graph, string inputCommand)
		{
			BaseGraphCommand command = Parse(inputCommand);

			if (commandTypeFunctions.ContainsKey(command.type))
				commandTypeFunctions[command.type](graph, command, inputCommand);
			else
				throw new InvalidOperationException("Command type not handled: " + command.type);
		}

		#region Utils

		static void GetNodes(BaseGraph graph, BaseGraphCommand command, out BaseNode fromNode, out BaseNode toNode, string inputCommand)
		{
			fromNode = graph.FindNodeByName(command.fromNodeName);
			toNode = graph.FindNodeByName(command.toNodeName);

			if (fromNode == null)
				throw new InvalidOperationException("Node " + command.fromNodeName + " not found in graph while parsing: '" + inputCommand + "'");
			if (toNode == null)
				throw new InvalidOperationException("Node " + command.toNodeName + " not found in graph while parsing: '" + inputCommand + "'");
		}

		static void FindAnchors(AnchorField fromAnchorField, AnchorField toAnchorField, out Anchor fromAnchor, out Anchor toAnchor)
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