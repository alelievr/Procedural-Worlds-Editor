using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Core;
using PW.Node;
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
			
			ScriptableObject.DestroyImmediate(graph);
		}
	
		[Test]
		public void PWGraphImport()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >().GetGraph();

			var exampleGraph = TestUtils.GenerateTestMainGraph();

			exampleGraph.Export(tmpFilePath);

			graph.Import(tmpFilePath);

			//Compare the two graphs node and link count:
			Assert.That(exampleGraph.nodes.Count == graph.nodes.Count, "Bad node count !");
			Assert.That(exampleGraph.nodeLinkTable.GetLinks().Count() == graph.nodeLinkTable.GetLinks().Count(), "Bad links count !");

			//Compare for node and links:
			for (int i = 0; i < exampleGraph.nodes.Count; i++)
			{
				var exNode = exampleGraph.nodes[i];
				var newNode = graph.nodes[i];

				Assert.That(exNode.GetType() == newNode.GetType(), "Node type differs !");

				var exAnchorFields = exNode.anchorFields.ToList();
				var newAnchorFields = newNode.anchorFields.ToList();
				for (int j = 0; j < exAnchorFields.Count; j++)
				{
					var exAnchors = exAnchorFields[j].anchors;
					var newAnchors = newAnchorFields[j].anchors;

					for (int k = 0; k < exAnchors.Count; k++)
					{
						var exLinks = exAnchors[k].links.ToList();
						var newLinks = exAnchors[k].links.ToList();

						for (int l = 0; l < exLinks.Count; l++)
						{
							Assert.That(exLinks[l].fromNode.GetType() == newLinks[l].fromNode.GetType());
							Assert.That(exLinks[l].toNode.GetType() == newLinks[l].toNode.GetType());
						}
					}
				}	
			}

			//Compare specific nodes:
			var exCurveNode = exampleGraph.FindNodeByName< PWNodeSlider >("slider");
			var newCurveNode = graph.FindNodeByName< PWNodeSlider >("slider");

			Assert.That(exCurveNode.min == newCurveNode.min, "imported slider curve min (" + exCurveNode.min + ") != exported slider curve min (" + newCurveNode.min + ")");
			Assert.That(exCurveNode.max == newCurveNode.max, "imported slider curve max (" + exCurveNode.max + ") != exported slider curve max (" + newCurveNode.max + ")");
			Assert.That(exCurveNode.outValue == newCurveNode.outValue, "imported slider curve outValue (" + exCurveNode.outValue + ") != exported slider curve outValue (" + newCurveNode.outValue + ")");
		}
	
	}
}