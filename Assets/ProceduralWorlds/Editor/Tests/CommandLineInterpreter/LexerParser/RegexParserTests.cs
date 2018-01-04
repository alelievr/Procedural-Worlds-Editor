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
	
			Assert.That(cmd.type == PWGraphCommandType.NewNodePosition, "Bad command type: " + cmd.type + " instead of " + PWGraphCommandType.NewNodePosition);
			Assert.That(cmd.nodeType == typeof(PWNodeAdd), "Bad node type: " + cmd.nodeType + " instead of " + typeof(PWNodeAdd));
			Assert.That(cmd.name == "addName", "Bad node name: " + cmd.name + " instead of addName");
			Assert.That(cmd.forcePositon == true, "Forceposition is false but expected to be true");
			Assert.That(cmd.position == new Vector2(42, -42), "Bad node position: " + cmd.position + " instead of " + new Vector2(42, -42));
		}
	
		[Test]
		public void BadTypeNewNodeCommand()
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
		public void TooManyArgumentsNewNodeCommand()
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
		public void WellFormatedNewNodeWithDataCommand()
		{
			string s = PWGraphCLI.GenerateNewNodeCommand(typeof(PWNodePerlinNoise2D), "perlin", new PWGraphCLIAttributes() {{"persistance", 2.4f}, {"octaves", 3}});
			PWGraphCommand cmd = PWGraphCLI.Parse(s);

			var parsedAttrs = PWJson.Parse(cmd.attributes);
			var persistanceAttr = parsedAttrs[0];
			var octavesAttr = parsedAttrs[1];

			Assert.That(persistanceAttr.first == "persistance", "The persistance name expected to be 'persistance' but was '" + persistanceAttr.first + "'");
			Assert.That((float)persistanceAttr.second == 2.4f, "The persistance value expected to be 2.4 but was '" + persistanceAttr.second + "'");
			Assert.That(octavesAttr.first == "octaves", "The octaves name expected to be 'octaves' but was '" + octavesAttr.first + "'");
			Assert.That((int)octavesAttr.second == 3, "The octaves value expected to be 3 but was '" + octavesAttr.second + "'");
		}
		
		[Test]
		public void WellFormatedNewNodeWithPositionAndDataCommand()
		{
			string s = PWGraphCLI.GenerateNewNodeCommand(typeof(PWNodePerlinNoise2D), "perlin", new Vector2(21, 84), new PWGraphCLIAttributes() {{"persistance", 1.4f}, {"octaves", 2}});
			PWGraphCommand cmd = PWGraphCLI.Parse(s);

			var parsedAttrs = PWJson.Parse(cmd.attributes);
			var persistanceAttr = parsedAttrs[0];
			var octavesAttr = parsedAttrs[1];

			Assert.That(persistanceAttr.first == "persistance", "The persistance name expected to be 'persistance' but was '" + persistanceAttr.first + "'");
			Assert.That((float)persistanceAttr.second == 1.4f, "The persistance value expected to be 1.4 but was '" + persistanceAttr.second + "'");
			Assert.That(octavesAttr.first == "octaves", "The octaves name expected to be 'octaves' but was '" + octavesAttr.first + "'");
			Assert.That((int)octavesAttr.second == 2, "The octaves value expected to be 2 but was '" + octavesAttr.second + "'");
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