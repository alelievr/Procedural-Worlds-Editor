using UnityEngine;
using System;

namespace ProceduralWorlds
{
	[System.SerializableAttribute]
	public struct SerializableColor : IEquatable< SerializableColor >
	{
		[SerializeField]
		public float		r;
		[SerializeField]
		public float		g;
		[SerializeField]
		public float		b;
		[SerializeField]
		public float		a;

		public SerializableColor(Color c)
		{
			r = c.r;
			g = c.g;
			b = c.b;
			a = c.a;
		}

		public SerializableColor(float r, float g, float b, float a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public SerializableColor(Color32 c)
		{
			r = c.r / 255f;
			g = c.g / 255f;
			b = c.b / 255f;
			a = c.a / 255f;
		}

		public Color GetColor()
		{
			return new Color(r, g, b, a);
		}

		public static explicit operator SerializableColor(Color c)
		{
			return new SerializableColor(c);
		}

		public static implicit operator Color(SerializableColor c)
		{
			return c.GetColor();
		}

		public override string ToString()
		{
			return "r: " + r + ", g: " + g + ", b: " + b + ", a: " + a;
		}

		public bool Equals(SerializableColor other)
		{
			return r == other.r
				&& g == other.g
				&& b == other.b
				&& a == other.a;
		}
	}
}