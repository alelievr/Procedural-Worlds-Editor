using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

namespace ProceduralWorlds.Core
{
	public class GraphBuilder
	{

		BaseGraph	graph;

		List< string >	commands = new List< string >();
		readonly List< Action< BaseGraph > > postExecuteCallbacks = new List< Action< BaseGraph > >();
		
		readonly List< string > commandOrder = new List< string >{"NewNode", "Link", "LinkAnchor"};
	
		//private constructor so you can't instantiate the class
		GraphBuilder() {}
	
		//call this methof to create an instance of this class
		public static GraphBuilder FromGraph(BaseGraph graph)
		{
			GraphBuilder builder = new GraphBuilder();
			builder.graph = graph;

			return builder;
		}

		public static GraphBuilder NewGraph< T >() where T : BaseGraph
		{
			if (typeof(T).IsAbstract)
			{
				Debug.LogError("[GraphBuilder] Can't instatiate an abstract graph");
				return null;
			}

			GraphBuilder	builder = new GraphBuilder();

			builder.graph = ScriptableObject.CreateInstance< T >();
			builder.graph.Initialize();
			builder.graph.OnEnable();
			builder.graph.name = Guid.NewGuid().ToString();

			//if the created graph is a biome graph, we manually reset previewGraph
			//to avoid unwished graph asset processing
			if (typeof(T) == typeof(BiomeGraph))
				(builder.graph.inputNode as NodeBiomeGraphInput).previewGraph = null;

			return builder;
		}

		public GraphBuilder NewNode(Type nodeType, string name, BaseGraphCLIAttributes attributes = null)
		{
			if (!nodeType.IsSubclassOf(typeof(BaseNode)))
			{
				Debug.Log("[GraphBuilder] unknown node type: '" + nodeType + "'");
				return this;
			}
			commands.Add(BaseGraphCLI.GenerateNewNodeCommand(nodeType, name, attributes));
			return this;
		}

		public GraphBuilder NewNode< T >(string name, BaseGraphCLIAttributes attributes = null) where T : BaseNode
		{
			commands.Add(BaseGraphCLI.GenerateNewNodeCommand(typeof(T), name, attributes));
			return this;
		}

		public GraphBuilder NewNode(Type nodeType, Vector2 position, string name, BaseGraphCLIAttributes attributes = null)
		{
			if (!nodeType.IsSubclassOf(typeof(BaseNode)))
			{
				Debug.Log("[GraphBuilder] unknown node type: '" + nodeType + "'");
				return this;
			}
			commands.Add(BaseGraphCLI.GenerateNewNodeCommand(nodeType, name, position, attributes));
			return this;
		}

		public GraphBuilder Link(string from, string to)
		{
			commands.Add(BaseGraphCLI.GenerateLinkCommand(from, to));
			return this;
		}

		public GraphBuilder Link(string fromNode, string fromAnchor, string toNode, string toAnchor)
		{
			commands.Add(BaseGraphCLI.GenerateLinkAnchorNameCommand(fromNode, fromAnchor, toNode, toAnchor));
			return this;
		}

		public GraphBuilder Link(string fromNode, int fromAnchorIndex, string toNode, int toAnchorIndex)
		{
			commands.Add(BaseGraphCLI.GenerateLinkAnchorCommand(fromNode, fromAnchorIndex, toNode, toAnchorIndex));
			return this;
		}

		public GraphBuilder Custom(Action< BaseGraph > callback)
		{
			callback(graph);
			return this;
		}

		public GraphBuilder CustomAfterExecute(Action< BaseGraph > callback)
		{
			postExecuteCallbacks.Add(callback);
			return this;
		}
		public GraphBuilder SortCommands()
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

		public GraphBuilder Execute(bool clearCommandsOnceExecuted = false)
		{
			SortCommands();

			foreach (var cmd in commands)
				graph.Execute(cmd);
			
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

		public GraphBuilder Import(string fileName, bool assetPath = true)
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

		public GraphBuilder ImportCommands(params string[] cmds)
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

		public BaseGraph GetGraph()
		{
			return graph;
		}

    }
}