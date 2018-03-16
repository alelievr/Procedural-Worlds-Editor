using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;

namespace ProceduralWorlds.Tests.Graphs
{
	public class BaseGraphParameterToNodesTests
	{

		static WorldGraph CreateTestGraph(out NodePerlinNoise2D perlinNode, out NodeDebugInfo debugNode)
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode(typeof(NodePerlinNoise2D), "perlin")
				.NewNode(typeof(NodeDebugInfo), "debug")
				.Link("perlin", "debug")
				.Execute()
				.GetGraph() as WorldGraph;
			
			perlinNode = graph.FindNodeByName< NodePerlinNoise2D >("perlin");
			debugNode = graph.FindNodeByName< NodeDebugInfo >("debug");

			graph.chunkSize = 64;
			graph.step = .5f;
			graph.chunkPosition = new Vector3(10, 42, -7);
			graph.seed = 123456789;

			return graph as WorldGraph;
		}
	
		[Test]
		public static void WorldGraphParameterToProcessedNodes()
		{
			NodePerlinNoise2D perlinNode;
			NodeDebugInfo		debugNode;
			var graph = CreateTestGraph(out perlinNode, out debugNode);

			graph.Process();

			Assert.That(perlinNode.output.size == graph.chunkSize, "Bad chunk size in perlin node after process");
			Assert.That(perlinNode.output.step == graph.step, "Bad step value in perlin node after process: expected " + graph.step + ", got: " + perlinNode.output.step);
		}
	
	}
}