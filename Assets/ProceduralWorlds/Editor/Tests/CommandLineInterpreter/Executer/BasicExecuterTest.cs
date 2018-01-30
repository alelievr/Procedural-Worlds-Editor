using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Core;
using PW.Node;
using System.Linq;

namespace PW.Tests.CLI
{
	public class BasicExecuterTest
	{
	
		[Test]
		public void PerlinNoiseToDebugNodeExecution()
		{
			string	perlinNodeName = "perlin";
			string	debugNodeName = "debug";
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode< PWNodePerlinNoise2D >(perlinNodeName)
				.NewNode< PWNodeDebugInfo >(debugNodeName)
				.Link(perlinNodeName, debugNodeName)
				.Execute()
				.GetGraph();
			
			PWNodePerlinNoise2D perlinNode = graph.FindNodeByName(perlinNodeName) as PWNodePerlinNoise2D;
			PWNodeDebugInfo debugNode = graph.FindNodeByName(debugNodeName) as PWNodeDebugInfo;
	
			Assert.That(perlinNode != null, "Perlin node not found in the graph (using FindNodeByName)");
			Assert.That(debugNode != null, "Debug node not found in the graph (using FindNodeByName)");
	
			PWNodeLink link = perlinNode.GetOutputLinks().First();
	
			Assert.That(link != null, "Link can't be found in the graph");
			Assert.That(link.toNode == debugNode);
		}

		[Test]
		public void PerlinNoiseWithAttributesToDebugNodeExecution()
		{
			string	perlinNodeName = "perlin";
			var perlinAttributes = new PWGraphCLIAttributes() {
				{"persistance", 2.4f}, {"octaves", 6}
			};

			var graph = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode< PWNodePerlinNoise2D >(perlinNodeName, perlinAttributes)
				.Execute()
				.GetGraph();
			
			PWNodePerlinNoise2D perlinNode = graph.FindNodeByName(perlinNodeName) as PWNodePerlinNoise2D;

			Assert.That(perlinNode.octaves == 6, "Perlin node octaves expected to be 6 but was " + perlinNode.octaves);
			Assert.That(perlinNode.persistance == 2.4f, "Perlin node persistance expected to be 2.4 but was " + perlinNode.persistance);
		}

		[Test]
		public void EmptyGraph()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >().Execute().GetGraph();

			Assert.That(graph != null, "Null graph !");
			Assert.That(graph.inputNode != null, "Null graph input node while creating empty graph");
			Assert.That(graph.outputNode != null, "Null graph output node while creating empty graph");
		}
	
	}
}