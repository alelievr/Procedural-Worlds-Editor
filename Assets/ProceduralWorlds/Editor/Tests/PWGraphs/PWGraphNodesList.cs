using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Tests;
using PW.Core;
using System;
using System.Linq;

namespace PW.Tests.Graphs
{
	public class PWGraphNodesList
	{
		[Test]
		public void PWGraphNodesListContainsInputOrOutput()
		{
			var mainGraph = TestUtils.GenerateTestMainGraph();

			Type[] inputAndOutputTypes = new Type[]{
				typeof(PWNodeBiomeGraphInput), typeof(PWNodeBiomeGraphOutput),
				typeof(PWNodeGraphInput), typeof(PWNodeGraphOutput)
			};

			foreach (var node in mainGraph.nodes)
			{
				Debug.Log("node: " + node);
				Assert.That(inputAndOutputTypes.Contains(node.GetType()) == false);
			}
		}
		
		[Test]
		public void PWGraphNodesAllListContainsInputOrOutput()
		{
			var mainGraph = TestUtils.GenerateTestMainGraph();

			Type[] inputAndOutputTypes = new Type[]{
				typeof(PWNodeBiomeGraphInput), typeof(PWNodeBiomeGraphOutput),
				typeof(PWNodeGraphInput), typeof(PWNodeGraphOutput)
			};

			bool found = false;
			foreach (var node in mainGraph.allNodes)
			{
				found = found || inputAndOutputTypes.Contains(node.GetType());
			}

			Assert.That(found == true, "PWGraph.allNodes don't contains input/output nodes");
		}
	}
}
