using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Core;
using PW.Node;

namespace PW.Tests.Graphs
{
	public class PWGraphParameterToNodesTests
	{

		PWMainGraph CreateTestGraph(out PWNodePerlinNoise2D perlinNode, out PWNodeDebugInfo debugNode)
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode(typeof(PWNodePerlinNoise2D), "perlin")
				.NewNode(typeof(PWNodeDebugInfo), "debug")
				.Link("perlin", "debug")
				.Execute()
				.GetGraph();
			
			perlinNode = graph.FindNodeByName< PWNodePerlinNoise2D >("perlin");
			debugNode = graph.FindNodeByName< PWNodeDebugInfo >("debug");

			graph.chunkSize = 64;
			graph.step = .5f;
			graph.chunkPosition = new Vector3(10, 42, -7);
			graph.seed = 123456789;

			return graph as PWMainGraph;
		}
	
		[Test]
		public void PWMainGraphParameterToProcessedNodes()
		{
			PWNodePerlinNoise2D perlinNode;
			PWNodeDebugInfo		debugNode;
			var graph = CreateTestGraph(out perlinNode, out debugNode);

			graph.Process();

			Assert.That(perlinNode.output.size == graph.chunkSize, "Bad chunk size in perlin node after process");
			Assert.That(perlinNode.output.step == graph.step, "Bad step value in perlin node after process");
			//TODO: add seed
		}

		//TODO: add a test which test the final terrain datas
	
	}
}