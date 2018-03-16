using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Tests.Graphs
{
	public class BaseGraphLinkProcessingTests
	{
	
		[Test]
		public static void BaseGraphLinkProcessSimple()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode< NodeSlider >("slider")
				.NewNode< NodeDebugInfo >("debug")
				.Link("slider", "debug")
				.Execute()
				.GetGraph();
			
			var slider = graph.FindNodeByName< NodeSlider >("slider");
			var debug = graph.FindNodeByName< NodeDebugInfo >("debug");

			slider.outValue = 0.42f;

			graph.name = "MyGraph !";

			graph.Process();

			Assert.That(debug.obj.Equals(0.42f));
		}

		//Test biome graph
		// +----+      +----+
		// | c1 +------> s1 +----+
		// +----+      +----+    |
		//                       |
		// +----+      +----+    |  +------+
		// | c2 +------> s2 +-------> surf |
		// +----+      +----+    |  +------+
		//                       |
		// +----+      +----+    |
		// | c3 +------> s3 +----+
		// +----+      +----+
		//c*: NodeBiomeSurfaceColor, s*: NodeBiomeSurfaceSwitch, surf: NodeBiomeSurface


		[Test]
		public static void BaseGraphLinkProcessOnceArray()
		{
			var graph = TestUtils.GenerateTestBiomeGraph();
			
			var c1 = graph.FindNodeByName< NodeBiomeSurfaceColor >("c1");
			var c2 = graph.FindNodeByName< NodeBiomeSurfaceColor >("c2");
			var c3 = graph.FindNodeByName< NodeBiomeSurfaceColor >("c3");
			var surf = graph.FindNodeByName< NodeBiomeSurface >("surf");

			c1.surfaceColor.baseColor = Color.red;
			c2.surfaceColor.baseColor = Color.green;
			c3.surfaceColor.baseColor = Color.yellow;

			graph.ProcessOnce();

			Assert.That(surf.surfaceGraph.cells.Count == 3);
		}
		
		[Test]
		public static void BaseGraphLinkArrayToArray()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >().GetGraph();

			var input = graph.FindNodeByType< NodeWorldGraphInput >();
			var output = graph.FindNodeByType< NodeWorldGraphOutput >();

			//create 5 links from first input node anchor to [1..5] output anchors
			for (int i = 0; i < 5; i++)
			{
				var inputAnchor = input.outputAnchors.Last();
				var outputAnchor = output.inputAnchors.Last();

				graph.CreateLink(inputAnchor, outputAnchor);
			}

			Assert.That(input.outputAnchors.Count() == 1, "input node output anchors count: " + input.outputAnchors.Count() + ", expected to be 1");
			Assert.That(output.inputAnchors.Count() == 7, "output node input anchors count: " + output.inputAnchors.Count() + ", expected to be 7");

			var inputLinks = input.GetOutputLinks().ToList();
			var outputLinks = output.GetInputLinks().ToList();

			Assert.That(inputLinks.Count == 5);
			Assert.That(outputLinks.Count == 5);

			Assert.That(inputLinks[0].toAnchor != inputLinks[1].toAnchor);
			Assert.That(outputLinks[0].toAnchor != outputLinks[1].toAnchor);
		}
		
		[Test]
		public static void BaseGraphLinkArrayToArrayProcess()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >().GetGraph();

			var input = graph.FindNodeByType< NodeWorldGraphInput >();
			var output = graph.FindNodeByType< NodeWorldGraphOutput >();

			input.SetMultiAnchor("outputValues", 5);

			var inputAnchors = input.outputAnchors.ToList();

			//Link input i anchor to output i anchor
			for (int i = 0; i < 5; i++)
			{
				var inputAnchor = inputAnchors[i];
				var outputAnchor = output.inputAnchors.Last();

				graph.CreateLink(inputAnchor, outputAnchor);
			}

			int value = 42;

			foreach (var anchor in input.outputAnchors)
				input.SetAnchorValue(anchor, value++);

			graph.Process();

			value = 42;
			
			var af = output.inputAnchorFields.FirstOrDefault(a => a.multiple);

			for (int i = 0; i < 5; i++)
			{
				var anchor = af.anchors[i];
				var val = output.GetAnchorValue(anchor);
				Assert.That(val.Equals(value++));
			}
		}
	
	}
}