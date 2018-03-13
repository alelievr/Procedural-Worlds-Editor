using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Tests.Graphs
{
	public class GraphProcessTests
	{
		
		//test main graph
		// 	              +-----+
		//            +---> add1+---+
		// +------+   |   +-----+   |   +-----+   +------+
		// |slider+---+             +---> add4+--->debug1|
		// +------+   |   +-----+       +-----+   +------+
		//            +---> add2+---+
		// +------+   |   +-----+   |   +------+
		// |constant+-+             +--->debug2|
		// +------+   |   +-----+       +------+
		//            +---> add3+    
		//                +-----+

		[Test]
		public static void RemoveNodeProcess()
		{
			var graph = TestUtils.GenerateTestWorldGraph();

			var add3 = graph.FindNodeByName("add3");
			var add2 = graph.FindNodeByName< NodeAdd >("add2");
			var debug = graph.FindNodeByName< NodeDebugInfo >("debug1");
			var debug2 = graph.FindNodeByName< NodeDebugInfo >("debug2");
			var slider = graph.FindNodeByName< NodeSlider >("slider");
			var constant = graph.FindNodeByName< NodeConstant >("constant");
			graph.RemoveNode(add3);

			slider.outValue = 3;
			constant.outf = 2;

			graph.Process();

			Assert.That(debug.obj.Equals(3f));
			Assert.That(debug2.obj.Equals(5f));
		}
	}
}