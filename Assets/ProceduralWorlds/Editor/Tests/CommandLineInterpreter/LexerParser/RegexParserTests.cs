using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Core;
using PW.Node;
using System.Linq;
using System;

namespace PW
{
	public class RegexParserTests
	{
	
		#region NewNode command tests
	
		[Test]
		public void WhiteSpaceNewNodeCommand()
		{
			PWGraphCommand cmd;
	
			cmd = PWGraphCLI.Parse("  	  NewNode 	      PWNodeAdd  	    add  	    	 ");
	
			Assert.That(cmd.type == PWGraphCommandType.NewNode);
			Assert.That(cmd.nodeType == typeof(PWNodeAdd));
			Assert.That(cmd.name == "add");
			Assert.That(cmd.forcePositon == false);
		}
	
		[Test]
		public void WellFormatedNewNodeCommand()
		{
			string s = PWGraphCLI.GenerateNewNodeCommand(typeof(PWNodeAdd), "addName");
			PWGraphCommand cmd = PWGraphCLI.Parse(s);
	
			Assert.That(cmd.type == PWGraphCommandType.NewNode);
			Assert.That(cmd.nodeType == typeof(PWNodeAdd));
			Assert.That(cmd.name == "addName");
			Assert.That(cmd.forcePositon == false);
		}
	
		[Test]
		public void WhiteSpaceNewNodeWithPositionCommand()
		{
			PWGraphCommand cmd = PWGraphCLI.Parse("  	  NewNode 	      PWNodeAdd  	    add  	    (  	 42,   -42 	   )	 ");
	
			Assert.That(cmd.type == PWGraphCommandType.NewNodePosition);
			Assert.That(cmd.nodeType == typeof(PWNodeAdd));
			Assert.That(cmd.name == "add");
			Assert.That(cmd.forcePositon == true);
			Assert.That(cmd.position == new Vector2(42, -42));
		}
	
		[Test]
		public void WellFormatedNewNodeWithPositionCommand()
		{
			string s = PWGraphCLI.GenerateNewNodeCommand(typeof(PWNodeAdd), "addName", new Vector2(42, -42));
			PWGraphCommand cmd = PWGraphCLI.Parse(s);
	
			Assert.That(cmd.type == PWGraphCommandType.NewNodePosition);
			Assert.That(cmd.nodeType == typeof(PWNodeAdd));
			Assert.That(cmd.name == "addName");
			Assert.That(cmd.forcePositon == true);
			Assert.That(cmd.position == new Vector2(42, -42));
		}
	
		[Test]
		public void BadTypeNewNodeCommand()
		{
			try {
				PWGraphCLI.Parse("NewNode PWNodeUnknown unknown");
				throw new Exception("Unknow node type in newNode command didn't throw an exception");
			} catch {
				//the exception was thrown so the commmand works as excpected
			}
		}
	
		[Test]
		public void TooManyArgumentsNewNodeCommand()
		{
			try {
				PWGraphCLI.Parse("NewNode PWNodeUnknown node node node node");
				throw new Exception("too many arguments in newNode command didn't throw an exception");
			} catch {
				//the exception was thrown so the commmand works as excpected
			}
		}

		[Test]
		public void WellFormatedNewNodeWithDataCommand()
		{
			string s = PWGraphCLI.GenerateNewNodeCommand(typeof(PWNodePerlinNoise2D), "perlin", new PWGraphCLIAttributes() {{"persistance", 2.4f}, {"octaves", 3}});
			PWGraphCommand cmd = PWGraphCLI.Parse(s);

			var parsedAttrs = PWJson.Parse(cmd.attributes);
			var persistanceAttr = parsedAttrs[0];
			var octavesAttr = parsedAttrs[1];

			Assert.That(persistanceAttr.first == "persistance");
			Assert.That(persistanceAttr.second.Equals(2.4f));
			Assert.That(octavesAttr.first == "octaves");
			Assert.That(octavesAttr.second.Equals(3));
		}
	
		#endregion //New Node commands
	
		#region Link command tests
	
		[Test]
		public void WellFormatedLinkCommand()
		{
			string s = PWGraphCLI.GenerateLinkCommand("node1", "node2");
			PWGraphCommand cmd = PWGraphCLI.Parse(s);
	
			Assert.That(cmd.type == PWGraphCommandType.Link);
			Assert.That(cmd.fromName == "node1");
			Assert.That(cmd.toName == "node2");
		}
		
		[Test]
		public void WhiteSpaceLinkCommand()
		{
			PWGraphCommand cmd = PWGraphCLI.Parse("    	 	 	Link 		 	 node1   	node2	 	      ");
	
			Assert.That(cmd.type == PWGraphCommandType.Link);
			Assert.That(cmd.fromName == "node1");
			Assert.That(cmd.toName == "node2");
		}
	
		#endregion
		
		#region Command parser test
	
		[Test]
		public void UnknowCommand()
		{
			try {
				PWGraphCLI.Parse("BlaBlaBla arg1 arg2");
				throw new Exception("no exception was thrown by unknow command");
			} catch {
				//the exception was thrown so the commmand works as excpected
			}
		}
		
		[Test]
		public void EmptyCommand()
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