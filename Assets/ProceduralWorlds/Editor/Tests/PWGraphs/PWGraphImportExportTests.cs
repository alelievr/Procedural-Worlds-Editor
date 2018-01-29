using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Core;
using System.IO;
using System.Linq;

namespace PW.Tests.Graphs
{
	public class PWGraphImportExportTests
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

		string tmpFilePath = Application.temporaryCachePath + "/tmp_graph.txt";

		[Test]
		public void PWGraphExport()
		{
			var graph = TestUtils.GenerateTestMainGraph();

			graph.Export(tmpFilePath);

			string[] lines = File.ReadAllLines(tmpFilePath);
		}
	
		[Test]
		public void PWGraphImport()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >().GetGraph();

			var exampleGraph = TestUtils.GenerateTestMainGraph();

			exampleGraph.Export(tmpFilePath);

			graph.Import(tmpFilePath);

			//compare the two graphs
			Assert.That(exampleGraph.nodes.Count == graph.nodes.Count);
			Assert.That(exampleGraph.nodeLinkTable.GetLinks().Count() == graph.nodeLinkTable.GetLinks().Count());

		}
	
	}
}