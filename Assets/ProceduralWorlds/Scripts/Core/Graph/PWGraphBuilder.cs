using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace PW.Core
{
	public class PWGraphBuilder
	{

		PWGraph	graph;

		List< string >	commands = new List< string >();
	
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

		public PWGraphBuilder Execute(bool clearCommandsOnceExecuted = false)
		{
			foreach (var cmd in commands)
				graph.Execute(cmd);
			
			if (clearCommandsOnceExecuted)
				commands.Clear();
			
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

		public void Import(string fileName, bool assetPath = true)
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
			string[] lines = File.ReadAllLines(fileName);

			foreach (var line in lines)
				commands.Add(line);
		}

		public PWGraph GetGraph()
		{
			return graph;
		}
	
	}
}