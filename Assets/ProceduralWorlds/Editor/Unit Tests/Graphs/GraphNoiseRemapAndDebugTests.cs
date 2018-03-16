using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using ProceduralWorlds.Node;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Tests.Graphs
{
	public class BaseGraphNoiseRemapAndDebugTests
	{
	
		[Test]
		public static void BaseGraphNoiseRemapAndDebugNodes()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode(typeof(NodePerlinNoise2D), "perlin")
				.NewNode(typeof(NodeCurve), "curve")
				.NewNode(typeof(NodeDebugInfo), "debug")
				.Link("perlin", "curve")
				.Link("curve", "debug")
				.Execute()
				.GetGraph();
			
			var perlinNode = graph.FindNodeByName< NodePerlinNoise2D >("perlin");
			var curveNode = graph.FindNodeByName< NodeCurve >("curve");
			var debugNode = graph.FindNodeByName< NodeDebugInfo >("debug");

			//Assign a curve like this -> /\
			var curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(.5f, 1), new Keyframe(1, 0));
			curveNode.curve = curve;
			
			graph.Process();

			var result = debugNode.obj as Sampler2D;

			Assert.That(result != null, "The perlin nose sampler was not properly given to the debug node");

			//compare all resulting sampler values to the naive interpretation of the curve remap
			result.Foreach((x, y, val) => {
				var expected = curve.Evaluate(perlinNode.output[x, y]);
				Assert.That(val == expected, "Bad value in result of noise remaping, got " + val + " but " + expected + " was expected");
			});
		}
	}
}