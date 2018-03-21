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
	//Command line interpreter for BaseGraph
	public static partial class BaseGraphCLI
	{
		public static readonly Type[] allowedGraphAttributeTypes = {typeof(int), typeof(float), typeof(bool), typeof(double), typeof(long)};
		
		public static void Export(BaseGraph graph, string filePath)
		{
			List< string > commands = new List< string >();
			List< string > nodeNames = new List< string >();
			var nodeToNameMap = new Dictionary< BaseNode, string >();

			//GraphAttr commands
			var fields = graph.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach (var field in fields)
			{
				var attrs = field.GetCustomAttributes(true);

				if (attrs.Any(a => a is TextSerializeField))
				{
					string s = GenerateGraphAttributeCommand(field.Name, field.GetValue(graph));

					if (s != null)
						commands.Add(s);
				}
			}

			//CreateNode commands
			foreach (var node in graph.allNodes)
			{
				var attrs = new BaseGraphCLIAttributes();

				//load node attributes
				FieldInfo[] attrFields = node.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
				foreach (var field in attrFields)
				{
					var attributes = field.GetCustomAttributes(false);

					bool isInput = attributes.Any(a => {
						return a.GetType() == typeof(InputAttribute);
					});

					if (isInput)
						continue ;

					//if the field can't be jsonified, we skip it
					if (Jsonizer.allowedJsonTypes.FindIndex(j => j.type == field.FieldType) == -1)
						continue ;

					attrs.Add(field.Name, field.GetValue(node));
				}

				string	nodeName = node.name;
				int		i = 0;

				//unique name generation
				while (nodeNames.Contains(nodeName))
					nodeName = node.name + i++;
			
				nodeNames.Add(nodeName);
				nodeToNameMap[node] = nodeName;
				
				commands.Add(GenerateNewNodeCommand(node.GetType(), nodeName, node.rect.position, attrs));
			}
		
			//Link commands
			foreach (var link in graph.nodeLinkTable.GetLinks())
			{
				if (link.fromNode == null || link.toNode == null)
					continue ;
				
				var fromName = nodeToNameMap[link.fromNode];
				var toName = nodeToNameMap[link.toNode];
				var fromAnchorName = link.fromAnchor.fieldName;
				var toAnchorName = link.toAnchor.fieldName;

				commands.Add(GenerateLinkAnchorNameCommand(fromName, fromAnchorName, toName, toAnchorName));
			}

			File.WriteAllLines(filePath, commands.ToArray());
		}
		
		public static void Import(BaseGraph graph, string filePath, bool wipeDatas = false)
		{
			if (wipeDatas)
			{
				while (graph.nodes.Count != 0)
				{
					Debug.Log("removing node: " + graph.nodes.First());
					graph.RemoveNode(graph.nodes.First());
				}
			}

			string[] commands = File.ReadAllLines(filePath);
			foreach (var command in commands)
			{
				//ignore empty lines:
				if (String.IsNullOrEmpty(command.Trim()))
					continue ;
				
				Execute(graph, command);
			}
		}

		public static string GenerateNewNodeCommand(Type nodeType, string name, BaseGraphCLIAttributes datas = null)
		{
			string cmd = "NewNode " + nodeType.Name + " \"" + name + "\"";
			if (datas != null && datas.Count != 0)
				cmd += " attr=" + Jsonizer.Generate(datas);
			
			return cmd;
		}

		public static string GenerateNewNodeCommand(Type nodeType, string name, Vector2 position, BaseGraphCLIAttributes datas = null)
		{
			string cmd = "NewNode " + nodeType.Name + " \"" + name + "\" (" + (int)position.x + ", " + (int)position.y+ ")";
			if (datas != null && datas.Count != 0)
				cmd += " attr=" + Jsonizer.Generate(datas);
			
			return cmd;
		}

		public static string GenerateLinkCommand(string fromNodeName, string toNodeName)
		{
			return "Link \"" + fromNodeName + "\" \"" + toNodeName + "\"";
		}

		public static string GenerateLinkAnchorCommand(string fromNodeName, int fromAnchorIndex, string toNodeName, int toAnchorIndex)
		{
			return "LinkAnchor \"" + fromNodeName + "\":" + fromAnchorIndex + " " + "\"" + toNodeName + "\":" + toAnchorIndex;
		}

		public static string GenerateLinkAnchorNameCommand(string fromNodeName, string fromAnchorName, string toNodeName, string toAnchorName)
		{
			return "LinkAnchor \"" + fromNodeName + "\":\"" + fromAnchorName + "\" " + "\"" + toNodeName + "\":\"" + toAnchorName + "\"";
		}

		public static string GenerateGraphAttributeCommand(string fieldName, object value)
		{
			if (!allowedGraphAttributeTypes.Contains(value.GetType()))
				return null;
			
			return "GraphAttr " + fieldName + " " + value;
		}

	}
}