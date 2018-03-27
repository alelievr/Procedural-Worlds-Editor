using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using ProceduralWorlds.Core;
using ProceduralWorlds.Nodes;
using System.Linq;
using System;

namespace ProceduralWorlds.Tests.CLI
{
	public static class RegexParserTests
	{
	
		#region NewNode command tests
	
		[Test]
		public static void WhiteSpaceNewNodeCommand()
		{
			BaseGraphCommand cmd;
	
			cmd = BaseGraphCLI.Parse("  	  NewNode 	      NodeAdd  	    add  	    	 ");
	
			Assert.That(cmd.type == BaseGraphCommandType.NewNode);
			Assert.That(cmd.nodeType == typeof(NodeAdd));
			Assert.That(cmd.name == "add");
			Assert.That(cmd.forcePositon == false);
		}
	
		[Test]
		public static void WellFormatedNewNodeCommand()
		{
			string s = BaseGraphCLI.GenerateNewNodeCommand(typeof(NodeAdd), "addName");
			BaseGraphCommand cmd = BaseGraphCLI.Parse(s);
	
			Assert.That(cmd.type == BaseGraphCommandType.NewNode);
			Assert.That(cmd.nodeType == typeof(NodeAdd));
			Assert.That(cmd.name == "addName");
			Assert.That(cmd.forcePositon == false);
		}
		
		[Test]
		public static void WellFormatedWhitespaceNewNodeCommand()
		{
			string s = BaseGraphCLI.GenerateNewNodeCommand(typeof(NodeAdd), "add node name");
			BaseGraphCommand cmd = BaseGraphCLI.Parse(s);
	
			Assert.That(cmd.type == BaseGraphCommandType.NewNode);
			Assert.That(cmd.nodeType == typeof(NodeAdd));
			Assert.That(cmd.name == "add node name");
			Assert.That(cmd.forcePositon == false);
		}
	
		[Test]
		public static void WhiteSpaceNewNodeWithPositionCommand()
		{
			BaseGraphCommand cmd = BaseGraphCLI.Parse("  	  NewNode 	      NodeAdd  	    add  	    (  	 42,   -42 	   )	 ");
	
			Assert.That(cmd.type == BaseGraphCommandType.NewNodePosition);
			Assert.That(cmd.nodeType == typeof(NodeAdd));
			Assert.That(cmd.name == "add");
			Assert.That(cmd.forcePositon == true);
			Assert.That(cmd.position == new Vector2(42, -42));
		}
	
		[Test]
		public static void WellFormatedNewNodeWithPositionCommand()
		{
			string s = BaseGraphCLI.GenerateNewNodeCommand(typeof(NodeAdd), "addName", new Vector2(42, -42));
			BaseGraphCommand cmd = BaseGraphCLI.Parse(s);
	
			Assert.That(cmd.type == BaseGraphCommandType.NewNodePosition, "Bad command type: " + cmd.type + " instead of " + BaseGraphCommandType.NewNodePosition);
			Assert.That(cmd.nodeType == typeof(NodeAdd), "Bad node type: " + cmd.nodeType + " instead of " + typeof(NodeAdd));
			Assert.That(cmd.name == "addName", "Bad node name: " + cmd.name + " instead of addName");
			Assert.That(cmd.forcePositon == true, "Forceposition is false but expected to be true");
			Assert.That(cmd.position == new Vector2(42, -42), "Bad node position: " + cmd.position + " instead of " + new Vector2(42, -42));
		}
	
		[Test]
		public static void BadTypeNewNodeCommand()
		{
			try {
				BaseGraphCLI.Parse("NewNode NodeUnknown unknown");
			} catch {
				//the exception was thrown so the commmand works as excpected
				return ;
			}

			throw new InvalidOperationException("Unknow node type in newNode command didn't throw an exception");
		}

		[Test]
		public static void missingNameNewNodeCommand()
		{
			try {
				BaseGraphCLI.Parse("NewNode NodeAdd");
			} catch {
				return ;
			}
			throw new InvalidOperationException("Missing name in newNode command did't throw an exception");
		}
	
		[Test]
		public static void TooManyArgumentsNewNodeCommand()
		{
			try {
				BaseGraphCLI.Parse("NewNode NodeAdd node node node node");
			} catch {
				//the exception was thrown so the commmand works as excpected
				return ;
			}
			
			throw new InvalidOperationException("too many arguments in newNode command didn't throw an exception");
		}

		[Test]
		public static void WellFormatedNewNodeWithDataCommand()
		{
			string s = BaseGraphCLI.GenerateNewNodeCommand(typeof(NodePerlinNoise2D), "perlin noise", new BaseGraphCLIAttributes {{"persistence", 2.4f}, {"octaves", 3}});
			BaseGraphCommand cmd = BaseGraphCLI.Parse(s);

			var parsedAttrs = Jsonizer.Parse(cmd.attributes);
			var persistenceAttr = parsedAttrs[0];
			var octavesAttr = parsedAttrs[1];

			Assert.That(persistenceAttr.first == "persistence", "The persistence name expected to be 'persistence' but was '" + persistenceAttr.first + "'");
			Assert.That((float)persistenceAttr.second == 2.4f, "The persistence value expected to be 2.4 but was '" + persistenceAttr.second + "'");
			Assert.That(octavesAttr.first == "octaves", "The octaves name expected to be 'octaves' but was '" + octavesAttr.first + "'");
			Assert.That((int)octavesAttr.second == 3, "The octaves value expected to be 3 but was '" + octavesAttr.second + "'");
		}
		
		[Test]
		public static void WellFormatedNewNodeWithPositionAndDataCommand()
		{
			string s = BaseGraphCLI.GenerateNewNodeCommand(typeof(NodePerlinNoise2D), "perlin noise", new Vector2(21, 84), new BaseGraphCLIAttributes {{"persistence", 1.4f}, {"octaves", 2}});
			BaseGraphCommand cmd = BaseGraphCLI.Parse(s);

			var parsedAttrs = Jsonizer.Parse(cmd.attributes);
			var persistenceAttr = parsedAttrs[0];
			var octavesAttr = parsedAttrs[1];

			Assert.That(persistenceAttr.first == "persistence", "The persistence name expected to be 'persistence' but was '" + persistenceAttr.first + "'");
			Assert.That((float)persistenceAttr.second == 1.4f, "The persistence value expected to be 1.4 but was '" + persistenceAttr.second + "'");
			Assert.That(octavesAttr.first == "octaves", "The octaves name expected to be 'octaves' but was '" + octavesAttr.first + "'");
			Assert.That((int)octavesAttr.second == 2, "The octaves value expected to be 2 but was '" + octavesAttr.second + "'");
		}
	
		#endregion //New Node commands
	
		#region Link command tests
	
		[Test]
		public static void WellFormatedLinkCommand()
		{
			string s = BaseGraphCLI.GenerateLinkCommand("node1", "node2");
			BaseGraphCommand cmd = BaseGraphCLI.Parse(s);
	
			Assert.That(cmd.type == BaseGraphCommandType.Link);
			Assert.That(cmd.fromNodeName == "node1");
			Assert.That(cmd.toNodeName == "node2");
		}
		
		[Test]
		public static void WellFormatedLinkCommandNameWhitespaces()
		{
			string s = BaseGraphCLI.GenerateLinkCommand("node 1", "node 2");
			BaseGraphCommand cmd = BaseGraphCLI.Parse(s);
	
			Assert.That(cmd.type == BaseGraphCommandType.Link);
			Assert.That(cmd.fromNodeName == "node 1");
			Assert.That(cmd.toNodeName == "node 2");
		}
		
		[Test]
		public static void WhiteSpaceLinkCommand()
		{
			BaseGraphCommand cmd = BaseGraphCLI.Parse("    	 	 	Link 		 	 node1   	node2	 	      ");
	
			Assert.That(cmd.type == BaseGraphCommandType.Link);
			Assert.That(cmd.fromNodeName == "node1");
			Assert.That(cmd.toNodeName == "node2");
		}
	
		#endregion
		
		#region LinkAnchor command tests

		[Test]
		public static void WellFormatedLinkAnchorCommand()
		{
			string s = BaseGraphCLI.GenerateLinkAnchorCommand("node1", 1, "node2", 4);
			BaseGraphCommand cmd = BaseGraphCLI.Parse(s);

			Assert.That(cmd.type == BaseGraphCommandType.LinkAnchor);
			Assert.That(cmd.fromNodeName == "node1");
			Assert.That(cmd.toNodeName == "node2");
			Assert.That(cmd.fromAnchorIndex == 1);
			Assert.That(cmd.toAnchorIndex == 4);
		}
		
		[Test]
		public static void WellFormatedLinkAnchorCommandNameWhitespaces()
		{
			string s = BaseGraphCLI.GenerateLinkAnchorCommand("node 1", 1, "node 2", 4);
			BaseGraphCommand cmd = BaseGraphCLI.Parse(s);

			Assert.That(cmd.type == BaseGraphCommandType.LinkAnchor);
			Assert.That(cmd.fromNodeName == "node 1");
			Assert.That(cmd.toNodeName == "node 2");
			Assert.That(cmd.fromAnchorIndex == 1);
			Assert.That(cmd.toAnchorIndex == 4);
		}
		

		[Test]
		public static void WellFormatedLinkAnchorNameCommand()
		{
			string s = BaseGraphCLI.GenerateLinkAnchorNameCommand("node1", "a1", "node2", "a2");
			BaseGraphCommand cmd = BaseGraphCLI.Parse(s);

			Assert.That(cmd.type == BaseGraphCommandType.LinkAnchorName);
			Assert.That(cmd.fromNodeName == "node1");
			Assert.That(cmd.toNodeName == "node2");
			Assert.That(cmd.fromAnchorFieldName == "a1");
			Assert.That(cmd.toAnchorFieldName == "a2");
		}
		
		[Test]
		public static void WellFormatedLinkAnchorNameCommandNameWhitespaces()
		{
			string s = BaseGraphCLI.GenerateLinkAnchorNameCommand("node 1", "a1", "node 2", "a2");
			BaseGraphCommand cmd = BaseGraphCLI.Parse(s);

			Assert.That(cmd.type == BaseGraphCommandType.LinkAnchorName);
			Assert.That(cmd.fromNodeName == "node 1");
			Assert.That(cmd.toNodeName == "node 2");
			Assert.That(cmd.fromAnchorFieldName == "a1");
			Assert.That(cmd.toAnchorFieldName == "a2");
		}

		#endregion

		#region Graph attribute commands

		[Test]
		public static void WellFormatedGraphAttributeCommand()
		{
			string s = BaseGraphCLI.GenerateGraphAttributeCommand("field", 12.5f);

			BaseGraphCommand cmd = BaseGraphCLI.Parse(s);

			Assert.That(cmd.type == BaseGraphCommandType.GraphAttribute);
			Assert.That(cmd.graphFieldName == "field");
			Assert.That(cmd.graphFieldValue.Equals(12.5f));
		}

		[Test]
		public static void WellFormatedGraphAttributeCommandInvalidType()
		{
			string s = BaseGraphCLI.GenerateGraphAttributeCommand("field", new object());

			Assert.That(s == null);
		}
		
		[Test]
		public static void WellFormatedGraphAttributeTypesCommand()
		{
			string s = BaseGraphCLI.GenerateGraphAttributeCommand("field", true);
			BaseGraphCommand cmd = BaseGraphCLI.Parse(s);

			Assert.That(cmd.graphFieldValue.Equals(true));
			
			s = BaseGraphCLI.GenerateGraphAttributeCommand("field", 12);
			cmd = BaseGraphCLI.Parse(s);

			Assert.That(cmd.graphFieldValue.Equals(12));
		}

		#endregion

		#region Command parser tests
	
		[Test]
		public static void UnknowCommand()
		{
			try {
				BaseGraphCLI.Parse("BlaBlaBla arg1 arg2");
				throw new InvalidOperationException("no exception was thrown by unknow command");
			} catch {
				//the exception was thrown so the commmand works as excpected
			}
		}
		
		[Test]
		public static void EmptyCommand()
		{
			try {
				BaseGraphCLI.Parse("");
				throw new InvalidOperationException("no exception was thrown by an empty command");
			} catch {
				//the exception was thrown so the commmand works as excpected
			}
		}
	
		#endregion
	
	}
}