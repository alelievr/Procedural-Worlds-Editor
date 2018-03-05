using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

namespace PW.Core
{
	public class PWGraphBuilder
	{

		PWGraph	graph;

		List< string >	commands = new List< string >();
		List< Action< PWGraph > > postExecuteCallbacks = new List< Action< PWGraph > >();
		
		readonly List< string > commandOrder = new List< string >{"NewNode", "Link", "LinkAnchor"};
	
		//private constructor so you can't instantiate the class
		PWGraphBuilder() {}
	
		//call this methof to create an instance of this class
		public static PWGraphBuilder FromGraph(PWGraph graph)
		{
			PWGraphBuilder builder = new PWGraphBuilder();
			builder.graph = graph;

			return builder;
		}

		public static PWGraphBuilder NewGraph< T >() where T : PWGraph
		{
			if (typeof(T).IsAbstract)
			{
				Debug.LogError("[PWGraphBuilder] Can't instatiate an abstract graph");
				return null;
			}

			PWGraphBuilder	builder = new PWGraphBuilder();

			builder.graph = ScriptableObject.CreateInstance< T >();
			builder.graph.Initialize();
			builder.graph.OnEnable();
			builder.graph.name = Guid.NewGuid().ToString();

			//if the created graph is a biome graph, we manually reset previewGraph
			//to avoid unwished graph asset processing
			if (typeof(T) == typeof(PWBiomeGraph))
				(builder.graph.inputNode as PWNodeBiomeGraphInput).previewGraph = null;

			return builder;
		}

		public PWGraphBuilder NewNode(Type nodeType, string name, PWGraphCLIAttributes attributes = null)
		{
			if (!nodeType.IsSubclassOf(typeof(PWNode)))
			{
				Debug.Log("[PWGraphBuilder] unknown node type: '" + nodeType + "'");
				return this;
			}
			commands.Add(PWGraphCLI.GenerateNewNodeCommand(nodeType, name, attributes));
			return this;
		}

		public PWGraphBuilder NewNode< T >(string name, PWGraphCLIAttributes attributes = null) where T : PWNode
		{
			commands.Add(PWGraphCLI.GenerateNewNodeCommand(typeof(T), name, attributes));
			return this;
		}

		public PWGraphBuilder NewNode(Type nodeType, Vector2 position, string name, PWGraphCLIAttributes attributes = null)
		{
			if (!nodeType.IsSubclassOf(typeof(PWNode)))
			{
				Debug.Log("[PWGraphBuilder] unknown node type: '" + nodeType + "'");
				return this;
			}
			commands.Add(PWGraphCLI.GenerateNewNodeCommand(nodeType, name, position, attributes));
			return this;
		}

		public PWGraphBuilder Link(string from, string to)
		{
			commands.Add(PWGraphCLI.GenerateLinkCommand(from, to));
			return this;
		}

		public PWGraphBuilder Link(string fromNode, string fromAnchor, string toNode, string toAnchor)
		{
			commands.Add(PWGraphCLI.GenerateLinkAnchorNameCommand(fromNode, fromAnchor, toNode, toAnchor));
			return this;
		}

		public PWGraphBuilder Link(string fromNode, int fromAnchorIndex, string toNode, int toAnchorIndex)
		{
			commands.Add(PWGraphCLI.GenerateLinkAnchorCommand(fromNode, fromAnchorIndex, toNode, toAnchorIndex));
			return this;
		}

		public PWGraphBuilder Custom(Action< PWGraph > callback)
		{
			callback(graph);
			return this;
		}

		public PWGraphBuilder CustomAfterExecute(Action< PWGraph > callback)
		{
			postExecuteCallbacks.Add(callback);
			return this;
		}
		public PWGraphBuilder SortCommands()
		{
			//sort command to put CreateNodes in first
			commands = commands.OrderBy((cmd) => {
				int index = commandOrder.FindIndex(c => cmd.StartsWith(c));
				if (index == -1)
					return commandOrder.Count;
				return index;
			}).ToList();

			return this;
		}

		public PWGraphBuilder Execute(bool clearCommandsOnceExecuted = false)
		{
			SortCommands();

			foreach (var cmd in commands)
			{
				Debug.Log("Execute command: " + cmd);
				graph.Execute(cmd);
			}
			
			if (clearCommandsOnceExecuted)
				commands.Clear();

			graph.UpdateComputeOrder();

			postExecuteCallbacks.ForEach(c => c(graph));
			
			return this;
		}

		public List< string > GetCommands()
		{
			return commands;
		}

		public string Export(string fileName, bool assetPath = true)
		{
			string	outFolder;

			#if UNITY_EDITOR
				outFolder = Application.dataPath;
			#else
				outFolder = Application.persistentDataPath;
			#endif


			string dstPath;
			
			if (assetPath)
				dstPath = Path.Combine(outFolder, fileName);
			else
				dstPath = fileName;

			File.WriteAllLines(dstPath, commands.ToArray());

			return dstPath;
		}

		public PWGraphBuilder Import(string fileName, bool assetPath = true)
		{
			string filePath = fileName;

			if (assetPath)
			{
				#if UNITY_EDITOR
					filePath = Path.Combine(Application.dataPath, fileName);
				#else
					filePath = Path.Combine(Application.persistentDataPath, fileName);
				#endif
			}
			string[] lines = File.ReadAllLines(filePath);

			foreach (var line in lines)
				commands.Add(line);

			return this;
		}

		public PWGraphBuilder ImportCommands(params string[] cmds)
		{
			foreach (var cmd in cmds)
			{
				//ignore empty commands
				if (String.IsNullOrEmpty(cmd.Trim()))
					continue ;
				
				commands.Add(cmd);
			}

			return this;
		}

		public PWGraph GetGraph()
		{
			return graph;
		}

    }
}