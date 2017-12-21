﻿using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Core;
using PW.Node;
using System.Linq;

namespace PW
{
	public class BasicExecuterTest
	{
	
		[Test]
		public void PerlinNoiseNodeToDebugNodeExecution()
		{
			string	perlinNodeName = "perlin";
			string	debugNodeName = "debug";
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode< PWNodePerlinNoise2D >(perlinNodeName)
				.NewNode< PWNodeDebugLog >(debugNodeName)
				.Link(perlinNodeName, debugNodeName)
				.Execute()
				.GetGraph();
			
			PWNodePerlinNoise2D perlinNode = graph.FindNodeByName(perlinNodeName) as PWNodePerlinNoise2D;
			PWNodeDebugLog debugNode = graph.FindNodeByName(debugNodeName) as PWNodeDebugLog;
	
			Assert.That(perlinNode != null, "Perlin node not found in the graph (using FindNodeByName)");
			Assert.That(debugNode != null, "Debug node not found in the graph (using FindNodeByName)");
	
			PWNodeLink link = perlinNode.GetOutputLinks().First();
	
			Assert.That(link != null, "Link can't be found in the graph");
			Assert.That(link.toNode == debugNode);
		}
	
	}
}