using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using System.Collections.Generic;
using NUnit.Framework;
using System.Collections;
using ProceduralWorlds.Core;
using ProceduralWorlds.Nodes;

namespace ProceduralWorlds.Tests.CLI
{
	public static class BasicLexerParserTest
	{
	
		[Test]
		public static void PerlinNoiseNodeToDebugNodeParsedCommands()
		{
			var builder = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode(typeof(NodePerlinNoise2D), "perlin")
				.NewNode(typeof(NodeDebugInfo), "debug")
				.Link("perlin", "debug");
	
			//list of the expected created commands
			List< BaseGraphCommand > expectedCommands = new List< BaseGraphCommand >
			{
				new BaseGraphCommand(typeof(NodePerlinNoise2D), "perlin"),
				new BaseGraphCommand(typeof(NodeDebugInfo), "debug"),
				new BaseGraphCommand("perlin", "debug"),
			};
	
			//get the commands as string
			var builderCommands = builder.GetCommands();
			
			for (int i = 0; i < expectedCommands.Count; i++)
			{
				//Parse the command and get the resulting command object
				BaseGraphCommand cmd = BaseGraphCLI.Parse(builderCommands[i]);
	
				Assert.That(cmd == expectedCommands[i]);
			}
		}

		[Test]
		public static void SliderNodeToAddNodeWithAnchorLink()
		{
			var builder = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode< NodeSlider >("s1")
				.NewNode< NodeSlider >("s2")
				.NewNode< NodeSlider >("s3")
				.NewNode< NodeSlider >("s4")
				.NewNode< NodeAdd >("add")
				.Link("s1", "outValue", "add", "values")
				.Link("s2", "outValue", "add", "values")
				.Link("s3", 1, "add", 1)
				.Link("s4", 1, "add", 1);

			var expectedCommands = new List< BaseGraphCommand >
			{
				new BaseGraphCommand(typeof(NodeSlider), "s1"),
				new BaseGraphCommand(typeof(NodeSlider), "s2"),
				new BaseGraphCommand(typeof(NodeSlider), "s3"),
				new BaseGraphCommand(typeof(NodeSlider), "s4"),
				new BaseGraphCommand(typeof(NodeAdd), "add"),
				new BaseGraphCommand("s1", "outValue", "add", "values"),
				new BaseGraphCommand("s2", "outValue", "add", "values"),
				new BaseGraphCommand("s3", 1, "add", 1),
				new BaseGraphCommand("s4", 1, "add", 1),
			};

			var commands = builder.GetCommands();
			
			for (int i = 0; i < expectedCommands.Count; i++)
			{
				//Parse the command and get the resulting command object
				BaseGraphCommand cmd = BaseGraphCLI.Parse(commands[i]);
	
				Assert.That(cmd == expectedCommands[i]);
			}
		}
	
	}
}