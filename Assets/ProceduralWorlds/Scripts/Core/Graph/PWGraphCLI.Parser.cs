using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;
using System.IO;

namespace PW.Core
{
	//Command line interpreter for PWGraph
	public static partial class PWGraphCLI
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

		> LinkAnchor nodeName1:anchorFieldIndex nodeName2:anchorFieldIndex
		> LinkAnchor nodeName1:anchorFieldName nodeName2:anchorFieldName
		Create a link between two nodes using the anchor index / name
		to find the anchor to link.
		when using the anchorFieldName, it'll try to find the field inside
		the nodes using their names and then link them. If the specified anchor
		is multiple (PWArray<>) the link will be created on the first unlinked
		available anchor.
		Remarks: anchorFieldIndex start from 0.

		ex:
			> NewNode PWNodeSlider slider
			> NewNode PWNodeAdd add
			> LinkAnchor slider:outpuValue add:values
			> LinkAnchor slider:0 add:0

		---------------------------------------------

		*/


		static readonly bool		debug = false;

		enum PWGraphToken
		{
			Undefined,
			NewNodeCommand,
			LinkCommand,
			LinkAnchorCommand,
			OpenParenthesis,
			ClosedParenthesis,
			Word,
			IntValue,
			Comma,
			Attr,
			Colon,
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
					if (ret.value[0] == '"')
						ret.value = ret.value.Trim('"');
					ret.remainingText = input.Substring(m.Value.Length);
				}
				return ret;
			}
		}

		static class PWGraphTokenSequence
		{
			public readonly static List< PWGraphToken > newNode = new List< PWGraphToken > {
				PWGraphToken.NewNodeCommand, PWGraphToken.Word, PWGraphToken.Word
			};

			public readonly static List< PWGraphToken > newNodePosition = new List< PWGraphToken > {
				PWGraphToken.NewNodeCommand, PWGraphToken.Word, PWGraphToken.Word, PWGraphToken.OpenParenthesis, PWGraphToken.IntValue, PWGraphToken.Comma, PWGraphToken.IntValue, PWGraphToken.ClosedParenthesis
			};

			public readonly static List< PWGraphToken > newLink = new List< PWGraphToken > {
				PWGraphToken.LinkCommand, PWGraphToken.Word, PWGraphToken.Word
			};

			public readonly static List< PWGraphToken > newLinkAnchor = new List< PWGraphToken > {
				PWGraphToken.LinkAnchorCommand, PWGraphToken.Word, PWGraphToken.Colon, PWGraphToken.IntValue, PWGraphToken.Word, PWGraphToken.Colon, PWGraphToken.IntValue
			};
			
			public readonly static List< PWGraphToken > newLinkAnchorName = new List< PWGraphToken > {
				PWGraphToken.LinkAnchorCommand, PWGraphToken.Word, PWGraphToken.Colon, PWGraphToken.Word, PWGraphToken.Word, PWGraphToken.Colon, PWGraphToken.Word
			};

			public readonly static List< PWGraphToken > newNodeAttrOption = new List< PWGraphToken > {
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
			public readonly static List< PWGraphCommandTokenSequence > validSequences = new List< PWGraphCommandTokenSequence >
			{
				//New Node command
				new PWGraphCommandTokenSequence {
					type = PWGraphCommandType.NewNode,
					options = PWGraphTokenSequence.newNodeAttrOption,
					requiredTokens = PWGraphTokenSequence.newNode,
				},
				//New Node position command
				new PWGraphCommandTokenSequence {
					type = PWGraphCommandType.NewNodePosition,
					requiredTokens = PWGraphTokenSequence.newNodePosition,
					options = PWGraphTokenSequence.newNodeAttrOption,
				},
				//New Link command
				new PWGraphCommandTokenSequence {
					type = PWGraphCommandType.Link,
					requiredTokens = PWGraphTokenSequence.newLink,
				},
				//New Link Anchor command
				new PWGraphCommandTokenSequence {
					type = PWGraphCommandType.LinkAnchor,
					requiredTokens = PWGraphTokenSequence.newLinkAnchor
				},
				//New Link Anchor using names command
				new PWGraphCommandTokenSequence {
					type = PWGraphCommandType.LinkAnchorName,
					requiredTokens = PWGraphTokenSequence.newLinkAnchorName
				},
			};
		}

		//token regex list by priority order
		readonly static List< PWGraphTokenDefinition >	tokenDefinitions = new List< PWGraphTokenDefinition >
		{
			new PWGraphTokenDefinition(PWGraphToken.LinkAnchorCommand, @"^LinkAnchor"),
			new PWGraphTokenDefinition(PWGraphToken.LinkCommand, @"^Link"),
			new PWGraphTokenDefinition(PWGraphToken.NewNodeCommand, @"^NewNode"),
			new PWGraphTokenDefinition(PWGraphToken.OpenParenthesis, @"^\("),
			new PWGraphTokenDefinition(PWGraphToken.ClosedParenthesis, @"^\)"),
			new PWGraphTokenDefinition(PWGraphToken.IntValue, @"^[-+]?\d+"),
			new PWGraphTokenDefinition(PWGraphToken.Comma, @"^,"),
			new PWGraphTokenDefinition(PWGraphToken.Attr, @"^attr"),
			new PWGraphTokenDefinition(PWGraphToken.Equal, @"^="),
			new PWGraphTokenDefinition(PWGraphToken.Colon, @"^:"),
			new PWGraphTokenDefinition(PWGraphToken.JsonDatas, @"^{.*}"),
			new PWGraphTokenDefinition(PWGraphToken.Word, "^(\\\"(.*?)\\\"|\\S+)"),
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
					Debug.Log("Token " + ret.token + " maches value: |" + ret.value + "|, remainingText: |" + ret.remainingText + "|");
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
			if (nodeType == null)
				nodeType = Type.GetType("PW.Core." + type);

			//thorw exception if the type can't be parse / does not inherit from PWNode
			if (nodeType == null || !nodeType.IsSubclassOf(typeof(PWNode)))
				throw new InvalidOperationException("Type " + type + " not found as a node type (" + nodeType + ")");
			
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
				case PWGraphCommandType.LinkAnchor:
					int fromAnchorField = int.Parse(tokens[3].value);
					int toAnchorField = int.Parse(tokens[6].value);
					return new PWGraphCommand(tokens[1].value, fromAnchorField, tokens[4].value, toAnchorField);
				case PWGraphCommandType.LinkAnchorName:
					return new PWGraphCommand(tokens[1].value, tokens[3].value, tokens[4].value, tokens[6].value);
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
				default:
					return null;
			}
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

				//if the validTokenList does not take options but there are remaining tokens, skip this command:
				if (validTokenList.options == null && i < tokens.Count)
					goto skipLoop;

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

			throw new InvalidOperationException("Invalid token squence: " + startLine);
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
					throw new InvalidOperationException("Invalid token at line \"" + inputCommand + "\"");

				lineTokens.Add(match);
			}

			if (lineTokens.Count == 0)
				throw new InvalidOperationException("Invalid empty command: " + inputCommand);

			return BuildCommand(lineTokens, startLine);
		}

	}
}