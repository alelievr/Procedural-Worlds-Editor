using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

namespace PW.Core
{
	//Command line interpreter for PWGraph
	public class PWGraphCLI {

		enum PWGraphToken
		{
			Undefined,
			NewNodeCommand,
			LinkCommand,
			To,
			OpenParenthesis,
			ClosedParenthesis,
			Comma,
			At,
			As,
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

		List< PWGraphTokenDefinition >	tokenDefinitions = new List< PWGraphTokenDefinition >()
		{
			new PWGraphTokenDefinition(PWGraphToken.ClosedParenthesis, @"^\)"),
			new PWGraphTokenDefinition(PWGraphToken.To, @"^to"),
			new PWGraphTokenDefinition(PWGraphToken.LinkCommand, @"^Link"),
			new PWGraphTokenDefinition(PWGraphToken.As, @"^as"),
			new PWGraphTokenDefinition(PWGraphToken.NewNodeCommand, @"^NewNode"),
			new PWGraphTokenDefinition(PWGraphToken.NodeTypeName, @"^\S+"),
			new PWGraphTokenDefinition(PWGraphToken.NodeTypeName, @"^,"),
			new PWGraphTokenDefinition(PWGraphToken.NameValue, "^(\\\"[^']*\\\"|\\S+)"),
			new PWGraphTokenDefinition(PWGraphToken.OpenParenthesis, @"^\("),
			new PWGraphTokenDefinition(PWGraphToken.At, @"^at"),
			new PWGraphTokenDefinition(PWGraphToken.IntValue, @"^\d+"),
		};

		//TODO: better things
		List< List< PWGraphToken > >	validTokenLines = new List< List< PWGraphToken > >()
		{
			new List< PWGraphToken >()
				{ PWGraphToken.NewNodeCommand, PWGraphToken.NodeTypeName, PWGraphToken.As, PWGraphToken.NameValue },
			new List< PWGraphToken >()
				{ PWGraphToken.NewNodeCommand, PWGraphToken.NodeTypeName, PWGraphToken.At, PWGraphToken.OpenParenthesis, PWGraphToken.IntValue, PWGraphToken.Comma, PWGraphToken.IntValue, PWGraphToken.ClosedParenthesis, PWGraphToken.As, PWGraphToken.NameValue },
			new List< PWGraphToken >()
				{ PWGraphToken.NewNodeCommand, PWGraphToken.NodeTypeName, PWGraphToken.As, PWGraphToken.NameValue, PWGraphToken.At, PWGraphToken.OpenParenthesis, PWGraphToken.IntValue, PWGraphToken.Comma, PWGraphToken.IntValue, PWGraphToken.ClosedParenthesis },
			new List< PWGraphToken >()
				{ PWGraphToken.LinkCommand, PWGraphToken.NameValue, PWGraphToken.To, PWGraphToken.NameValue},
		};

		PWGraphTokenMatch	Match(ref string line)
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

		PWGraphCommand	BuildCommand(List< PWGraphTokenMatch > tokens, string startLine)
		{
			PWGraphCommand	ret = null;

			foreach (var validTokenList in validTokenLines)
			{
				if (tokens.Count != validTokenList.Count)
					continue ;
				
				for (int i = 0; i < validTokenList.Count; i++)
				{
					if (tokens[i].token != validTokenList[i])
						break ;
					else if (i == validTokenList.Count - 1)
					{
						ret = new PWGraphCommand();
					}
				}
			}

			if (ret == null)
				throw new Exception("Invalid token line");
			
			return ret;
		}

		public PWGraphCommand Parse(string inputLine)
		{
			PWGraphTokenMatch			match;
			List< PWGraphTokenMatch >	lineTokens = new List< PWGraphTokenMatch >();
			string						startLine = inputLine;

			while (!String.IsNullOrEmpty(inputLine))
			{
				match = Match(ref inputLine);

				if (match == null)
					throw new Exception("Invalid token at line \"" + inputLine + "\"");

				lineTokens.Add(match);
			}

			return BuildCommand(lineTokens, startLine);
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