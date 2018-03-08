using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Core;
using PW.Node;
using PW.Editor;

namespace PW.Tests.Nodes
{
	public class PWGraphNodesTests
	{
	
		[Test]
		public static void PWGraphNodesSimplePasses()
		{
			var allMainNodeInfos = PWNodeTypeProvider.GetAllowedNodesForGraph(PWGraphType.Main);

			var builder = PWGraphBuilder.NewGraph< PWMainGraph >();

			foreach (var mainTypes in allMainNodeInfos)
				foreach (var nodeInfo in mainTypes.typeInfos)
				builder.NewNode(nodeInfo.type, nodeInfo.type.ToString());
			
			var graph = builder.Execute().GetGraph();

			foreach (var node in graph.allNodes)
			{
				var editor = UnityEditor.Editor.CreateEditor(node) as PWNodeEditor;
				editor.Initialize(null);
				editor.OnNodeUnitTest();
				UnityEditor.Editor.DestroyImmediate(editor);
			}
			
			builder = PWGraphBuilder.NewGraph< PWBiomeGraph >();

			foreach (var type in PWNodeTypeProvider.GetExlusiveNodeTypesForGraph(PWGraphType.Biome))
				builder.NewNode(type, type.ToString());
			
			graph = builder.Execute().GetGraph();

			foreach (var node in graph.allNodes)
			{
				var editor = UnityEditor.Editor.CreateEditor(node) as PWNodeEditor;
				editor.OnNodeUnitTest();
			}
		}
	
	}
}