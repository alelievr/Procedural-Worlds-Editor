using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Core;
using PW.Node;
using System.Linq;
using System;

namespace PW.Tests.CLI
{
	public class RegexParserTests
	{
	
		#region NewNode command tests
	
		[Test]
		public static void WhiteSpaceNewNodeCommand()
		{
			PWGraphCommand cmd;
	
			cmd = PWGraphCLI.Parse("  	  NewNode 	      PWNodeAdd  	    add  	    	 ");
	
			Assert.That(cmd.type == PWGraphCommandType.NewNode);
			Assert.That(cmd.nodeType == typeof(PWNodeAdd));
			Assert.That(cmd.name == "add");
			Assert.That(cmd.forcePositon == false);
		}
	
		[Test]
		public static void WellFormatedNewNodeCommand()
		{
			string s = PWGraphCLI.GenerateNewNodeCommand(typeof(PWNodeAdd), "addName");
			PWGraphCommand cmd = PWGraphCLI.Parse(s);
	
			Assert.That(cmd.type == PWGraphCommandType.NewNode);
			Assert.That(cmd.nodeType == typeof(PWNodeAdd));
			Assert.That(cmd.name == "addName");
			Assert.That(cmd.forcePositon == false);
		}
		
		[Test]
		public static void WellFormatedWhitespaceNewNodeCommand()
		{
			string s = PWGraphCLI.GenerateNewNodeCommand(typeof(PWNodeAdd), "add node name");
			PWGraphCommand cmd = PWGraphCLI.Parse(s);
	
			Assert.That(cmd.type == PWGraphCommandType.NewNode);
			Assert.That(cmd.nodeType == typeof(PWNodeAdd));
			Assert.That(cmd.name == "add node name");
			Assert.That(cmd.forcePositon == false);
		}
	
		[Test]
		public static void WhiteSpaceNewNodeWithPositionCommand()
		{
			PWGraphCommand cmd = PWGraphCLI.Parse("  	  NewNode 	      PWNodeAdd  	    add  	    (  	 42,   -42 	   )	 ");
	
			Assert.That(cmd.type == PWGraphCommandType.NewNodePosition);
			Assert.That(cmd.nodeType == typeof(PWNodeAdd));
			Assert.That(cmd.name == "add");
			Assert.That(cmd.forcePositon == true);
			Assert.That(cmd.position == new Vector2(42, -42));
		}
	
		[Test]
		public static void WellFormatedNewNodeWithPositionCommand()
		{
			string s = PWGraphCLI.GenerateNewNodeCommand(typeof(PWNodeAdd), "addName", new Vector2(42, -42));
			PWGraphCommand cmd = PWGraphCLI.Parse(s);
	
			Assert.That(cmd.type == PWGraphCommandType.NewNodePosition, "Bad command type: " + cmd.type + " instead of " + PWGraphCommandType.NewNodePosition);
			Assert.That(cmd.nodeType == typeof(PWNodeAdd), "Bad node type: " + cmd.nodeType + " instead of " + typeof(PWNodeAdd));
			Assert.That(cmd.name == "addName", "Bad node name: " + cmd.name + " instead of addName");
			Assert.That(cmd.forcePositon == true, "Forceposition is false but expected to be true");
			Assert.That(cmd.position == new Vector2(42, -42), "Bad node position: " + cmd.position + " instead of " + new Vector2(42, -42));
		}
	
		[Test]
		public static void BadTypeNewNodeCommand()
		{
			try {
				PWGraphCLI.Parse("NewNode PWNodeUnknown unknown");
			} catch {
				//the exception was thrown so the commmand works as excpected
				return ;
			}

			throw new Exception("Unknow node type in newNode command didn't throw an exception");
		}

		[Test]
		public static void missingNameNewNodeCommand()
		{
			try {
				PWGraphCLI.Parse("NewNode PWNodeAdd");
			} catch {
				return ;
			}
			throw new Exception("Missing name in newNode command did't throw an exception");
		}
	
		[Test]
		public static void TooManyArgumentsNewNodeCommand()
		{
			try {
				PWGraphCLI.Parse("NewNode PWNodeAdd node node node node");
			} catch {
				//the exception was thrown so the commmand works as excpected
				return ;
			}
			
			throw new Exception("too many arguments in newNode command didn't throw an exception");
		}

		[Test]
		public static void WellFormatedNewNodeWithDataCommand()
		{
			string s = PWGraphCLI.GenerateNewNodeCommand(typeof(PWNodePerlinNoise2D), "perlin noise", new PWGraphCLIAttributes() {{"persistence", 2.4f}, {"octaves", 3}});
			PWGraphCommand cmd = PWGraphCLI.Parse(s);

			var parsedAttrs = PWJson.Parse(cmd.attributes);
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
			string s = PWGraphCLI.GenerateNewNodeCommand(typeof(PWNodePerlinNoise2D), "perlin noise", new Vector2(21, 84), new PWGraphCLIAttributes() {{"persistence", 1.4f}, {"octaves", 2}});
			PWGraphCommand cmd = PWGraphCLI.Parse(s);

			var parsedAttrs = PWJson.Parse(cmd.attributes);
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
			string s = PWGraphCLI.GenerateLinkCommand("node1", "node2");
			PWGraphCommand cmd = PWGraphCLI.Parse(s);
	
			Assert.That(cmd.type == PWGraphCommandType.Link);
			Assert.That(cmd.fromNodeName == "node1");
			Assert.That(cmd.toNodeName == "node2");
		}
		
		[Test]
		public static void WellFormatedLinkCommandNameWhitespaces()
		{
			string s = PWGraphCLI.GenerateLinkCommand("node 1", "node 2");
			PWGraphCommand cmd = PWGraphCLI.Parse(s);
	
			Assert.That(cmd.type == PWGraphCommandType.Link);
			Assert.That(cmd.fromNodeName == "node 1");
			Assert.That(cmd.toNodeName == "node 2");
		}
		
		[Test]
		public static void WhiteSpaceLinkCommand()
		{
			PWGraphCommand cmd = PWGraphCLI.Parse("    	 	 	Link 		 	 node1   	node2	 	      ");
	
			Assert.That(cmd.type == PWGraphCommandType.Link);
			Assert.That(cmd.fromNodeName == "node1");
			Assert.That(cmd.toNodeName == "node2");
		}
	
		#endregion
		
		#region LinkAnchor command tests

		[Test]
		public static void WellFormatedLinkAnchorCommand()
		{
			string s = PWGraphCLI.GenerateLinkAnchorCommand("node1", 1, "node2", 4);
			PWGraphCommand cmd = PWGraphCLI.Parse(s);

			Assert.That(cmd.type == PWGraphCommandType.LinkAnchor);
			Assert.That(cmd.fromNodeName == "node1");
			Assert.That(cmd.toNodeName == "node2");
			Assert.That(cmd.fromAnchorIndex == 1);
			Assert.That(cmd.toAnchorIndex == 4);
		}
		
		[Test]
		public static void WellFormatedLinkAnchorCommandNameWhitespaces()
		{
			string s = PWGraphCLI.GenerateLinkAnchorCommand("node 1", 1, "node 2", 4);
			PWGraphCommand cmd = PWGraphCLI.Parse(s);

			Assert.That(cmd.type == PWGraphCommandType.LinkAnchor);
			Assert.That(cmd.fromNodeName == "node 1");
			Assert.That(cmd.toNodeName == "node 2");
			Assert.That(cmd.fromAnchorIndex == 1);
			Assert.That(cmd.toAnchorIndex == 4);
		}
		

		[Test]
		public static void WellFormatedLinkAnchorNameCommand()
		{
			string s = PWGraphCLI.GenerateLinkAnchorNameCommand("node1", "a1", "node2", "a2");
			PWGraphCommand cmd = PWGraphCLI.Parse(s);

			Assert.That(cmd.type == PWGraphCommandType.LinkAnchorName);
			Assert.That(cmd.fromNodeName == "node1");
			Assert.That(cmd.toNodeName == "node2");
			Assert.That(cmd.fromAnchorFieldName == "a1");
			Assert.That(cmd.toAnchorFieldName == "a2");
		}
		
		[Test]
		public static void WellFormatedLinkAnchorNameCommandNameWhitespaces()
		{
			string s = PWGraphCLI.GenerateLinkAnchorNameCommand("node 1", "a1", "node 2", "a2");
			PWGraphCommand cmd = PWGraphCLI.Parse(s);

			Assert.That(cmd.type == PWGraphCommandType.LinkAnchorName);
			Assert.That(cmd.fromNodeName == "node 1");
			Assert.That(cmd.toNodeName == "node 2");
			Assert.That(cmd.fromAnchorFieldName == "a1");
			Assert.That(cmd.toAnchorFieldName == "a2");
		}

		#endregion

		#region Command parser tests
	
		[Test]
		public static void UnknowCommand()
		{
			try {
				PWGraphCLI.Parse("BlaBlaBla arg1 arg2");
				throw new Exception("no exception was thrown by unknow command");
			} catch {
				//the exception was thrown so the commmand works as excpected
			}
		}
		
		[Test]
		public static void EmptyCommand()
		{
			try {
				PWGraphCLI.Parse("");
				throw new Exception("no exception was thrown by an empty command");
			} catch {
				//the exception was thrown so the commmand works as excpected
			}
		}
	
		#endregion
	
	}
}