using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine;
using PW.Core;

public static class PWJson
{
    static List< Type > allowedJsonTypes = new List< Type >() {
        typeof(string), typeof(int), typeof(float), typeof(double),
        typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(Texture2D),
        typeof(Material)
    };

    static string Jsonify(object data)
    {
        Type t = data.GetType();

        if (!allowedJsonTypes.Contains(t))
            throw new Exception("Can't jsonify type '" + t + "'");

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
            if (data != last)
                ret += Jsonify(data.second);
        }

        return ret + " }";
    }

    public static List< Pair< string, object > > Parse(string s)
    {
        return null;
    }
}