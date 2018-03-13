using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Core
{
	[AttributeUsage(AttributeTargets.Field)]
	public class InputAttribute : Attribute
	{
		public string	name = null;
		
		public InputAttribute()
		{
		}
		
		public InputAttribute(string fieldName)
		{
			name = fieldName;
		}
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class OutputAttribute : Attribute
	{
		public string	name = null;

		public OutputAttribute()
		{
		}
		
		public OutputAttribute(string fieldName)
		{
			name = fieldName;
		}
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class OffsetAttribute : Attribute
	{
		public int		offset;
		public int		padding = 0;
		
		public OffsetAttribute(int y, int padding)
		{
			offset = y;
			this.padding = padding;
		}

		public OffsetAttribute(int y)
		{
			offset = y;
		}
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class ColorAttribute : Attribute
	{
		public Color		color;

		public ColorAttribute(float r, float g, float b)
		{
			color.r = r * .8f;
			color.g = g * .8f;
			color.b = b * .8f;
			color.a = 1;
		}

		public ColorAttribute(float r, float g, float b, float a)
		{
			color.r = r;
			color.g = g;
			color.b = b;
			color.a = a;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class NotRequiredAttribute : Attribute
	{
	}
}
