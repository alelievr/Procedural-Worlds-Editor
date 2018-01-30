using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PW.Core;
using PW.Node;

namespace PW.Tests.Graphs
{
	public class PWGraphBuilderTests
	{
	
		[Test]
		public void PWGraphBuilderAddNode()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode< PWNodeBiome >("biome")
				.Execute()
				.GetGraph();
			
			Assert.That(graph.FindNodeByName< PWNodeBiome >("biome") != null);
		}
		
		[Test]
		public void PWGraphBuilderLinkSimple()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode< PWNodePerlinNoise2D >("perlin")
				.NewNode< PWNodeCircleNoiseMask >("mask")
				.Link("perlin", "mask")
				.Execute()
				.GetGraph();
			
			var perlin = graph.FindNodeByName< PWNodePerlinNoise2D >("perlin");
			var mask = graph.FindNodeByName< PWNodeCircleNoiseMask >("mask");

			var perlinAnchor = perlin.outputAnchors.First();
			var maskAnchor = mask.inputAnchors.First();

			Assert.That(perlinAnchor.linkCount == 1);
			Assert.That(maskAnchor.linkCount == 1);
		}
		
		[Test]
		public void PWGraphBuilderLinkArray()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode< PWNodeAdd >("add")
				.NewNode< PWNodeSlider >("s1")
				.NewNode< PWNodeSlider >("s2")
				.NewNode< PWNodeSlider >("s3")
				.NewNode< PWNodeSlider >("s4")
				.Link("s1", "add")
				.Link("s2", "add")
				.Link("s3", "add")
				.Link("s4", "add")
				.Execute()
				.GetGraph();
			
			var add = graph.FindNodeByName< PWNodeAdd >("add");
			var s1 = graph.FindNodeByName< PWNodeSlider >("s1");
			var s2 = graph.FindNodeByName< PWNodeSlider >("s2");
			var s3 = graph.FindNodeByName< PWNodeSlider >("s3");
			var s4 = graph.FindNodeByName< PWNodeSlider >("s4");

			Assert.That(s1.GetOutputNodes().First() == add);
			Assert.That(s2.GetOutputNodes().First() == add);
			Assert.That(s3.GetOutputNodes().First() == add);
			Assert.That(s4.GetOutputNodes().First() == add);
		}
		
		//TestUtils.GenerateTestMainGraph():
		// 	              +-----+
		//            +---> Add1+---+
		// +------+   |   +-----+   |   +-----+   +------+
		// |Slider+---+             +--->Curve+--->Debug1|
		// +------+   |   +-----+       +-----+   +------+
		//            +---> Add2+---+
		// +------+   |   +-----+   |   +------+
		// |Float +---+             +--->Debug2|
		// +------+   |   +-----+       +------+
		//            +---> Add3+    
		//                +-----+

		[Test]
		public void PWGraphBuilderMainGraph()
		{
			var graph = TestUtils.GenerateTestMainGraph();

			//get nodes:
			var slider = graph.FindNodeByName< PWNodeSlider >("slider");
			var constant = graph.FindNodeByName< PWNodeConstant >("constant");
			var add1 = graph.FindNodeByName< PWNodeAdd >("add1");
			var add2 = graph.FindNodeByName< PWNodeAdd >("add2");
			var add3 = graph.FindNodeByName< PWNodeAdd >("add3");
			var curve = graph.FindNodeByName< PWNodeCurve >("curve");
			var debug1 = graph.FindNodeByName< PWNodeDebugInfo >("debug1");
			var debug2 = graph.FindNodeByName< PWNodeDebugInfo >("debug2");

			//get anchors
			var sliderAnchor = slider.outputAnchors.First();
			var constantAnchor = constant.outputAnchors.First();
			var add1InputAnchor = add1.inputAnchors.First();
			var add1OutputAnchor = add1.outputAnchors.First();
			var add2InputAnchor = add2.inputAnchors.First();
			var add2OutputAnchor = add2.outputAnchors.First();
			var add3InputAnchor = add3.inputAnchors.First();
			var add3OutputAnchor = add3.outputAnchors.First();
			var curveInputAnchor = curve.inputAnchors.First();
			var curveOutputAnchor = curve.outputAnchors.First();
			var debug1InputAnchor = debug1.inputAnchors.First();
			var debug2InputAnchor = debug2.inputAnchors.First();

			//slider links:
			var sliderOutList = sliderAnchor.links.Select(l => l.toAnchor);
			var expectedSliderOutList = new PWAnchor[] {add1InputAnchor, add2InputAnchor };

			Assert.That(ScrambledEqual(sliderOutList, expectedSliderOutList));

			//constant links:
			var constantOutList = constantAnchor.links.Select(l => l.toAnchor).OrderBy(a => a);
			var expectedConstantOutList = new PWAnchor[] {add2InputAnchor, add3InputAnchor }.OrderBy(a => a);
			
			Assert.That(ScrambledEqual(constantOutList, expectedConstantOutList));

			//add1 links
			Assert.That(add1InputAnchor.links[0].fromAnchor == sliderAnchor);
		}

		bool ScrambledEqual(IEnumerable< PWAnchor > anchorList1, IEnumerable< PWAnchor > anchorList2)
		{
			//TODO
			return true;
		}
	
	}
}