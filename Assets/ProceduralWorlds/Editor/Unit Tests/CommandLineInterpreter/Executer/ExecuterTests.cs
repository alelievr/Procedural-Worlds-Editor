using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;
using System.Linq;

namespace ProceduralWorlds.Tests.CLI
{
	public class ExecuterTests
	{
	
		[Test]
		public static void PerlinNoiseToDebugNodeExecution()
		{
			string	perlinNodeName = "perlin";
			string	debugNodeName = "debug";
			var graph = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode< NodePerlinNoise2D >(perlinNodeName)
				.NewNode< NodeDebugInfo >(debugNodeName)
				.Link(perlinNodeName, debugNodeName)
				.Execute()
				.GetGraph();
			
			NodePerlinNoise2D perlinNode = graph.FindNodeByName(perlinNodeName) as NodePerlinNoise2D;
			NodeDebugInfo debugNode = graph.FindNodeByName(debugNodeName) as NodeDebugInfo;
	
			Assert.That(perlinNode != null, "Perlin node not found in the graph (using FindNodeByName)");
			Assert.That(debugNode != null, "Debug node not found in the graph (using FindNodeByName)");
	
			NodeLink link = perlinNode.GetOutputLinks().First();
	
			Assert.That(link != null, "Link can't be found in the graph");
			Assert.That(link.toNode == debugNode);
		}

		[Test]
		public static void PerlinNoiseWithAttributesToDebugNodeExecution()
		{
			string	perlinNodeName = "perlin";
			var perlinAttributes = new BaseGraphCLIAttributes() {
				{"persistence", 2.4f}, {"octaves", 6}
			};

			var graph = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode< NodePerlinNoise2D >(perlinNodeName, perlinAttributes)
				.Execute()
				.GetGraph();
			
			NodePerlinNoise2D perlinNode = graph.FindNodeByName(perlinNodeName) as NodePerlinNoise2D;

			Assert.That(perlinNode.octaves == 6, "Perlin node octaves expected to be 6 but was " + perlinNode.octaves);
			Assert.That(perlinNode.persistence == 2.4f, "Perlin node persistence expected to be 2.4 but was " + perlinNode.persistence);
		}

		[Test]
		public static void EmptyGraph()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >().Execute().GetGraph();

			Assert.That(graph != null, "Null graph !");
			Assert.That(graph.inputNode != null, "Null graph input node while creating empty graph");
			Assert.That(graph.outputNode != null, "Null graph output node while creating empty graph");
		}

		[Test]
		public static void SliderNodeAnchorLinkedToAddNodeExecution()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode< NodeSlider >("s1")
				.NewNode< NodeSlider >("s2")
				.NewNode< NodeSlider >("s3")
				.NewNode< NodeSlider >("s4")
				.NewNode< NodeAdd >("add")
				.Link("s1", "outValue", "add", "values")
				.Link("s2", "outValue", "add", "values")
				.Link("s3", 0, "add", 0)
				.Link("s4", 0, "add", 0)
				.Execute()
				.GetGraph();
			
			var s1 = graph.FindNodeByName("s1");
			var s2 = graph.FindNodeByName("s2");
			var s3 = graph.FindNodeByName("s3");
			var s4 = graph.FindNodeByName("s4");

			var s1Link = s1.GetOutputLinks().First();
			var s2Link = s2.GetOutputLinks().First();
			var s3Link = s3.GetOutputLinks().First();
			var s4Link = s4.GetOutputLinks().First();

			//check if the links haven't been linked to the same anchor
			Assert.That(s1Link.toAnchor != s2Link.toAnchor);
			Assert.That(s2Link.toAnchor != s3Link.toAnchor);
			Assert.That(s3Link.toAnchor != s4Link.toAnchor);
			Assert.That(s4Link.toAnchor != s1Link.toAnchor);
		}

		[Test]
		public static void GraphAttributeCommandFieldExecution()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >().GetGraph() as WorldGraph;

			graph.Execute(BaseGraphCLI.GenerateGraphAttributeCommand("scaledPreviewEnabled", true));

			Assert.That(graph.scaledPreviewEnabled == true);
		}
		
		[Test]
		public static void GraphAttributeCommandPropertyExecution()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >().GetGraph() as WorldGraph;

			graph.Execute(BaseGraphCLI.GenerateGraphAttributeCommand("_chunkSize", 42));

			Assert.That(graph.chunkSize == 42);
		}
	
	}
}