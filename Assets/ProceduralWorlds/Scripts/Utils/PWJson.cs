using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine;
using PW.Core;

public static class PWJson
{
    class JsonType
    {
        public Type     type;
        public string   prefix;
        public string   sufix;
        public Regex    regex;
        public Func< string, object >	parser;
    }

	class JsonTypes : List< JsonType >
	{
		public void Add(Type t, string prefix, string sufix, Regex regex, Func< string, object > parser)
		{
			JsonType jsonType = new JsonType();

			jsonType.type = t;
			jsonType.prefix = prefix;
			jsonType.sufix = sufix;
			jsonType.regex = regex;
			jsonType.parser = parser;
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
	static string vector2Regex = @"(\s*" + floatRegex + @"\s*,\s*" + floatRegex + ")";
	static string texture2DRegex = @"{\s*Texture2D\s*:\s*(\w+)}";

	static JsonTypes allowedJsonTypes = new JsonTypes() {
		{typeof(string), "\"", "\"", new Regex("^\".*\"$"), (val) => val.Trim('"') },
		{typeof(int), new Regex(@"^" + intRegex + "$"), (val) => int.Parse(val)},
		{typeof(float), new Regex("^" + floatRegex + "$"), (val) => float.Parse(val) },
		{typeof(Vector2), new Regex("^" + vector2Regex + "$"),
			(val) => {
				//Arrrrrg i hate string manipulation in C#
				MatchCollection mc = new Regex(@"\(\s*(.*)\s*,\s*(.*)\s*\)").Matches(val);
				float f1 = float.Parse(mc[1].Value);
				float f2 = float.Parse(mc[2].Value);
				return new Vector2(f1, f2);
			}
		},
		{typeof(Texture2D), "{Texture2D:", "}", new Regex(texture2DRegex), (val) => Resources.Load< Texture2D >(val) }
	};

    static string Jsonify(object data)
    {
        Type t = data.GetType();

        if (allowedJsonTypes.Count(a => a.type == t) == 0)
            throw new Exception("[PWJson] Can't jsonify type '" + t + "'");

        if (t == typeof(string))
            return "\"" + (data as string) + "\"";
        else
            return data.ToString();
    }

    public static string Generate(IEnumerable< Pair< string, object > > datas)
    {
        string  ret = "{ ";

        if (datas == null || datas.Count() == 0)
            throw new Exception("[PWJson] Null datas sent to Json generator");

        var last = datas.Last();
        foreach (var data in datas)
        {
            ret += data.first + ": ";
            ret += Jsonify(data.second);
            if (data != last)
                ret += ", ";
        }

        return ret + " }";
    }

	static Regex nameRegex = new Regex("^(\\w{1,}|\"\\w{1,}\")");
	static Regex separatorRegex = new Regex(@"\s*^:\s*");

	static Pair< string, object > ParsePart(string part)
	{
		Match nameMatch = nameRegex.Match(part);

		if (!nameMatch.Success)
			throw new Exception("[PWJson] Parse error near '" + part + "'");
		
		string name = nameMatch.Value.Replace("\"", String.Empty);

		part = part.Substring(nameMatch.Value.Length);

		Match separatorMatch = separatorRegex.Match(part);

		if (!separatorMatch.Success)
			throw new Exception("[PWJson] Parse error near '" + part + "'");
			
		part = part.Substring(separatorMatch.Value.Length).Trim();

		object obj = null;

		foreach (var allowedType in allowedJsonTypes)
		{
			// Debug.Log("part: [" + part + "], match '" + allowedType.regex + "': " + allowedType.regex.Match(part).Success);
			if (allowedType.regex.Match(part).Success)
			{
				 obj = allowedType.parser(part);
				 break ;
			}
		}

		if (obj == null)
			throw new Exception("[PWJson] Parse error near '" + part + "'");

		return new Pair< string, object >(name, obj);
	}

    public static Pairs< string, object > Parse(string s)
    {
        var ret = new Pairs< string, object >();

		s = s.Trim();

		//check for enclosing brackets
		if (!Regex.Match(s, @"^{.*}$").Success)
			throw new Exception("[PWJson] Bad json format while parsing '" + s + "'");
		
		//remove enclosing brackets
		s = s.Substring(1);
		s = s.Substring(0, s.Length - 1);

		var datas = s.Split(',');

		foreach (var data in datas)
			ret.Add(ParsePart(data.Trim()));

        return ret;
    }
}