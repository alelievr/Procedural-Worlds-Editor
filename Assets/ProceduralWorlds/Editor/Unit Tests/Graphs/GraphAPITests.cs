using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using ProceduralWorlds.Core;
using ProceduralWorlds.Nodes;

namespace ProceduralWorlds.Tests.Graphs
{
	public static class BaseGraphAPITests
	{
	
		[Test]
		public static void BaseGraphCreateNewNodeTest()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >().GetGraph();

			var add = graph.CreateNewNode(typeof(NodeAdd), new Vector2(42, -21), "add !");

			Assert.That(graph.nodes.Find(n => n == add) != null);
			Assert.That(add.rect.position == new Vector2(42, -21));
			Assert.That(add.name == "add !");
		}
	
		[Test]
		public static void BaseGraphCreateNewNodeEventDisabledTest()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >().GetGraph();

			var add = graph.CreateNewNode(typeof(NodeAdd), new Vector2(42, -21), "add !", false);

			Assert.That(graph.nodes.Find(n => n == add) != null);
			Assert.That(add.rect.position == new Vector2(42, -21));
			Assert.That(add.name == "add !");
		}

		[Test]
		public static void BaseGraphCreateLinkTest()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >().GetGraph();

			var slider = graph.CreateNewNode(typeof(NodeSlider), Vector2.zero);
			var add = graph.CreateNewNode(typeof(NodeAdd), Vector2.zero);

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
		public static void BaseGraphCreateLinkEventDisabledTest()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >().GetGraph();

			var slider = graph.CreateNewNode(typeof(NodeSlider), Vector2.zero);
			var add = graph.CreateNewNode(typeof(NodeAdd), Vector2.zero);

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

		[Test]
		public static void BaseGraphSafeCreateLinkDuplicateTest()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode< NodeSlider >("slider")
				.NewNode< NodeDebugInfo >("debug")
				.Execute()
				.GetGraph();
			
			var sliderAnchor = graph.FindNodeByName("slider").outputAnchors.First();
			var debugAnchor = graph.FindNodeByName("debug").inputAnchors.First();

			graph.SafeCreateLink(sliderAnchor, debugAnchor);
			graph.SafeCreateLink(sliderAnchor, debugAnchor);

			Assert.That(sliderAnchor.linkCount == 1);
			Assert.That(debugAnchor.linkCount == 1);
			Assert.That(graph.nodeLinkTable.GetLinks().Count() == 1);
		}
		
		[Test]
		public static void FindNodeByNameTest()
		{
			const string addNodeName = "add";
			const string colorNodeName = "c_o_l_o_r";
			const string textureNodeName = "   this is a texture ! ";

			var graph = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode(typeof(NodeAdd), addNodeName)
				.NewNode(typeof(NodeColor), colorNodeName)
				.NewNode(typeof(NodeTexture2D), textureNodeName)
				.Execute()
				.GetGraph();
			
			Assert.That(graph.FindNodeByName< NodeAdd >(addNodeName) != null, "add node was not found in the graph");
			Assert.That(graph.FindNodeByName< NodeColor >(colorNodeName) != null, "Color node was not found in the graph");
			Assert.That(graph.FindNodeByName< NodeTexture2D >(textureNodeName) != null, "Texture2D node was not found in the graph");
		}

		[Test]
		public static void FindNodeByNameNotFoundTest()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >().Execute().GetGraph();

			Assert.That(graph.FindNodeByName("inhexistant node") == null);
		}

		[Test]
		public static void RemoveNodeTest()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode< NodeSlider >("slider")
				.Execute()
				.GetGraph();
			
			var slider = graph.FindNodeByName("slider");

			graph.RemoveNode(slider);

			Assert.That(graph.nodes.Count == 0);
			Assert.That(graph.FindNodeByName("slider") == null);
		}

		[Test]
		public static void RemoveNodeWithoutEventsTest()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >()
				.NewNode< NodeSlider >("slider")
				.Execute()
				.GetGraph();
			
			var slider = graph.FindNodeByName("slider");

			graph.RemoveNode(slider, false);

			Assert.That(graph.nodes.Count == 0);
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
		public static void RemoveNodeLinksTest()
		{
			var graph = TestUtils.GenerateTestWorldGraph();

			var add2 = graph.FindNodeByName("add2");

			graph.RemoveNode(add2);

			RemoveNodeLinkAsserts(graph);
		}
		
		[Test]
		public static void RemoveNodeLinksWithoutEventsTest()
		{
			var graph = TestUtils.GenerateTestWorldGraph();

			var add2 = graph.FindNodeByName("add2");

			graph.RemoveNode(add2, false);

			RemoveNodeLinkAsserts(graph);
		}

		static void RemoveNodeLinkAsserts(BaseGraph graph)
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
		// c*: NodeBiomeSurfaceColor, s*: NodeBiomeSurfaceSwitch, surf: NodeBiomeSurface


		[Test]
		public static void GetNodeChildsRecursiveTest()
		{
			WorldGraph worldGraph = TestUtils.GenerateTestWorldGraphBiomeSwitch();

			var wlevel = worldGraph.FindNodeByName("wlevel");

			var recursiveNodesFromC2 = worldGraph.GetNodeChildsRecursive(wlevel);

			//check for duplicates
			Assert.That(recursiveNodesFromC2.Count == recursiveNodesFromC2.Distinct().Count());

			//check for compute order:
			for (int i = 0; i < recursiveNodesFromC2.Count - 1; i++)
			{
				var node1 = recursiveNodesFromC2[i];
				var node2 = recursiveNodesFromC2[i + 1];

				Assert.That(node1.computeOrder <= node2.computeOrder, "Nodes from GetNodeChildsRecursive are not computeOrder sorted");
			}
		}

		[Test]
		public static void CloneWorldGraphTest()
		{
			WorldGraph worldGraph = TestUtils.GenerateTestWorldGraph();
			WorldGraph clonedGraph = worldGraph.Clone() as WorldGraph;

			Assert.That(worldGraph.nodes.Count == clonedGraph.nodes.Count);
			Assert.That(worldGraph.allNodes.Count() == clonedGraph.allNodes.Count());
			Assert.That(worldGraph.nodeLinkTable.GetLinks().Count() == clonedGraph.nodeLinkTable.GetLinks().Count());

			foreach (var node in clonedGraph.allNodes)
				Assert.That(worldGraph.allNodes.Contains(node) == false);

			foreach (var node in clonedGraph.allNodes)
			{
				foreach (var anchorField in node.anchorFields)
					foreach (var anchor in anchorField.anchors)
						foreach (var link in anchor.links)
						{
							Assert.That(link.toNode != null);
							Assert.That(link.fromNode != null);

							Assert.That(clonedGraph.FindNodeById(link.toNode.id) != null);
							Assert.That(clonedGraph.FindNodeById(link.fromNode.id) != null);
							
							Assert.That(worldGraph.allNodes.Contains(link.fromNode) == false);
							Assert.That(worldGraph.allNodes.Contains(link.toNode) == false);
						}
			}

			Assert.That(clonedGraph.isReadyToProcess == true);
		
			clonedGraph.Process();
		}

		[Test]
		public static void CloneBiomeGraphTest()
		{
			BiomeGraph biomeGraph = TestUtils.GenerateTestBiomeGraph();
			BiomeGraph clonedGraph = biomeGraph.Clone() as BiomeGraph;

			Assert.That(biomeGraph.nodes.Count == clonedGraph.nodes.Count);
			Assert.That(biomeGraph.allNodes.Count() == clonedGraph.allNodes.Count());
			Assert.That(biomeGraph.nodeLinkTable.GetLinks().Count() == clonedGraph.nodeLinkTable.GetLinks().Count());

			Assert.That(clonedGraph.isReadyToProcess == true);
			
			foreach (var node in clonedGraph.allNodes)
				Assert.That(biomeGraph.allNodes.Contains(node) == false);
			
			foreach (var node in clonedGraph.allNodes)
			{
				foreach (var anchorField in node.anchorFields)
					foreach (var anchor in anchorField.anchors)
						foreach (var link in anchor.links)
						{
							Assert.That(link.toNode != null);
							Assert.That(link.fromNode != null);

							Assert.That(clonedGraph.FindNodeById(link.toNode.id) != null);
							Assert.That(clonedGraph.FindNodeById(link.fromNode.id) != null);
							
							Assert.That(biomeGraph.allNodes.Contains(link.fromNode) == false);
							Assert.That(biomeGraph.allNodes.Contains(link.toNode) == false);
						}
			}

			clonedGraph.Process();
		}

	}
}