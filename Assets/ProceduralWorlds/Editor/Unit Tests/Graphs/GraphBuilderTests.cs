using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;

namespace ProceduralWorlds.Tests.Graphs
{
	public static class GraphBuilderTests
	{
	
		[Test]
		public static void GraphBuilderAddNode()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode< NodeBiome >("biome")
				.Execute()
				.GetGraph();
			
			Assert.That(graph.FindNodeByName< NodeBiome >("biome") != null);
		}
		
		[Test]
		public static void GraphBuilderLinkSimple()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode< NodePerlinNoise2D >("perlin")
				.NewNode< NodeCircleNoiseMask >("mask")
				.Link("perlin", "mask")
				.Execute()
				.GetGraph();

			var perlin = graph.FindNodeByName< NodePerlinNoise2D >("perlin");
			var mask = graph.FindNodeByName< NodeCircleNoiseMask >("mask");

			var perlinAnchor = perlin.outputAnchors.First();
			var maskAnchor = mask.inputAnchors.First();

			Assert.That(perlinAnchor.linkCount == 1);
			Assert.That(maskAnchor.linkCount == 1);
		}

		[Test]
		public static void GraphBuilderBiomeGraph()
		{
			var graph = GraphBuilder.NewGraph< BiomeGraph >()
				.NewNode< NodeBiomeSurfaceColor >("c")
				.NewNode< NodeBiomeSurfaceSwitch >("s")
				.NewNode< NodeBiomeSurface >("surf")
				.Link("s" ,"surf")
				.Link("c", "s")
				.Execute()
				.GetGraph();

			var color = graph.FindNodeByName("c");
			var surfSwitch = graph.FindNodeByName("s");
			var surf = graph.FindNodeByName("surf");

			Assert.That(color.GetOutputLinks().Count() == 1);
			Assert.That(surfSwitch.GetInputLinks().Count() == 1);
			Assert.That(surfSwitch.GetOutputLinks().Count() == 1);
			Assert.That(surf.GetInputLinks().Count() == 1);

			var colorLink = color.GetOutputLinks().First();
			var surfSwitchLink = surfSwitch.GetOutputLinks().First();

			Assert.That(colorLink.fromNode == color);
			Assert.That(colorLink.toNode == surfSwitch);
			Assert.That(surfSwitchLink.fromNode == surfSwitch);
			Assert.That(surfSwitchLink.toNode == surf);
		}
		
		[Test]
		public static void GraphBuilderLinkArray()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode< NodeAdd >("add")
				.NewNode< NodeSlider >("s1")
				.NewNode< NodeSlider >("s2")
				.NewNode< NodeSlider >("s3")
				.NewNode< NodeSlider >("s4")
				.Link("s1", "add")
				.Link("s2", "add")
				.Link("s3", "add")
				.Link("s4", "add")
				.Execute()
				.GetGraph();
			
			var add = graph.FindNodeByName< NodeAdd >("add");
			var s1 = graph.FindNodeByName< NodeSlider >("s1");
			var s2 = graph.FindNodeByName< NodeSlider >("s2");
			var s3 = graph.FindNodeByName< NodeSlider >("s3");
			var s4 = graph.FindNodeByName< NodeSlider >("s4");

			Assert.That(s1.GetOutputNodes().First() == add);
			Assert.That(s2.GetOutputNodes().First() == add);
			Assert.That(s3.GetOutputNodes().First() == add);
			Assert.That(s4.GetOutputNodes().First() == add);

			var s1a = s1.outputAnchors.First().links.First().toAnchor;
			var s2a = s2.outputAnchors.First().links.First().toAnchor;
			var s3a = s3.outputAnchors.First().links.First().toAnchor;
			var s4a = s4.outputAnchors.First().links.First().toAnchor;

			//check that links are not on the same input anchor:
			Assert.That(s1a != s2a);
			Assert.That(s2a != s3a);
			Assert.That(s3a != s4a);
		}
		
		//TestUtils.GenerateTestWorldGraph():
		// 	              +-----+
		//            +---> Add1+---+
		// +------+   |   +-----+   |   +-----+   +------+
		// |Slider+---+             +---> Add4+--->Debug1|
		// +------+   |   +-----+       +-----+   +------+
		//            +---> Add2+---+
		// +------+   |   +-----+   |   +------+
		// |Float +---+             +--->Debug2|
		// +------+   |   +-----+       +------+
		//            +---> Add3+    
		//                +-----+

		[Test]
		public static void GraphBuilderWorldGraph()
		{
			var graph = TestUtils.GenerateTestWorldGraph();

			//get nodes:
			var slider = graph.FindNodeByName< NodeSlider >("slider");
			var constant = graph.FindNodeByName< NodeConstant >("constant");
			var add1 = graph.FindNodeByName< NodeAdd >("add1");
			var add2 = graph.FindNodeByName< NodeAdd >("add2");
			var add3 = graph.FindNodeByName< NodeAdd >("add3");
			var add4 = graph.FindNodeByName< NodeAdd >("add4");
			var debug1 = graph.FindNodeByName< NodeDebugInfo >("debug1");
			var debug2 = graph.FindNodeByName< NodeDebugInfo >("debug2");

			//get anchors
			var sliderAnchor = slider.outputAnchors.First();
			var constantAnchor = constant.outputAnchors.FirstOrDefault(a => a.visibility == Visibility.Visible);
			var add1InputAnchor = add1.inputAnchors.First();
			var add1OutputAnchor = add1.outputAnchors.First();
			var add2InputAnchor = add2.inputAnchors.First();
			var add2OutputAnchor = add2.outputAnchors.First();
			var add3InputAnchor = add3.inputAnchors.First();
			var add4InputAnchor = add4.inputAnchors.First();
			var add4OutputAnchor = add4.outputAnchors.First();
			var debug1InputAnchor = debug1.inputAnchors.First();
			var debug2InputAnchor = debug2.inputAnchors.First();
			
			var add2InputAnchors = add2.inputAnchorFields[0].anchors;

			//slider links:
			var sliderOutList = sliderAnchor.links.Select(l => l.toAnchor).ToList();
			var expectedSliderOutList = new Anchor[] { add1InputAnchor, add2InputAnchor }.ToList();

			Assert.That(ScrambledEqual(sliderOutList, expectedSliderOutList));

			//constant links:
			var constantOutList = constantAnchor.links.Select(l => l.toAnchor).ToList();
			var expectedConstantOutList = add2.inputAnchors.Concat(add3.inputAnchors).ToList();

			Assert.That(ScrambledEqual(constantOutList, expectedConstantOutList));

			//add1 links:
			Assert.That(add1InputAnchor.linkCount == 1);
			Assert.That(add1OutputAnchor.linkCount == 1);
			Assert.That(add1InputAnchor.links[0].fromAnchor == sliderAnchor);
			Assert.That(add1OutputAnchor.links[0].toAnchor == add4InputAnchor);
			
			//add2 links:
			Assert.That(add2OutputAnchor.linkCount == 1);
			Assert.That(add2OutputAnchor.links[0].toAnchor == debug2InputAnchor);
			Assert.That(add2InputAnchor.links.All(l => l.toNode == add2));
			Assert.That(add2InputAnchors.Count == 3);
			Assert.That(add2InputAnchors[0].linkCount == 1);
			Assert.That(add2InputAnchors[1].linkCount == 1);
			Assert.That(add2InputAnchors[2].linkCount == 0);

			//add3 links:
			Assert.That(add3InputAnchor.linkCount == 1);
			Assert.That(add3InputAnchor.links[0].fromAnchor == constantAnchor);

			//add4 links:
			Assert.That(add4InputAnchor.linkCount == 1);
			Assert.That(add4OutputAnchor.linkCount == 1);
			Assert.That(add4OutputAnchor.links[0].toAnchor == debug1InputAnchor);
			Assert.That(add4InputAnchor.links[0].fromAnchor == add1OutputAnchor);

			//debug1 links:
			Assert.That(debug1InputAnchor.linkCount == 1);
			Assert.That(debug1InputAnchor.links[0].fromAnchor == add4OutputAnchor);

			//debug2 links:
			Assert.That(debug2InputAnchor.linkCount == 1);
			Assert.That(debug2InputAnchor.links[0].fromAnchor == add2OutputAnchor);
		}

		static bool ScrambledEqual(IEnumerable< Anchor > anchorList1, IEnumerable< Anchor > anchorList2)
		{
			return anchorList1.All(a1 => anchorList2.Contains(a1));
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
		public static void GraphBuilderBiomeLinkArray()
		{
			var graph = TestUtils.GenerateTestBiomeGraph();
			
			var s1 = graph.FindNodeByName("s1");
			var s2 = graph.FindNodeByName("s2");
			var s3 = graph.FindNodeByName("s3");
			var surf = graph.FindNodeByName("surf");

			var surfinputAnchorField = surf.inputAnchorFields.First();

			var expectedConnectedAnchors = new List< Anchor >()
			{
				s1.outputAnchors.First(),
				s2.outputAnchors.First(),
				s3.outputAnchors.First(),
			};

			var surfConnectedAnchors =	from anchor in surfinputAnchorField.anchors
										from links in anchor.links
										select links.fromAnchor;

			Assert.That(surfinputAnchorField.anchors.Count == 4);
			Assert.That(ScrambledEqual(surfConnectedAnchors, expectedConnectedAnchors));
		}
	
	}
}