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

public class PWNodeAPITests
{
	//TestGraph: 
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

	PWMainGraph	GenerateTestGraph()
	{
		return PWGraphBuilder.NewGraph< PWMainGraph >()
			.NewNode(typeof(PWNodeSlider), "slider")
			.NewNode(typeof(PWNodeConstant), "constant")
			.NewNode(typeof(PWNodeAdd), "add1")
			.NewNode(typeof(PWNodeAdd), "add2")
			.NewNode(typeof(PWNodeAdd), "add3")
			.NewNode(typeof(PWNodeCurve), "curve")
			.NewNode(typeof(PWNodeDebugInfo), "debug1")
			.NewNode(typeof(PWNodeDebugInfo), "debug2")
			.Link("slider", "add1")
			.Link("slider", "add2")
			.Link("constant", "add2")
			.Link("constant", "add3")
			.Link("add1", "curve")
			.Link("curve", "debug1")
			.Link("add2", "debug2")
			.Execute()
			.GetGraph() as PWMainGraph;
	}

	[Test]
	public void PWNodeAPIGetNodesAttachedToAnchorArray()
	{
		PWMainGraph graph = GenerateTestGraph();

		var add2Node = graph.FindNodeByName("add2");
		var sliderNode = graph.FindNodeByName("slider");
		var floatNode = graph.FindNodeByName("constant");
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
		PWMainGraph graph = GenerateTestGraph();

		var curveNode = graph.FindNodeByName("curve");
		var add1Node = graph.FindNodeByName("add1");
		var debug1Node = graph.FindNodeByName("debug1");

		List< PWNode > curveInputNodes = curveNode.GetNodesAttachedToAnchor(curveNode.inputAnchors.First()).ToList();
		List< PWNode > curveOutputNodes = curveNode.GetNodesAttachedToAnchor(curveNode.outputAnchors.First()).ToList();

		Assert.That(curveInputNodes[0] == add1Node);
		Assert.That(curveOutputNodes[0] == debug1Node);
	}
	
}
