using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;
using System.IO;
using System.Linq;

namespace ProceduralWorlds.Tests.Graphs
{
	public class BaseGraphImportExportTests
	{

		//TestUtils.GenerateTestWorldGraph():
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

		static string tmpFilePath = Application.temporaryCachePath + "/tmp_graph.txt";

		[Test]
		static public void BaseGraphExport()
		{
			var graph = TestUtils.GenerateTestWorldGraph();

			graph.Export(tmpFilePath);

			string[] lines = File.ReadAllLines(tmpFilePath);

			foreach (var line in lines)
				BaseGraphCLI.Parse(line);
		}
	
		[Test]
		static public void BaseGraphImport()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >().GetGraph();

			var exampleGraph = TestUtils.GenerateTestWorldGraph();

			exampleGraph.Export(tmpFilePath);

			graph.Import(tmpFilePath);

			CompareGraphs(exampleGraph, graph);
			
			//Compare specific nodes:
			var exCurveNode = exampleGraph.FindNodeByName< NodeSlider >("slider");
			var newCurveNode = graph.FindNodeByName< NodeSlider >("slider");

			Assert.That(exCurveNode.min == newCurveNode.min, "imported slider curve min (" + exCurveNode.min + ") != exported slider curve min (" + newCurveNode.min + ")");
			Assert.That(exCurveNode.max == newCurveNode.max, "imported slider curve max (" + exCurveNode.max + ") != exported slider curve max (" + newCurveNode.max + ")");
			Assert.That(exCurveNode.outValue == newCurveNode.outValue, "imported slider curve outValue (" + exCurveNode.outValue + ") != exported slider curve outValue (" + newCurveNode.outValue + ")");
		}
		
		[Test]
		static public void BaseGraphImportBiome()
		{
			var graph = GraphBuilder.NewGraph< BiomeGraph >().GetGraph();

			var exampleGraph = TestUtils.GenerateTestBiomeGraph();

			exampleGraph.Export(tmpFilePath);

			graph.Import(tmpFilePath);

			CompareGraphs(exampleGraph, graph);
		}
		
		[Test]
		static public void BaseGraphImportWorldWhiteSpaces()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >().GetGraph();

			var exampleGraph = TestUtils.GenerateTestWorldGraphWhitespaces();

			exampleGraph.Export(tmpFilePath);

			graph.Import(tmpFilePath);

			CompareGraphs(exampleGraph, graph);
		}

		[Test]
		static public void BaseGraphExportImportDuplicateNodeNames()
		{
			var graph = GraphBuilder.NewGraph< WorldGraph >().GetGraph();

			var s1 = graph.CreateNewNode< NodeSlider >(Vector2.zero, "slider");
			var s2 = graph.CreateNewNode< NodeSlider >(Vector2.zero, "slider");
			var s3 = graph.CreateNewNode< NodeSlider >(Vector2.zero, "slider");

			var add = graph.CreateNewNode< NodeAdd >(Vector2.zero);

			graph.SafeCreateLink(s1.outputAnchors.First(), add.inputAnchors.Last());
			graph.SafeCreateLink(s2.outputAnchors.First(), add.inputAnchors.Last());
			graph.SafeCreateLink(s3.outputAnchors.First(), add.inputAnchors.Last());

			graph.Export(tmpFilePath);

			var importedGraph = GraphBuilder.NewGraph< WorldGraph >().GetGraph();

			importedGraph.Import(tmpFilePath);

			CompareGraphs(graph, importedGraph);
		}

		static void CompareGraphs(BaseGraph g1, BaseGraph g2)
		{
			//Compare the two graphs node and link count:
			Assert.That(g1.nodes.Count == g2.nodes.Count, "Bad node count !");
			Assert.That(g1.nodeLinkTable.GetLinks().Count() == g2.nodeLinkTable.GetLinks().Count(), "Bad links count !");

			//Compare node count:
			Assert.That(g1.allNodes.Count() == g2.allNodes.Count(), "Bad node count !");

			//Compare for node and links:
			for (int i = 0; i < g1.allNodes.Count(); i++)
			{
				var exNode = g1.allNodes.ElementAt(i);
				var newNode = g2.allNodes.ElementAt(i);

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