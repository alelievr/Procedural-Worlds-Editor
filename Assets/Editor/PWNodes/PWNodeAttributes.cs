using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW
{
	[AttributeUsage(AttributeTargets.Field)]
	public class PWInput : Attribute
	{
		public string	name = null;
		
		public PWInput()
		{
		}
		
		public PWInput(string fieldName)
		{
			name = fieldName;
		}
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class PWOutput : Attribute
	{
		public string	name = null;

		public PWOutput()
		{
		}
		
		public PWOutput(string fieldName)
		{
			name = fieldName;
		}
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class PWOffset : Attribute
	{
		public Vector2	offset;

		public PWOffset(int x, int y)
		{
			offset.x = x;
			offset.y = y;
		}
		
		public PWOffset(int y)
		{
			offset.x = 0;
			offset.y = y;
		}
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class PWColor : Attribute
	{
		public Color		color;

		public PWColor(float r, float g, float b)
		{
			color.r = r * .8f;
			color.g = g * .8f;
			color.b = b * .8f;
			color.a = 1;
		}

		public PWColor(float r, float g, float b, float a)
		{
			color.r = r;
			color.g = g;
			color.b = b;
			color.a = a;
		}
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class PWMultiple : Attribute
	{
		public Type[]	allowedTypes;
		public int		minValues;
		public int		maxValues;
		
		public PWMultiple(int min, int max, params Type[] allowedTypes)
		{
			this.allowedTypes = allowedTypes;
			minValues = min;
			maxValues = max;
		}

		public PWMultiple(int min, params Type[] allowedTypes)
		{
			this.allowedTypes = allowedTypes;
			minValues = min;
			maxValues = 100;
		}
		
		public PWMultiple(params Type[] allowedTypes)
		{
			this.allowedTypes = allowedTypes;
			minValues = 0;
			maxValues = 100;
		}
	}
}