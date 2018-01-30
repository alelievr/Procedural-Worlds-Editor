using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using PW.Core;
using PW.Node;
using PW.Biomator;

namespace PW.Tests.Graphs
{
	public class PWGraphLinkProcessingTests
	{
	
		[Test]
		public void PWGraphLinkProcessSimple()
		{
			var graph = PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode< PWNodeSlider >("slider")
				.NewNode< PWNodeDebugInfo >("debug")
				.Link("slider", "debug")
				.Execute()
				.GetGraph();
			
			var slider = graph.FindNodeByName< PWNodeSlider >("slider");
			var debug = graph.FindNodeByName< PWNodeDebugInfo >("debug");

			slider.outValue = 0.42f;

			graph.name = "MyGraph !";

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
				.NewNode< PWNodeBiomeSurfaceSwitch >("s1")
				.NewNode< PWNodeBiomeSurfaceSwitch >("s2")
				.NewNode< PWNodeBiomeSurfaceSwitch >("s3")
				.NewNode< PWNodeBiomeSurface >("surf")
				.Link("s1", "surf")
				.Link("s2" ,"surf")
				.Link("s3" ,"surf")
				.Link("c1", "s1")
				.Link("c2" ,"s2")
				.Link("c3" ,"s3")
				.Custom(g => {
					(g as PWBiomeGraph).surfaceType = BiomeSurfaceType.Color;
				})
				.Execute()
				.GetGraph();
			
			var c1 = graph.FindNodeByName< PWNodeBiomeSurfaceColor >("c1");
			var c2 = graph.FindNodeByName< PWNodeBiomeSurfaceColor >("c2");
			var c3 = graph.FindNodeByName< PWNodeBiomeSurfaceColor >("c3");
			var surf = graph.FindNodeByName< PWNodeBiomeSurface >("surf");

			c1.surfaceColor.baseColor = Color.red;
			c2.surfaceColor.baseColor = Color.green;
			c3.surfaceColor.baseColor = Color.yellow;

			graph.ProcessOnce();

			Assert.That(surf.surfaceGraph.cells.Count == 3);
		}
	
	}
}