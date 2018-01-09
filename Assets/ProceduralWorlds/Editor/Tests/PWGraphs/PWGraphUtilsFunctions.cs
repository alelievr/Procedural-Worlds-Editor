using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using PW.Node;
using PW.Core;

namespace PW
{
	public class PWGraphUtilsFunctions
	{
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