using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using System.Collections.Generic;
using NUnit.Framework;
using System.Collections;
using PW.Core;
using PW.Node;

namespace PW.Tests.CLI
{
	public class BasicLexerParserTest
	{
	
		[Test]
		public void PerlinNoiseNodeToDebugNodeParsedCommands()
		{
			var builder = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode(typeof(PWNodePerlinNoise2D), "perlin")
				.NewNode(typeof(PWNodeDebugInfo), "debug")
				.Link("perlin", "debug");
	
			//list of the expected created commands
			List< PWGraphCommand > expectedCommands = new List< PWGraphCommand >()
			{
				new PWGraphCommand(typeof(PWNodePerlinNoise2D), "perlin"),
				new PWGraphCommand(typeof(PWNodeDebugInfo), "debug"),
				new PWGraphCommand("perlin", "debug"),
			};
	
			//get the commands as string
			var builderCommands = builder.GetCommands();
			
			for (int i = 0; i < expectedCommands.Count; i++)
			{
				//Parse the command and get the resulting command object
				PWGraphCommand cmd = PWGraphCLI.Parse(builderCommands[i]);
	
				Assert.That(cmd == expectedCommands[i]);
			}
		}

		[Test]
		public void SliderNodeToAddNodeWithAnchorLink()
		{
			var builder = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode< PWNodeSlider >("s1")
				.NewNode< PWNodeSlider >("s2")
				.NewNode< PWNodeSlider >("s3")
				.NewNode< PWNodeSlider >("s4")
				.NewNode< PWNodeAdd >("add")
				.Link("s1", "outValue", "add", "values")
				.Link("s2", "outValue", "add", "values")
				.Link("s3", 1, "add", 1)
				.Link("s4", 1, "add", 1);

			var expectedCommands = new List< PWGraphCommand >()
			{
				new PWGraphCommand(typeof(PWNodeSlider), "s1"),
				new PWGraphCommand(typeof(PWNodeSlider), "s2"),
				new PWGraphCommand(typeof(PWNodeSlider), "s3"),
				new PWGraphCommand(typeof(PWNodeSlider), "s4"),
				new PWGraphCommand(typeof(PWNodeAdd), "add"),
				new PWGraphCommand("s1", "outValue", "add", "values"),
				new PWGraphCommand("s2", "outValue", "add", "values"),
				new PWGraphCommand("s3", 1, "add", 1),
				new PWGraphCommand("s4", 1, "add", 1),
			};

			var commands = builder.GetCommands();
			
			for (int i = 0; i < expectedCommands.Count; i++)
			{
				//Parse the command and get the resulting command object
				PWGraphCommand cmd = PWGraphCLI.Parse(commands[i]);
	
				Assert.That(cmd == expectedCommands[i]);
			}
		}
	
	}
}