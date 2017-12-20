using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

namespace PW.Core
{
	//Command line interpreter for PWGraph
	public static class PWGraphCLI
	{

		enum PWGraphToken
		{
			Undefined,
			NewNodeCommand,
			LinkCommand,
			OpenParenthesis,
			ClosedParenthesis,
			NodeTypeName,
			NameValue,
			IntValue,
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
					ret.value = m.Value;
					ret.remainingText = input.Substring(m.Value.Length);
				}
				return ret;
			}
		}

		/*
		Valid command syntaxes:

		> NewNode nodeType nodeName [position]
		Create a new node of type nodeType in the graph, if the node
		type does not exists, an excetion is raised and the node is
		not created.
		The position is in pixels so it's an integer, floating values
		will not be accepted and will result with a lex error.

		ex:
			> NewNode PWNodeAdd simple_add
			> NewNode PWNodeAdd simple_add (10, 100)

		---------------------------------------------

		> NewLink nodeName1 nodeName2
		Create a link between two nodes, this command will try to find
		any linkable anchor between the two nodes, if there is not an
		exception is raised and the link is not created.

		ex:
			> NewNode PWNodeSlider slider
			> NewNode PWNodeAdd add
			> NewLink slider add

		---------------------------------------------

		*/

		static class PWGraphTokenSequence
		{
			public static List< PWGraphToken > newNode = new List< PWGraphToken >() {
				PWGraphToken.NewNodeCommand, PWGraphToken.NodeTypeName
			};

			public static List< PWGraphToken > newNodePositionOption = new List< PWGraphToken >() {
				PWGraphToken.OpenParenthesis, PWGraphToken.IntValue, PWGraphToken.IntValue, PWGraphToken.ClosedParenthesis
			};

			public static List< PWGraphToken > newNodeNameOption = new List< PWGraphToken >() {
				PWGraphToken.NameValue
			};

			public static List< PWGraphToken > newLink = new List< PWGraphToken >() {
				PWGraphToken.LinkCommand, PWGraphToken.NameValue, PWGraphToken.NameValue
			};
		}

		class PWGraphCommandTokenSequence
		{
			public PWGraphCommandType			type;
			public List< PWGraphToken >			requiredTokens = null;
			
			public int							requiredOptionCount = 0;
			public List< List< PWGraphToken > >	optionTokens = null;
		}

		static class PWGraphValidCommandTokenSequence
		{
			public static List< PWGraphCommandTokenSequence > validSequences = new List< PWGraphCommandTokenSequence >()
			{
				//Node command
				new PWGraphCommandTokenSequence() {
					type = PWGraphCommandType.NewNode,
					requiredTokens = PWGraphTokenSequence.newNode,
					requiredOptionCount = 1,
					optionTokens = new List< List< PWGraphToken > > { PWGraphTokenSequence.newNodeNameOption, PWGraphTokenSequence.newNodePositionOption }
				},
				//Link command
				new PWGraphCommandTokenSequence() {
					type = PWGraphCommandType.Link,
					requiredTokens = PWGraphTokenSequence.newLink,
				}
			};
		}

		static List< PWGraphTokenDefinition >	tokenDefinitions = new List< PWGraphTokenDefinition >()
		{
			new PWGraphTokenDefinition(PWGraphToken.ClosedParenthesis, @"^\)"),
			new PWGraphTokenDefinition(PWGraphToken.LinkCommand, @"^Link"),
			new PWGraphTokenDefinition(PWGraphToken.NewNodeCommand, @"^NewNode"),
			new PWGraphTokenDefinition(PWGraphToken.NodeTypeName, @"^\S+"),
			new PWGraphTokenDefinition(PWGraphToken.NodeTypeName, @"^,"),
			new PWGraphTokenDefinition(PWGraphToken.NameValue, "^(\\\"[^']*\\\"|\\S+)"),
			new PWGraphTokenDefinition(PWGraphToken.OpenParenthesis, @"^\("),
			new PWGraphTokenDefinition(PWGraphToken.IntValue, @"^\d+"),
		};

		static Dictionary< PWGraphCommandType, Action< PWGraph, PWGraphCommand, string > > commandTypeFunctions = new Dictionary< PWGraphCommandType, Action< PWGraph, PWGraphCommand, string > >()
		{
			{PWGraphCommandType.Link, CreateLink},
			{PWGraphCommandType.NewNode, CreateNode},
		};

		static PWGraphTokenMatch	Match(ref string line)
		{
			PWGraphTokenMatch	ret = null;

			//remove useless whitespaces:
			line.Trim();

			foreach (var tokenDef in tokenDefinitions)
				if ((ret = tokenDef.Match(line)) != null)
					break ;

			if (ret != null)
				line = ret.remainingText;

			return ret;
		}

		static PWGraphCommand CreateGraphCommand(PWGraphCommandTokenSequence seq, List< PWGraphTokenMatch > tokens)
		{
			switch (seq.type)
			{
				case PWGraphCommandType.Link:
					return new PWGraphCommand(tokens[1].value, tokens[2].value);
				case PWGraphCommandType.NewNode:
					//try to parse the node type:
					Type nodeType = Type.GetType(tokens[1].value);
					if (nodeType == null)
						nodeType = Type.GetType("PW.Node." + tokens[1].value);
		
					Debug.Log("nodeType: " + nodeType);
		
					//thorw exception if the type can't be parse / does not inherit from PWNode
					if (nodeType == null || !nodeType.IsAssignableFrom(typeof(PWNode)))
						throw new Exception("Type " + tokens[1].value + " not found as a node type");
					
					return new PWGraphCommand(nodeType, tokens[2].value);
			}

			return null;
		}

		static PWGraphCommand		BuildCommand(List< PWGraphTokenMatch > tokens, string startLine)
		{
			foreach (var validTokenList in PWGraphValidCommandTokenSequence.validSequences)
			{
				if (validTokenList.requiredTokens.Count > tokens.Count)
					continue ;
				
				//check if the tokens we received correspond to a valid squence of token
				for (int i = 0; i < validTokenList.requiredTokens.Count; i++)
					if (tokens[i].token != validTokenList.requiredTokens[i])
						goto skipLoop;
				
				//the valid token list iterated until it's end so we have a valid command
				// if there is no argument or the required options count is 0
				if (validTokenList.optionTokens == null || validTokenList.requiredOptionCount == 0)
					return CreateGraphCommand(validTokenList, tokens);
				
				//manage options tokens if 
				int	validOptionCount = 0;
				foreach (var optionTokenList in validTokenList.optionTokens)
				{
					for (int i = 0; i < optionTokenList.Count; i++)
					{
						if (tokens[i].token != optionTokenList[i])
							goto skipLoop;
						else
							validOptionCount++;
					}
					if (validOptionCount == validTokenList.requiredOptionCount)
						return CreateGraphCommand(validTokenList, tokens);
				}

				skipLoop:
				continue ;
			}

			throw new Exception("Invalid token at line: " + startLine);
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

		static void CreateLink(PWGraph graph, PWGraphCommand command, string inputCommand)
		{
			//get nodes from the graph:
			PWNode fromNode = graph.FindNodeByName(command.fromName);
			PWNode toNode = graph.FindNodeByName(command.toName);

			if (fromNode == null)
				throw new Exception("Node not found in graph: " + command.fromName + " while parsing: '" + inputCommand + "'");
			if (toNode == null)
				throw new Exception("Node not found in graph: " + command.toName + " while parsing: '" + inputCommand + "'");
			
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

			graph.CreateNewNode(command.nodeType, position);
		}

		//yup i know, this function is very slow with big graphs, but fast enough for a pre-apha
		public static void Execute(PWGraph graph, string inputCommand)
		{
			PWGraphCommand command = Parse(inputCommand);

			if (commandTypeFunctions.ContainsKey(command.type))
				commandTypeFunctions[command.type](graph, command, inputCommand);
			else
				throw new Exception("Command type not handled: " + command.type);
		}

		public static void Export(PWGraph graph, string filePath)
		{
			Debug.Log("TODO");
		}

		public static string GenerateNewNodeCommand(Type nodeType, string name)
		{
			//TODO: use tokens here
			return "NewNode " + nodeType.Name + " as \"" + name + "\"";
		}

		public static string GenerateNewNodeCommand(Type nodeType, string name, Vector2 position)
		{
			//TODO: use tokens here
			return "NewNode " + nodeType.Name + " at (" + (int)position.x + ", " + (int)position.y+ ") as \"" + name + "\"";
		}

		public static string GenerateLinkCommand(string fromName, string toName)
		{
			//TODO: use tokens here
			return "Link \"" + fromName + "\" to \"" + toName + "\"";
		}

	}
}