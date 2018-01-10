using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Core;
using PW.Node;

namespace PW
{
	public class PWGraphNodesTests
	{
	
		[Test]
		public void PWGraphNodesSimplePasses()
		{
			var allMainNodeInfos = PWNodeTypeProvider.GetAllowedNodesForGraph(PWGraphType.Main);

			var builder = PWGraphBuilder.NewGraph< PWMainGraph >();

			foreach (var mainTypes in allMainNodeInfos)
				foreach (var nodeInfo in mainTypes.typeInfos)
				builder.NewNode(nodeInfo.type, nodeInfo.type.ToString());
			
			var graph = builder.Execute().GetGraph();

			foreach (var node in graph.nodes)
				node.OnNodeUnitTest();
			
			builder = PWGraphBuilder.NewGraph< PWMainGraph >();

			foreach (var type in PWNodeTypeProvider.GetExlusiveNodeTypesForGraph(PWGraphType.Biome))
				builder.NewNode(type, type.ToString());
			
			graph = builder.Execute().GetGraph();

			foreach (var node in graph.nodes)
				node.OnNodeUnitTest();
		}
	
	}
}