using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PW.Core
{
	public class PWGraphBuilder {

		PWGraph	graph;

		List< string >	commands;
	
		//private constructor so you can't instantiate the class
		PWGraphBuilder() {}
	
		//call this methof to create an instance of this class
		public static PWGraphBuilder FromGraph(PWGraph graph)
		{
			PWGraphBuilder builder = new PWGraphBuilder();
			builder.graph = graph;

			return builder;
		}

		public PWGraphBuilder NewNode(Type nodeType, string name)
		{
			commands.Add(PWGraphCLI.GenerateNewNodeCommand(nodeType, name));
			return this;
		}

		public PWGraphBuilder NewNode(Type nodeType, Vector2 position, string name)
		{
			commands.Add(PWGraphCLI.GenerateNewNodeCommand(nodeType, name, position));
			return this;
		}

		public PWGraphBuilder Link(string from, string to)
		{
			commands.Add(PWGraphCLI.GenerateLinkCommand(from, to));
			return this;
		}

		public void Execute()
		{
			foreach (var cmd in commands)
				graph.Execute(cmd);
		}

		public List< string > GetCommands()
		{
			return commands;
		}
	
	}
}