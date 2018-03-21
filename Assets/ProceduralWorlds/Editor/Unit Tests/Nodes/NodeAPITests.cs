using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;
using ProceduralWorlds;

namespace ProceduralWorlds.Tests.Nodes
{
	public static class NodeAPITests
	{
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
		public static void NodeAPIGetNodesAttachedToAnchorArray()
		{
			WorldGraph graph = TestUtils.GenerateTestWorldGraph();
	
			var add2Node = graph.FindNodeByName("add2");
			var sliderNode = graph.FindNodeByName("slider");
			var debug2Node = graph.FindNodeByName("debug2");
	
			var add2NodeInputAnchor = add2Node.inputAnchors.First();
			var add2NodeOutputAnchor = add2Node.outputAnchors.First();
	
			List< BaseNode > inputNodes = add2Node.GetNodesAttachedToAnchor(add2NodeInputAnchor).ToList();
			List< BaseNode > outputNodes = add2Node.GetNodesAttachedToAnchor(add2NodeOutputAnchor).ToList();
	
			Assert.That(inputNodes.Count == 1);
			Assert.That(inputNodes[0] == sliderNode);
	
			Assert.That(outputNodes.Count == 1);
			Assert.That(outputNodes[0] == debug2Node);
		}
		
		[Test]
		public static void NodeAPIGetNodesAttachedToAnchor()
		{
			WorldGraph graph = TestUtils.GenerateTestWorldGraph();
	
			var add4Node = graph.FindNodeByName("add4");
			var add1Node = graph.FindNodeByName("add1");
			var debug1Node = graph.FindNodeByName("debug1");
		
			var add4NodeInAnchor = add4Node.inputAnchors.First();
			var add4NodeOutAnchor = add4Node.outputAnchors.First();
	
			List< BaseNode > add4InputNodes = add4Node.GetNodesAttachedToAnchor(add4NodeInAnchor).ToList();
			List< BaseNode > add4OutputNodes = add4Node.GetNodesAttachedToAnchor(add4NodeOutAnchor).ToList();
			
			Assert.That(add4InputNodes[0] == add1Node, "add4 input node was " + add4InputNodes[0] + ", " + add1Node + " expected");
			Assert.That(add4OutputNodes[0] == debug1Node, "add4 output node was " + add4OutputNodes[0] + ", " + debug1Node + " expected");
		}
		
	}
}