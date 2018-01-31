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
	}
}