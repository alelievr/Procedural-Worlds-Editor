using UnityEngine;

namespace PW
{
	[System.SerializableAttribute]
	public struct SerializableColor
	{
		[SerializeField]
		float		r;
		[SerializeField]
		float		g;
		[SerializeField]
		float		b;
		[SerializeField]
		float		a;

		public SerializableColor(Color c)
		{
			r = c.r;
			g = c.g;
			b = c.b;
			a = c.a;
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
	}
}