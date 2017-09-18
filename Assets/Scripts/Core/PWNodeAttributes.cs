using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Core
{
	[AttributeUsage(AttributeTargets.Field)]
	public class PWInputAttribute : Attribute
	{
		public string	name = null;
		
		public PWInputAttribute()
		{
		}
		
		public PWInputAttribute(string fieldName)
		{
			name = fieldName;
		}
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class PWOutputAttribute : Attribute
	{
		public string	name = null;

		public PWOutputAttribute()
		{
		}
		
		public PWOutputAttribute(string fieldName)
		{
			name = fieldName;
		}
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class PWOffsetAttribute : Attribute
	{
		public int		offset;
		public int		multiPadding = 0;
		
		public PWOffsetAttribute(int y, int multiPadding)
		{
			offset = y;
			this.multiPadding = multiPadding;
		}

		public PWOffsetAttribute(int y)
		{
			offset = y;
		}
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class PWColorAttribute : Attribute
	{
		public Color		color;

		public PWColorAttribute(float r, float g, float b)
		{
			color.r = r * .8f;
			color.g = g * .8f;
			color.b = b * .8f;
			color.a = 1;
		}

		public PWColorAttribute(float r, float g, float b, float a)
		{
			color.r = r;
			color.g = g;
			color.b = b;
			color.a = a;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class PWMultipleAttribute : Attribute
	{
		public SerializableType[]	allowedTypes;
		public int		minValues;
		public int		maxValues;
		
		public PWMultipleAttribute(int min, int max, params Type[] allowedTypes)
		{
			this.allowedTypes = allowedTypes.Cast< SerializableType >().ToArray();
			minValues = min;
			maxValues = max;
		}

		public PWMultipleAttribute(int min, params Type[] allowedTypes)
		{
			List< SerializableType > ts = new List< SerializableType >();
			foreach (var t in allowedTypes)
				ts.Add((SerializableType)t);
			this.allowedTypes = ts.ToArray();
			minValues = min;
			maxValues = 100;
		}
		
		public PWMultipleAttribute(params Type[] allowedTypes)
		{
			List< SerializableType > ts = new List< SerializableType >();
			foreach (var t in allowedTypes)
				ts.Add((SerializableType)t);
			this.allowedTypes = ts.ToArray();
			minValues = 0;
			maxValues = 100;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class PWGenericAttribute : Attribute
	{
		public SerializableType[]	allowedTypes;

		public PWGenericAttribute(params Type[] allowedTypes)
		{
			this.allowedTypes = new SerializableType[allowedTypes.Length];
			for (int i = 0; i < allowedTypes.Length; i++)
				this.allowedTypes[i] = (SerializableType)allowedTypes[i];
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class PWNotRequiredAttribute : Attribute
	{
		public PWNotRequiredAttribute() {}
	}

	//Copy attribute tell to the graph to try to copy the field when
	//	it is transfered from a the linked input anchor to this field.
	//	by default, values are passed by references.
	//	Can be used only on a PWInput field.
	[AttributeUsage(AttributeTargets.Field)]
	public class PWCopyAttribute : Attribute
	{
		public PWCopyAttribute() {}
	}
}
