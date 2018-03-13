using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using ProceduralWorlds.Core;
using ProceduralWorlds.Node;
using ProceduralWorlds.Editor;

namespace ProceduralWorlds.Tests.Nodes
{
	public class BaseGraphNodesTests
	{
	
		[Test]
		public static void BaseGraphNodesSimplePasses()
		{
			var allWorldNodeInfos = NodeTypeProvider.GetAllowedNodesForGraph(BaseGraphType.World);

			var builder = BaseGraphBuilder.NewGraph< WorldGraph >();

			foreach (var mainTypes in allWorldNodeInfos)
				foreach (var nodeInfo in mainTypes.typeInfos)
				builder.NewNode(nodeInfo.type, nodeInfo.type.ToString());
			
			var graph = builder.Execute().GetGraph();

			foreach (var node in graph.allNodes)
			{
				var editor = UnityEditor.Editor.CreateEditor(node) as NodeEditor;
				editor.Initialize(null);
				editor.OnNodeUnitTest();
				UnityEditor.Editor.DestroyImmediate(editor);
			}
			
			builder = BaseGraphBuilder.NewGraph< WorldGraph >();

			foreach (var type in NodeTypeProvider.GetExlusiveNodeTypesForGraph(BaseGraphType.Biome))
				builder.NewNode(type, type.ToString());
			
			graph = builder.Execute().GetGraph();

			foreach (var node in graph.allNodes)
			{
				var editor = UnityEditor.Editor.CreateEditor(node) as NodeEditor;
				editor.OnNodeUnitTest();
			}
		}
	
	}
}