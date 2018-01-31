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
	//Command line interpreter for PWGraph
	public static partial class PWGraphCLI
	{

		#region Export and command creation

		public static void Export(PWGraph graph, string filePath)
		{
			List< string > commands = new List< string >();
			List< string > nodeNames = new List< string >();
			var nodeToNameMap = new Dictionary< PWNode, string >();

			foreach (var node in graph.nodes)
			{
				var attrs = new PWGraphCLIAttributes();

				//load node attributes
				FieldInfo[] attrFields = node.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
				foreach (var field in attrFields)
				{
					var attributes = field.GetCustomAttributes(false);

					bool isInput = attributes.Any(a => {
						return a.GetType() == typeof(PWInputAttribute);
					});

					if (isInput)
						continue ;

					//if the field can't be jsonified, we skip it
					if (PWJson.allowedJsonTypes.FindIndex(j => j.type == field.FieldType) == -1)
						continue ;

					attrs.Add(field.Name, field.GetValue(node));
				}

				string	nodeName = node.name;
				int		i = 0;

				//unique name generation
				while (nodeNames.Contains(nodeName))
					nodeName = node.name + i++;
				
				nodeToNameMap[node] = nodeName;
				
				commands.Add(GenerateNewNodeCommand(node.GetType(), nodeName, node.rect.position, attrs));
			}
		
			foreach (var link in graph.nodeLinkTable.GetLinks())
			{
				if (link.fromNode == null || link.toNode == null)
					continue ;
				
				var fromName = nodeToNameMap[link.fromNode];
				var toName = nodeToNameMap[link.toNode];

				commands.Add(GenerateLinkCommand(fromName, toName));
			}

			File.WriteAllLines(filePath, commands.ToArray());
		}
		
		public static void Import(PWGraph graph, string filePath, bool wipeDatas = false)
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
				Execute(graph, command);
		}

		public static string GenerateNewNodeCommand(Type nodeType, string name, PWGraphCLIAttributes datas = null)
		{
			//TODO: use tokens here
			string cmd = "NewNode " + nodeType.Name + " \"" + name + "\"";
			if (datas != null && datas.Count != 0)
				cmd += " attr=" + PWJson.Generate(datas);
			
			return cmd;
		}

		public static string GenerateNewNodeCommand(Type nodeType, string name, Vector2 position, PWGraphCLIAttributes datas = null)
		{
			//TODO: use tokens here
			string cmd = "NewNode " + nodeType.Name + " \"" + name + "\" (" + (int)position.x + ", " + (int)position.y+ ")";
			if (datas != null && datas.Count != 0)
				cmd += " attr=" + PWJson.Generate(datas);
			
			return cmd;
		}

		public static string GenerateLinkCommand(string fromName, string toName)
		{
			//TODO: use tokens here
			return "Link \"" + fromName + "\" \"" + toName + "\"";
		}

		#endregion //Export and command creation

	}
}