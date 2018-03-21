using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System;
using System.Text;
using UnityEngine;
using ProceduralWorlds.Core;

public static class Jsonizer
{
	static readonly bool debug = false;

    public class JsonType
    {
        public Type     type;
        public string   prefix;
        public string   sufix;
        public Regex    regex;
        public Func< string, object >	parser;
    }

	public class JsonTypes : List< JsonType >
	{
		public void Add(Type t, string prefix, string sufix, Regex regex, Func< string, object > parser)
		{
			JsonType jsonType = new JsonType();

			jsonType.type = t;
			jsonType.prefix = prefix;
			jsonType.sufix = sufix;
			jsonType.regex = regex;
			jsonType.parser = parser;

			this.Add(jsonType);
		}
		
		public void Add(Type t, Regex regex, Func< string, object > parser)
		{
			JsonType jsonType = new JsonType();

			jsonType.type = t;
			jsonType.prefix = "";
			jsonType.sufix = "";
			jsonType.regex = regex;
			jsonType.parser = parser;
			
			this.Add(jsonType);
		}
	}

	static string intRegex = @"[-+]?\d+";
	static string floatRegex = @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+|f?)?";
	static string vector2Regex = @"\(\s*" + floatRegex + @"\s*,\s*" + floatRegex + @"\)";
	static string texture2DRegex = @"{\s*Texture2D\s*:\s*(\w+)}";

	public static readonly JsonTypes allowedJsonTypes = new JsonTypes {
		{typeof(string), "\"", "\"", new Regex("^\".*\"$"), (val) => val.Trim('"') },
		{typeof(int), new Regex(@"^" + intRegex + "$"), (val) => int.Parse(val)},
		{typeof(long), new Regex(@"^" + intRegex + "$"), (val) => long.Parse(val)},
		{typeof(float), new Regex("^" + floatRegex + "$"), (val) => float.Parse(val) },
		{typeof(double), new Regex("^" + floatRegex + "$"), (val) => double.Parse(val) },
		{typeof(Vector2), new Regex("^" + vector2Regex + "$"),
			(val) => {
				//Arrrrrg i hate string manipulation in C#
				MatchCollection mc = Regex.Matches(val, @"\(\s*(.*)\s*,\s*(.*)\s*\)");
				var groups = mc[0].Groups;
				float f1 = float.Parse(groups[1].Value);
				float f2 = float.Parse(groups[2].Value);
				return new Vector2(f1, f2);
			}
		},
		{typeof(Texture2D), "{Texture2D:", "}", new Regex(texture2DRegex), (val) => Resources.Load< Texture2D >(val) }
	};

    static string Jsonify(object data)
    {
        Type t = data.GetType();

        if (allowedJsonTypes.FindIndex(a => a.type == t) == -1)
            throw new InvalidOperationException("[Jsonizer] Can't jsonify type '" + t + "'");

        if (t == typeof(string))
            return "\"" + (data as string) + "\"";
        else
            return data.ToString();
    }

    public static string Generate(IEnumerable< Pair< string, object > > datas)
    {
		StringBuilder sb = new StringBuilder();
		sb.Append("{");

        if (datas == null || datas.Count() == 0)
            throw new InvalidOperationException("[Jsonizer] Null datas sent to Json generator");

        var last = datas.Last();
        foreach (var data in datas)
        {
			sb.Append(data.first);
			sb.Append(":");
			sb.Append(Jsonify(data.second));
            if (data != last)
				sb.Append(", ");
        }

		sb.Append("}");
        return sb.ToString();
    }

	static Regex nameRegex = new Regex("^(\\w{1,}|\"\\w{1,}\")");
	static Regex separatorRegex = new Regex(@"\s*^:\s*");

	static Pair< string, object > ParsePart(string part)
	{
		Match nameMatch = nameRegex.Match(part);

		if (!nameMatch.Success)
			throw new InvalidOperationException("[Jsonizer] Parse error near '" + part + "'");
		
		string name = nameMatch.Value.Replace("\"", String.Empty);

		part = part.Substring(nameMatch.Value.Length);

		Match separatorMatch = separatorRegex.Match(part);

		if (!separatorMatch.Success)
			throw new InvalidOperationException("[Jsonizer] Parse error near '" + part + "'");
			
		part = part.Substring(separatorMatch.Value.Length).Trim();

		object obj = null;

		foreach (var allowedType in allowedJsonTypes)
		{
			if (debug)
				Debug.Log("part: [" + part + "], match '" + allowedType.regex + "': " + allowedType.regex.Match(part).Success);
			
			if (allowedType.regex.Match(part).Success)
			{
				 obj = allowedType.parser(part);
				 break ;
			}
		}

		if (obj == null)
			throw new InvalidOperationException("[Jsonizer] Parse error near '" + part + "'");

		return new Pair< string, object >(name, obj);
	}

    public static Pairs< string, object > Parse(string s)
    {
        var ret = new Pairs< string, object >();

		s = s.Trim();

		//check for enclosing brackets
		if (!Regex.Match(s, @"^{.*}$").Success)
			throw new InvalidOperationException("[Jsonizer] Bad json format while parsing '" + s + "'");
		
		//remove enclosing brackets
		s = s.Substring(1);
		s = s.Substring(0, s.Length - 1);

		//magic regex to parse exluding commas
		var datas = Regex.Matches(s, @"([\""].+?[\""]|\S+)\s*:\s*([\(].+?[\)]|[^,]+)")
            .Cast< Match >()
            .Select(m => m.Value);

		foreach (var data in datas)
			ret.Add(ParsePart(data.Trim()));

        return ret;
    }
}