using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using PW.Core;
using PW.Node;

namespace PW.Tests.Graphs
{
	public class PWGraphLinkProcessingTests
	{
	
		[Test]
		public void PWGraphLinkProcessSimple()
		{
			var graph = PWGraphBuilder.NewGraph< PWBiomeGraph >()
				.NewNode< PWNodeSlider >("slider")
				.NewNode< PWNodeDebugInfo >("debug")
				.Link("slider", "debug")
				.Execute()
				.GetGraph();
			
			var slider = graph.FindNodeByName< PWNodeSlider >("slider");
			var debug = graph.FindNodeByName< PWNodeDebugInfo >("debug");

			slider.outValue = 0.42f;

			graph.Process();

			Assert.That(debug.obj.Equals(0.42f));
		}

		[Test]
		public void PWGraphLinkProcessOnceArray()
		{
			var graph = PWGraphBuilder.NewGraph< PWBiomeGraph >()
				.NewNode< PWNodeBiomeSurfaceColor >("c1")
				.NewNode< PWNodeBiomeSurfaceColor >("c2")
				.NewNode< PWNodeBiomeSurfaceColor >("c3")
				.NewNode< PWNodeBiomeSurface >("surf")
				.Link("c1", "surf")
				.Link("c2" ,"surf")
				.Link("c3" ,"surf")
				.Execute()
				.GetGraph();
			
			var c1 = graph.FindNodeByName< PWNodeBiomeSurfaceColor >("c1");
			var c2 = graph.FindNodeByName< PWNodeBiomeSurfaceColor >("c2");
			var c3 = graph.FindNodeByName< PWNodeBiomeSurfaceColor >("c3");
			var surf = graph.FindNodeByName< PWNodeBiomeSurface >("surf");

			c1.surfaceColor.baseColor = Color.red;
			c2.surfaceColor.baseColor = Color.green;
			c3.surfaceColor.baseColor = Color.yellow;

			graph.Process();

			Debug.Log("surfaceGraph: " + surf.surfaceGraph.isBuilt);
			Debug.Log("surfaceGraph: " + surf.surfaceGraph.cells.Count);
			Assert.That(surf.surfaceGraph.cells.Count == 3);
		}
	
	}
}