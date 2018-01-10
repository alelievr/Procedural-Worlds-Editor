using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Node;
using PW.Core;

namespace PW
{
	public class PWGraphNoiseRemapAndDebugTests
	{
	
		[Test]
		public void PWGraphNoiseRemapAndDebugNodes()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode(typeof(PWNodePerlinNoise2D), "perlin")
				.NewNode(typeof(PWNodeCurve), "curve")
				.NewNode(typeof(PWNodeDebugLog), "debug")
				.Link("perlin", "curve")
				.Link("curve", "debug")
				.Execute()
				.GetGraph();
			
			var perlinNode = graph.FindNodeByName< PWNodePerlinNoise2D >("perlin");
			var curveNode = graph.FindNodeByName< PWNodeCurve >("curve");
			var debugNode = graph.FindNodeByName< PWNodeDebugLog >("debug");

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