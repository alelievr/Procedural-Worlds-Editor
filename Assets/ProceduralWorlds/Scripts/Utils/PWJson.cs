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
        public Regex    matchingRegex;
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
			jsonType.matchingRegex = regex;
			jsonType.parser = parser;
		}
	}

	static JsonTypes allowedJsonTypes = new JsonTypes() {
		{typeof(string), "\"", "\"", new Regex("^\".*\"$"), (val) => { return val.Trim('"'); }},
	};

    // static List< Type > allowedJsonTypes = new List< Type >() {
        // typeof(string), typeof(int), typeof(float), typeof(double),
        // typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(Texture2D),
        // typeof(Material)
    // };

    static string Jsonify(object data)
    {
        Type t = data.GetType();

        // if (!allowedJsonTypes.Contains(t))
            // throw new Exception("[PWJson] Can't jsonify type '" + t + "'");

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
			throw new Exception("[PWJson] Parse error near " + part);
		
		string name = nameMatch.Value.Replace("\"", String.Empty);

		part = part.Substring(nameMatch.Value.Length);

		Match separatorMatch = separatorRegex.Match(part);

		if (!separatorMatch.Success)
			throw new Exception("[PWJson] Parse error near " + part);
			
		part = part.Substring(separatorMatch.Value.Length);



		return new Pair< string, object >(name, obj);
	}

    public static Pairs< string, object > Parse(string s)
    {
        var ret = new Pairs< string, object >();

		s = s.Trim();

		//check for enclosing brackets
		if (!Regex.Match(s, @"^{/.*}$").Success)
			throw new Exception("[PWJson] Bad json format while parsing '" + s + "'");
		
		//remove enclosing brackets
		s = s.Substring(1);
		s = s.Substring(0, s.Length - 1);

		var datas = s.Split(',');

		foreach (var data in datas)
		{
			var pair = ParsePart(data.Trim());
		}

        return ret;
    }
}