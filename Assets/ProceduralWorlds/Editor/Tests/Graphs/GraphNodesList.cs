using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using ProceduralWorlds.Tests;
using ProceduralWorlds.Core;
using System;
using System.Linq;

namespace ProceduralWorlds.Tests.Graphs
{
	public class BaseGraphNodesList
	{
		[Test]
		public void BaseGraphNodesListContainsInputOrOutput()
		{
			var worldGraph = TestUtils.GenerateTestWorldGraph();

			Type[] inputAndOutputTypes = new Type[]{
				typeof(NodeBiomeGraphInput), typeof(NodeBiomeGraphOutput),
				typeof(NodeGraphInput), typeof(NodeGraphOutput)
			};

			foreach (var node in worldGraph.nodes)
				Assert.That(inputAndOutputTypes.Contains(node.GetType()) == false);
		}
		
		[Test]
		public void BaseGraphNodesAllListContainsInputOrOutput()
		{
			var worldGraph = TestUtils.GenerateTestWorldGraph();

			Type[] inputAndOutputTypes = new Type[]{
				typeof(NodeBiomeGraphInput), typeof(NodeBiomeGraphOutput),
				typeof(NodeGraphInput), typeof(NodeGraphOutput)
			};

			bool found = false;
			foreach (var node in worldGraph.allNodes)
			{
				found = found || inputAndOutputTypes.Contains(node.GetType());
			}

			Assert.That(found == true, "BaseGraph.allNodes don't contains input/output nodes");
		}
	}
}
