using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using ProceduralWorlds.Tests;

namespace ProceduralWorlds.Tests.Graphs
{
	public class BaseGraphReadyToProcess
	{
	
		[Test]
		public void BaseGraphReadyToProcessWorldGraph()
		{
			var worldGraph = TestUtils.GenerateTestWorldGraph();

			Assert.That(worldGraph.readyToProcess == true, "Graph is not ready to process");
		}
	}
}