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

			//TODO: compare lines
			
			ScriptableObject.DestroyImmediate(graph);
		}
	
		[Test]
		public void PWGraphImport()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >().GetGraph();

			var exampleGraph = TestUtils.GenerateTestMainGraph();

			exampleGraph.Export(tmpFilePath);

			graph.Import(tmpFilePath);

			CompareGraphs(exampleGraph, graph);
			
			//Compare specific nodes:
			var exCurveNode = exampleGraph.FindNodeByName< PWNodeSlider >("slider");
			var newCurveNode = graph.FindNodeByName< PWNodeSlider >("slider");

			Assert.That(exCurveNode.min == newCurveNode.min, "imported slider curve min (" + exCurveNode.min + ") != exported slider curve min (" + newCurveNode.min + ")");
			Assert.That(exCurveNode.max == newCurveNode.max, "imported slider curve max (" + exCurveNode.max + ") != exported slider curve max (" + newCurveNode.max + ")");
			Assert.That(exCurveNode.outValue == newCurveNode.outValue, "imported slider curve outValue (" + exCurveNode.outValue + ") != exported slider curve outValue (" + newCurveNode.outValue + ")");
		}
		
		[Test]
		public void PWGraphImportBiome()
		{
			var graph = PWGraphBuilder.NewGraph< PWBiomeGraph >().GetGraph();

			var exampleGraph = TestUtils.GenerateTestBiomeGraph();

			exampleGraph.Export(tmpFilePath);

			graph.Import(tmpFilePath);

			CompareGraphs(exampleGraph, graph);
		}
		
		[Test]
		public void PWGraphImportMainWhiteSpaces()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >().GetGraph();

			var exampleGraph = TestUtils.GenerateTestMainGraphWhitespaces();

			exampleGraph.Export(tmpFilePath);

			graph.Import(tmpFilePath);

			CompareGraphs(exampleGraph, graph);
		}

		[Test]
		public void PWGraphExportImportDuplicateNodeNames()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >().GetGraph();

			var s1 = graph.CreateNewNode< PWNodeSlider >(Vector2.zero, "slider");
			var s2 = graph.CreateNewNode< PWNodeSlider >(Vector2.zero, "slider");
			var s3 = graph.CreateNewNode< PWNodeSlider >(Vector2.zero, "slider");

			var add = graph.CreateNewNode< PWNodeAdd >(Vector2.zero);

			graph.SafeCreateLink(s1.outputAnchors.First(), add.inputAnchors.Last());
			graph.SafeCreateLink(s2.outputAnchors.First(), add.inputAnchors.Last());
			graph.SafeCreateLink(s3.outputAnchors.First(), add.inputAnchors.Last());

			graph.Export(tmpFilePath);

			var importedGraph = PWGraphBuilder.NewGraph< PWMainGraph >().GetGraph();

			importedGraph.Import(tmpFilePath);

			CompareGraphs(graph, importedGraph);
		}

		void CompareGraphs(PWGraph g1, PWGraph g2)
		{
			//Compare the two graphs node and link count:
			Assert.That(g1.nodes.Count == g2.nodes.Count, "Bad node count !");
			Assert.That(g1.nodeLinkTable.GetLinks().Count() == g2.nodeLinkTable.GetLinks().Count(), "Bad links count !");

			//Compare node count:
			Assert.That(g1.nodes.Count == g2.nodes.Count, "Bad node count !");

			//Compare for node and links:
			for (int i = 0; i < g1.nodes.Count; i++)
			{
				var exNode = g1.nodes[i];
				var newNode = g2.nodes[i];

				Assert.That(exNode.GetType() == newNode.GetType(), "Node type differs: expected " + exNode.GetType() + ", got:" + newNode.GetType());

				var exAnchorFields = exNode.anchorFields.ToList();
				var newAnchorFields = newNode.anchorFields.ToList();
				for (int j = 0; j < exAnchorFields.Count; j++)
				{
					var exAnchors = exAnchorFields[j].anchors;
					var newAnchors = newAnchorFields[j].anchors;

					for (int k = 0; k < exAnchors.Count; k++)
					{
						var exLinks = exAnchors[k].links.ToList();
						var newLinks = newAnchors[k].links.ToList();
						
						Assert.That(exLinks.Count == newLinks.Count, "Anchors " + exAnchors[k] + " and " + newAnchors[k] + " have different link count");

						for (int l = 0; l < exLinks.Count; l++)
						{
							Assert.That(exLinks[l].fromNode.GetType() == newLinks[l].fromNode.GetType());
							Assert.That(newLinks[l].toNode.GetType() == newLinks[l].toNode.GetType());
						}
					}
				}
			}
		}
	
	}
}