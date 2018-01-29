using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Node;
using PW.Core;

namespace PW.Tests
{
	public static class TestUtils
	{
	
		//TestGraph 1: 
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
	
		public static PWMainGraph	GenerateTestMainGraph()
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
	
	}
}