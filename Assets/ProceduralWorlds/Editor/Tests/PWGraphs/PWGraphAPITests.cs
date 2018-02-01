using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using PW.Core;
using PW.Node;

namespace PW.Tests.Graphs
{
	public class PWGraphAPITests
	{
	
		[Test]
		public void PWGraphCreateNewNode()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >().GetGraph();

			var add = graph.CreateNewNode(typeof(PWNodeAdd), new Vector2(42, -21), "add !");

			Assert.That(graph.nodes.Find(n => n == add) != null);
			Assert.That(add.rect.position == new Vector2(42, -21));
			Assert.That(add.name == "add !");
		}
	
		[Test]
		public void PWGraphCreateNewNodeEventDisabled()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >().GetGraph();

			var add = graph.CreateNewNode(typeof(PWNodeAdd), new Vector2(42, -21), "add !", false);

			Assert.That(graph.nodes.Find(n => n == add) != null);
			Assert.That(add.rect.position == new Vector2(42, -21));
			Assert.That(add.name == "add !");
		}

		[Test]
		public void PWGraphCreateLink()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >().GetGraph();

			var slider = graph.CreateNewNode(typeof(PWNodeSlider), Vector2.zero);
			var add = graph.CreateNewNode(typeof(PWNodeAdd), Vector2.zero);

			var sliderOut = slider.outputAnchors.First();
			var addIn = add.inputAnchors.First();

			var link = graph.CreateLink(sliderOut, addIn);

			Assert.That(sliderOut.linkCount == 1);
			Assert.That(addIn.linkCount == 1);

			Assert.That(link == sliderOut.links.First());
			Assert.That(link == addIn.links.First());

			Assert.That(link.fromNode == slider);
			Assert.That(link.toNode == add);
		}

		[Test]
		public void PWGraphCreateLinkEventDisabled()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >().GetGraph();

			var slider = graph.CreateNewNode(typeof(PWNodeSlider), Vector2.zero);
			var add = graph.CreateNewNode(typeof(PWNodeAdd), Vector2.zero);

			var sliderOut = slider.outputAnchors.First();
			var addIn = add.inputAnchors.First();

			var link = graph.CreateLink(sliderOut, addIn, false);

			Assert.That(sliderOut.linkCount == 1);
			Assert.That(addIn.linkCount == 1);

			Assert.That(link == sliderOut.links.First());
			Assert.That(link == addIn.links.First());

			Assert.That(link.fromNode == slider);
			Assert.That(link.toNode == add);
		}

		//TODO: test for SafeCreateLink
		
		[Test]
		public void FindNodeByName()
		{
			const string addNodeName = "add";
			const string colorNodeName = "c_o_l_o_r";
			const string textureNodeName = "   this is a texture ! ";

			var graph = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode(typeof(PWNodeAdd), addNodeName)
				.NewNode(typeof(PWNodeColor), colorNodeName)
				.NewNode(typeof(PWNodeTexture2D), textureNodeName)
				.Execute()
				.GetGraph();
			
			Assert.That(graph.FindNodeByName< PWNodeAdd >(addNodeName) != null, "add node was not found in the graph");
			Assert.That(graph.FindNodeByName< PWNodeColor >(colorNodeName) != null, "Color node was not found in the graph");
			Assert.That(graph.FindNodeByName< PWNodeTexture2D >(textureNodeName) != null, "Texture2D node was not found in the graph");
		}

		[Test]
		public void FindNodeByNameNotFound()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >().Execute().GetGraph();

			Assert.That(graph.FindNodeByName("inhexistant node") == null);
		}

		[Test]
		public void RemoveNode()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode< PWNodeSlider >("slider")
				.Execute()
				.GetGraph();
			
			var slider = graph.FindNodeByName("slider");

			graph.RemoveNode(slider);

			Assert.That(graph.nodes.Count == 2);
			Assert.That(graph.FindNodeByName("slider") == null);
		}

		[Test]
		public void RemoveNodeWithoutEvents()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode< PWNodeSlider >("slider")
				.Execute()
				.GetGraph();
			
			var slider = graph.FindNodeByName("slider");

			graph.RemoveNode(slider, false);

			Assert.That(graph.nodes.Count == 2);
			Assert.That(graph.FindNodeByName("slider") == null);
		}
		
		//test main graph
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
		public void RemoveNodeLinks()
		{
			var graph = TestUtils.GenerateTestMainGraph();

			var add2 = graph.FindNodeByName("add2");

			graph.RemoveNode(add2);

			RemoveNodeLinkAsserts(graph);
		}
		
		[Test]
		public void RemoveNodeLinksWithoutEvents()
		{
			var graph = TestUtils.GenerateTestMainGraph();

			var add2 = graph.FindNodeByName("add2");

			graph.RemoveNode(add2, false);

			RemoveNodeLinkAsserts(graph);
		}

		void RemoveNodeLinkAsserts(PWGraph graph)
		{
			var add1 = graph.FindNodeByName("add1");
			var add2 = graph.FindNodeByName("add2");
			var add3 = graph.FindNodeByName("add3");
			var slider = graph.FindNodeByName("slider");
			var constant = graph.FindNodeByName("constant");
			var debug2 = graph.FindNodeByName("debug2");

			Assert.That(slider.GetOutputLinks().Count() == 1);
			Assert.That(slider.GetOutputLinks().First().toNode == add1);
			Assert.That(constant.GetOutputLinks().Count() == 1);
			Assert.That(constant.GetOutputLinks().First().toNode == add3);
			Assert.That(debug2.GetInputLinks().Count() == 0);

			foreach (var link in graph.nodeLinkTable.GetLinks())
			{
				Assert.That(link.fromNode != add2);
				Assert.That(link.toNode != add2);
			}
		}
	}
}