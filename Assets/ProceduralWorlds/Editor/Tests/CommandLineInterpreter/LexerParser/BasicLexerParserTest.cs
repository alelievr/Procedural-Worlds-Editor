using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using System.Collections.Generic;
using NUnit.Framework;
using System.Collections;
using PW.Core;
using PW.Node;

public class BasicLexerParserTest
{

	[Test]
	public void BasicPerlinNoiseDebug()
	{
		var builder = PWGraphBuilder.NewGraph< PWMainGraph >()
			.NewNode(typeof(PWNodePerlinNoise2D), "perlin")
			.NewNode(typeof(PWNodeDebugLog), "debug")
			.Link("perlin", "debug");

		//list of the expected created commands
		List< PWGraphCommand > expectedCommands = new List< PWGraphCommand >()
		{
			new PWGraphCommand(typeof(PWNodePerlinNoise2D), "perlin"),
			new PWGraphCommand(typeof(PWNodeDebugLog), "debug"),
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
}
