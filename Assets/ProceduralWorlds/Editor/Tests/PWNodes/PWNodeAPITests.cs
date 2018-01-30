using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PW.Core;
using PW.Node;
using PW;

namespace PW.Tests.Nodes
{
	public class PWNodeAPITests
	{
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
		public void PWNodeAPIGetNodesAttachedToAnchorArray()
		{
			PWMainGraph graph = TestUtils.GenerateTestMainGraph();
	
			var add2Node = graph.FindNodeByName("add2");
			var sliderNode = graph.FindNodeByName("slider");
			var debug2Node = graph.FindNodeByName("debug2");
	
			var add2NodeInputAnchor = add2Node.inputAnchors.First();
			var add2NodeOutputAnchor = add2Node.outputAnchors.First();
	
			List< PWNode > inputNodes = add2Node.GetNodesAttachedToAnchor(add2NodeInputAnchor).ToList();
			List< PWNode > outputNodes = add2Node.GetNodesAttachedToAnchor(add2NodeOutputAnchor).ToList();
	
			Assert.That(inputNodes.Count == 1);
			Assert.That(inputNodes[0] == sliderNode);
	
			Assert.That(outputNodes.Count == 1);
			Assert.That(outputNodes[0] == debug2Node);
		}
		
		[Test]
		public void PWNodeAPIGetNodesAttachedToAnchor()
		{
			PWMainGraph graph = TestUtils.GenerateTestMainGraph();
	
			var curveNode = graph.FindNodeByName("curve");
			var add1Node = graph.FindNodeByName("add1");
			var debug1Node = graph.FindNodeByName("debug1");
		
			var curveNodeInAnchor = curveNode.inputAnchors.First();
			var curveNodeOutAnchor = curveNode.outputAnchors.First();
		
			Debug.Log("curve input: " + curveNodeInAnchor);
			Debug.Log("curve output: " + curveNodeOutAnchor);
	
			List< PWNode > curveInputNodes = curveNode.GetNodesAttachedToAnchor(curveNodeInAnchor).ToList();
			List< PWNode > curveOutputNodes = curveNode.GetNodesAttachedToAnchor(curveNodeOutAnchor).ToList();

			Debug.Log("linkCount: " + curveNodeInAnchor.linkCount);
			
			foreach (var inputNode in curveInputNodes)
				Debug.Log("Input node: " + inputNode);

			foreach (var outputNode in curveOutputNodes)
				Debug.Log("Output node: " + outputNode);
	
			Assert.That(curveInputNodes[0] == add1Node, "Curve input node was " + curveInputNodes[0] + ", " + add1Node + " expected");
			Assert.That(curveOutputNodes[0] == debug1Node, "Curve output node was " + curveOutputNodes[0] + ", " + debug1Node + " expected");
		}
		
	}
}