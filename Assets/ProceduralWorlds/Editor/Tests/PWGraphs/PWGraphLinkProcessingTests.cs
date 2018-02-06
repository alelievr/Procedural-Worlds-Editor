using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using PW.Core;
using PW.Node;
using PW.Biomator;

namespace PW.Tests.Graphs
{
	public class PWGraphLinkProcessingTests
	{
	
		[Test]
		public void PWGraphLinkProcessSimple()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode< PWNodeSlider >("slider")
				.NewNode< PWNodeDebugInfo >("debug")
				.Link("slider", "debug")
				.Execute()
				.GetGraph();
			
			var slider = graph.FindNodeByName< PWNodeSlider >("slider");
			var debug = graph.FindNodeByName< PWNodeDebugInfo >("debug");

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
		//c*: PWNodeBiomeSurfaceColor, s*: PWNodeBiomeSurfaceSwitch, surf: PWNodeBiomeSurface


		[Test]
		public void PWGraphLinkProcessOnceArray()
		{
			var graph = TestUtils.GenerateTestBiomeGraph();
			
			var c1 = graph.FindNodeByName< PWNodeBiomeSurfaceColor >("c1");
			var c2 = graph.FindNodeByName< PWNodeBiomeSurfaceColor >("c2");
			var c3 = graph.FindNodeByName< PWNodeBiomeSurfaceColor >("c3");
			var surf = graph.FindNodeByName< PWNodeBiomeSurface >("surf");

			c1.surfaceColor.baseColor = Color.red;
			c2.surfaceColor.baseColor = Color.green;
			c3.surfaceColor.baseColor = Color.yellow;

			graph.ProcessOnce();

			Assert.That(surf.surfaceGraph.cells.Count == 3);
		}
		
		[Test]
		public void PWGraphLinkArrayToArray()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >().GetGraph();

			var input = graph.FindNodeByType< PWNodeGraphInput >();
			var output = graph.FindNodeByType< PWNodeGraphOutput >();

			//create 5 links from first input node anchor to [1..5] output anchors
			for (int i = 0; i < 5; i++)
			{
				var inputAnchor = input.outputAnchors.Last();
				var outputAnchor = output.inputAnchors.Last();

				graph.CreateLink(inputAnchor, outputAnchor);
			}

			Assert.That(input.outputAnchors.Count() == 1, "input node output anchors count: " + input.outputAnchors.Count() + ", expected to be 1");
			Assert.That(output.inputAnchors.Count() == 6, "output node input anchors count: " + output.inputAnchors.Count() + ", expected to be 6");

			var inputLinks = input.GetOutputLinks().ToList();
			var outputLinks = output.GetInputLinks().ToList();

			Assert.That(inputLinks.Count == 5);
			Assert.That(outputLinks.Count == 5);

			Assert.That(inputLinks[0].toAnchor != inputLinks[1].toAnchor);
			Assert.That(outputLinks[0].toAnchor != outputLinks[1].toAnchor);
		}
		
		[Test]
		public void PWGraphLinkArrayToArrayProcess()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >().GetGraph();

			var input = graph.FindNodeByType< PWNodeGraphInput >();
			var output = graph.FindNodeByType< PWNodeGraphOutput >();

			var outputAnchorField = input.outputAnchorFields.First();

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
			
			var outputAnchors = output.inputAnchors.ToList();

			for (int i = 0; i < 5; i++)
			{
				var anchor = outputAnchors[i];
				var val = output.GetAnchorValue(anchor);
				Assert.That(val.Equals(value++));
			}
		}
	
	}
}