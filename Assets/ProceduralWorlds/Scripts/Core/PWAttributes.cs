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
		public int		padding = 0;
		
		public PWOffsetAttribute(int y, int padding)
		{
			offset = y;
			this.padding = padding;
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
