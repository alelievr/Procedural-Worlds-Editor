using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Tests;

namespace PW.Tests.Graphs
{
	public class PWGraphReadyToProcess
	{
	
		[Test]
		public void PWGraphReadyToProcessMainGraph()
		{
			var mainGraph = TestUtils.GenerateTestMainGraph();

			Assert.That(mainGraph.readyToProcess == true, "Graph is not ready to process");
		}
	}
}