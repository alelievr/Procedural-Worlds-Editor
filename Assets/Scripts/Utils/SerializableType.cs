using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.SerializableAttribute]
public class SerializableType {

	[SerializeField]
	public string	typeString;

	public SerializableType(Type t)
	{
		SetType(t);
	}
	
	public new Type GetType()
	{
		return Type.GetType(typeString);
	}

	public void SetType(Type type)
	{
		if (type != null)
			typeString = type.FullName + ", " + type.Assembly.GetName().Name;
	}

	public static implicit operator Type(SerializableType st)
	{
		return Type.GetType(st.typeString);
	}

	public static explicit operator SerializableType(Type t)
	{
		return new SerializableType(t);
	}

	public static bool operator==(SerializableType st1, SerializableType st2)
	{
		return st1.typeString == st2.typeString;
	}
	
	public static bool operator!=(SerializableType st1, SerializableType st2)
	{
		return st1.typeString != st2.typeString;
	}

	public override bool Equals(object st)
	{
		return ((SerializableType)st).typeString == typeString;
	}

	public override int GetHashCode()
    {
    	return typeString.GetHashCode();
    }

}
