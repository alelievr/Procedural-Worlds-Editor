using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Reflection;

namespace PW.Core
{
	public class PWGraphCLIAttributes : Pairs< string, object > {}

	//Command line interpreter for PWGraph
	public static class PWGraphCLI
	{

		/*
		Valid command syntaxes:

		> NewNode nodeType nodeName [position] [attr=...]
		Create a new node of type nodeType in the graph, if the node
		type does not exists, an excetion is raised and the node is
		not created.
		The position is in pixels so it's an integer, floating values
		will not be accepted and will result with a lex error.

		ex:
			> NewNode PWNodeAdd simple_add
			> NewNode PWNodeAdd simple_add (10, 100)
			> NewNode PWPerlinNoise2D perlin attr={ frequency: 1.2, octaves: 4, scale: 8.5 }

		---------------------------------------------

		> Link nodeName1 nodeName2
		Create a link between two nodes, this command will try to find
		any linkable anchor between the two nodes, if there is not an
		exception is raised and the link is not created.

		ex:
			> NewNode PWNodeSlider slider
			> NewNode PWNodeAdd add
			> Link slider add

		---------------------------------------------

		*/


		static bool		debug = false;
		
		#region Command Parsing

		enum PWGraphToken
		{
			Undefined,
			NewNodeCommand,
			LinkCommand,
			OpenParenthesis,
			ClosedParenthesis,
			Word,
			IntValue,
			Comma,
			Attr,
			Equal,
			JsonDatas,
		}

		class PWGraphTokenMatch
		{
			public PWGraphToken	token;
			public string		value;
			public string		remainingText;
		}

		class PWGraphTokenDefinition
		{
			Regex					regex;
			readonly PWGraphToken	token;

			public PWGraphTokenDefinition(PWGraphToken graphToken, string regexString)
			{
				this.token = graphToken;
				this.regex = new Regex(regexString);
			}

			public PWGraphTokenMatch Match(string input)
			{
				Match				m = regex.Match(input);
				PWGraphTokenMatch	ret = null;

				if (m.Success)
				{
					ret = new PWGraphTokenMatch();

					ret.token = token;
					ret.value = m.Value.Trim();
					ret.remainingText = input.Substring(m.Value.Length);
				}
				return ret;
			}
		}

		static class PWGraphTokenSequence
		{
			public static List< PWGraphToken > newNode = new List< PWGraphToken >() {
				PWGraphToken.NewNodeCommand, PWGraphToken.Word, PWGraphToken.Word
			};

			public static List< PWGraphToken > newNodePosition = new List< PWGraphToken >() {
				PWGraphToken.NewNodeCommand, PWGraphToken.Word, PWGraphToken.Word, PWGraphToken.OpenParenthesis, PWGraphToken.IntValue, PWGraphToken.Comma, PWGraphToken.IntValue, PWGraphToken.ClosedParenthesis
			};

			public static List< PWGraphToken > newLink = new List< PWGraphToken >() {
				PWGraphToken.LinkCommand, PWGraphToken.Word, PWGraphToken.Word
			};

			public static List< PWGraphToken > newNodeAttrOption = new List< PWGraphToken >() {
				PWGraphToken.Attr, PWGraphToken.Equal, PWGraphToken.JsonDatas
			};
		}

		class PWGraphCommandTokenSequence
		{
			public PWGraphCommandType			type;
			public List< PWGraphToken >			requiredTokens = null;
			public List< PWGraphToken >			options = null;
		}

		static class PWGraphValidCommandTokenSequence
		{
			public static List< PWGraphCommandTokenSequence > validSequences = new List< PWGraphCommandTokenSequence >()
			{
				//New Node command
				new PWGraphCommandTokenSequence() {
					type = PWGraphCommandType.NewNode,
					options = PWGraphTokenSequence.newNodeAttrOption,
					requiredTokens = PWGraphTokenSequence.newNode,
				},
				//New Node position command
				new PWGraphCommandTokenSequence() {
					type = PWGraphCommandType.NewNodePosition,
					requiredTokens = PWGraphTokenSequence.newNodePosition,
					options = PWGraphTokenSequence.newNodeAttrOption,
				},
				//New Link command
				new PWGraphCommandTokenSequence() {
					type = PWGraphCommandType.Link,
					requiredTokens = PWGraphTokenSequence.newLink,
				}
			};
		}

		//token regex list by priority order
		static List< PWGraphTokenDefinition >	tokenDefinitions = new List< PWGraphTokenDefinition >()
		{
			new PWGraphTokenDefinition(PWGraphToken.LinkCommand, @"^Link"),
			new PWGraphTokenDefinition(PWGraphToken.NewNodeCommand, @"^NewNode"),
			new PWGraphTokenDefinition(PWGraphToken.OpenParenthesis, @"^\("),
			new PWGraphTokenDefinition(PWGraphToken.ClosedParenthesis, @"^\)"),
			new PWGraphTokenDefinition(PWGraphToken.IntValue, @"^[-+]?\d+"),
			new PWGraphTokenDefinition(PWGraphToken.Comma, @"^,"),
			new PWGraphTokenDefinition(PWGraphToken.Attr, @"^attr"),
			new PWGraphTokenDefinition(PWGraphToken.Equal, @"^="),
			new PWGraphTokenDefinition(PWGraphToken.JsonDatas, @"^{.*}"),
			new PWGraphTokenDefinition(PWGraphToken.Word, "^(\\\".*\\\"|\\S+)"),
		};

		static Dictionary< PWGraphCommandType, Action< PWGraph, PWGraphCommand, string > > commandTypeFunctions = new Dictionary< PWGraphCommandType, Action< PWGraph, PWGraphCommand, string > >()
		{
			{PWGraphCommandType.Link, CreateLink},
			{PWGraphCommandType.NewNode, CreateNode},
		};

		static PWGraphTokenMatch	Match(ref string line)
		{
			PWGraphTokenMatch	ret = null;

			foreach (var tokenDef in tokenDefinitions)
				if ((ret = tokenDef.Match(line.Trim())) != null)
					break ;

			if (ret != null)
			{
				if (debug)
					Debug.Log("Token " + ret.token + " maches value: " + ret.value + ", remainingText: " + ret.remainingText);
				line = ret.remainingText;
			}
			else if (debug)
				Debug.Log("No token maches found with line: " + line);

			return ret;
		}

		static Type	TryParseNodeType(string type)
		{
			Type	nodeType;

			//try to parse the node type:
			nodeType = Type.GetType(type);
			if (nodeType == null)
				nodeType = Type.GetType("PW.Node." + type);

			//thorw exception if the type can't be parse / does not inherit from PWNode
			if (nodeType == null || !nodeType.IsSubclassOf(typeof(PWNode)))
				throw new Exception("Type " + type + " not found as a node type");
			
			return nodeType;
		}

		static Vector2 TryParsePosition(string x, string y)
		{
			return new Vector2(Int32.Parse(x), Int32.Parse(y));
		}

		static PWGraphCommand CreateGraphCommand(PWGraphCommandTokenSequence seq, List< PWGraphTokenMatch > tokens)
		{
			Type	nodeType;
			string	attributes = null;

			switch (seq.type)
			{
				case PWGraphCommandType.Link:
					return new PWGraphCommand(tokens[1].value, tokens[2].value);
				case PWGraphCommandType.NewNode:
					nodeType = TryParseNodeType(tokens[1].value);
					if (tokens.Count > 4)
						attributes = tokens[5].value;
					return new PWGraphCommand(nodeType, tokens[2].value, attributes);
				case PWGraphCommandType.NewNodePosition:
					nodeType = TryParseNodeType(tokens[1].value);
					if (tokens.Count > 9)
						attributes = tokens[10].value;
					Vector2 position = TryParsePosition(tokens[4].value, tokens[6].value);
					return new PWGraphCommand(nodeType, tokens[2].value, position, attributes);
			}

			return null;
		}

		static PWGraphCommand		BuildCommand(List< PWGraphTokenMatch > tokens, string startLine)
		{
			foreach (var validTokenList in PWGraphValidCommandTokenSequence.validSequences)
			{
				//check if the token count can match the current valid token list
				if (validTokenList.requiredTokens.Count > tokens.Count)
					continue ;

				int i = 0;
				
				//check if the tokens we received correspond to a valid squence of token
				for (i = 0; i < validTokenList.requiredTokens.Count; i++)
				{
					if (tokens[i].token != validTokenList.requiredTokens[i])
						goto skipLoop;
				}

				//check for options:
				for (int j = 0; i < tokens.Count; i++, j++)
				{
					if (tokens[i].token != validTokenList.options[j])
						goto skipLoop;
				}
				
				//the valid token list iterated until it's end so we have a valid command
				return CreateGraphCommand(validTokenList, tokens);

				skipLoop:
				continue ;
			}

			throw new Exception("Invalid token squence: " + startLine);
		}

		public static PWGraphCommand	Parse(string inputCommand)
		{
			PWGraphTokenMatch			match;
			List< PWGraphTokenMatch >	lineTokens = new List< PWGraphTokenMatch >();
			string						startLine = inputCommand;

			while (!String.IsNullOrEmpty(inputCommand))
			{
				match = Match(ref inputCommand);

				if (match == null)
					throw new Exception("Invalid token at line \"" + inputCommand + "\"");

				lineTokens.Add(match);
			}

			return BuildCommand(lineTokens, startLine);
		}

		#endregion //Command parsing

		#region  Command Execution

		static void	CreateLink(PWGraph graph, PWGraphCommand command, string inputCommand)
		{
			//get nodes from the graph:
			PWNode fromNode = graph.FindNodeByName(command.fromName);
			PWNode toNode = graph.FindNodeByName(command.toName);

			if (fromNode == null)
				throw new Exception("Node " + command.fromName + " found in graph while parsing: '" + inputCommand + "'");
			if (toNode == null)
				throw new Exception("Node " + command.toName + " found in graph while parsing: '" + inputCommand + "'");
			
			//Create the first linkable anchors we found:
			foreach (var outAnchor in fromNode.outputAnchors)
				foreach (var inAnchor in toNode.inputAnchors)
					if (PWAnchorUtils.AnchorAreAssignable(outAnchor, inAnchor))
					{
						graph.CreateLink(outAnchor, inAnchor);
						break ;
					}
		}

		static void	CreateNode(PWGraph graph, PWGraphCommand command, string inputCommand)
		{
			Vector2	position = command.position;

			PWNode node = graph.CreateNewNode(command.nodeType, position);
			Type nodeType = node.GetType();

			if (!String.IsNullOrEmpty(command.attributes))
			{
				foreach (var attr in PWJson.Parse(command.attributes))
				{
					Debug.Log(attr.first + " -> " + attr.second);
					FieldInfo attrField = nodeType.GetField(attr.first, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

					if (attrField != null)
						attrField.SetValue(node, attr.second);
					else
						Debug.LogError("Attribute " + attr.first + " can be found in node " + node);
				}
			}

			node.name = command.name;
		}

		public static void Execute(PWGraph graph, string inputCommand)
		{
			PWGraphCommand command = Parse(inputCommand);

			if (commandTypeFunctions.ContainsKey(command.type))
				commandTypeFunctions[command.type](graph, command, inputCommand);
			else
				throw new Exception("Command type not handled: " + command.type);
		}

		#endregion // Command Execution

		#region Export and command creation

		public static string Export(PWGraph graph, string filePath)
		{
			Debug.Log("TODO");
			return null;
		}

		public static string GenerateNewNodeCommand(Type nodeType, string name, PWGraphCLIAttributes datas = null)
		{
			//TODO: use tokens here
			string cmd = "NewNode " + nodeType.Name + " " + name;
			if (datas != null && datas.Count != 0)
				cmd += " attr=" + PWJson.Generate(datas);
			
			return cmd;
		}

		public static string GenerateNewNodeCommand(Type nodeType, string name, Vector2 position, PWGraphCLIAttributes datas = null)
		{
			//TODO: use tokens here
			string cmd = "NewNode " + nodeType.Name + " " + name + " (" + (int)position.x + ", " + (int)position.y+ ")";
			if (datas != null && datas.Count != 0)
				cmd += " attr=" + PWJson.Generate(datas);
			
			return cmd;
		}

		public static string GenerateLinkCommand(string fromName, string toName)
		{
			//TODO: use tokens here
			return "Link " + fromName + " " + toName;
		}

		#endregion //Export and command creation

	}
}